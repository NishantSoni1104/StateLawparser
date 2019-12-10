using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using WebCsvParser.Context;
using WebCsvParser.Filter;
using WebCsvParser.Helper;
using WebCsvParser.Models;
using WebCsvParser.ViewModels;

namespace WebCsvParser.Controllers
{
    [Route("datafield")]
    public class DataFileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DataFileController> _logger;
        private readonly IHostingEnvironment _hostingEnvironment;

        // Get the default form options so that we can use them to set the default limits for
        // request body data
        private static readonly FormOptions DefaultFormOptions = new FormOptions();

        public DataFileController(ApplicationDbContext context, ILogger<DataFileController> logger, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ValidateAntiForgeryToken]
        [DisableFormValueModelBinding]
        [HttpPost("upload")]
        public async Task<IActionResult> Upload()
        {
            try
            {
                if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
                {
                    return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
                }

                // Used to accumulate all the form url encoded key value pairs in the 
                // request.
                var formAccumulator = new KeyValueAccumulator();

                var boundary = MultipartRequestHelper.GetBoundary(
                    MediaTypeHeaderValue.Parse(Request.ContentType),
                    DefaultFormOptions.MultipartBoundaryLengthLimit);
                var reader = new MultipartReader(boundary, HttpContext.Request.Body);

                var targetFileDirectory = Path.Combine(_hostingEnvironment.ContentRootPath, "Uploads");
                if (!Directory.Exists(targetFileDirectory))
                {
                    Directory.CreateDirectory(targetFileDirectory);
                }

                var section = await reader.ReadNextSectionAsync();
                while (section != null)
                {
                    var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);

                    if (hasContentDispositionHeader)
                    {
                        if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                        {
                            var fileName = HeaderUtilities.RemoveQuotes(contentDisposition.FileName).Value;
                            var filePath = Path.Combine(targetFileDirectory, fileName);

                            formAccumulator.Append("FileName", fileName);
                            formAccumulator.Append("FilePath", filePath);

                            using (var targetStream = System.IO.File.Create(filePath))
                            {
                                await section.Body.CopyToAsync(targetStream);

                                _logger.LogInformation($"Copied the uploaded file '{filePath}'");
                            }
                        }
                        else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                        {
                            // Content-Disposition: form-data; name="key"
                            //
                            // value

                            // Do not limit the key name length here because the 
                            // multipart headers length limit is already in effect.
                            var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name);
                            var encoding = MultipartRequestHelper.GetEncoding(section);
                            using (var streamReader = new StreamReader(
                                section.Body,
                                encoding,
                                detectEncodingFromByteOrderMarks: true,
                                bufferSize: 1024,
                                leaveOpen: true))
                            {
                                // The value length limit is enforced by MultipartBodyLengthLimit
                                var value = await streamReader.ReadToEndAsync();
                                if (string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                                {
                                    value = string.Empty;
                                }
                                formAccumulator.Append(key.Value, value);

                                if (formAccumulator.ValueCount > DefaultFormOptions.ValueCountLimit)
                                {
                                    throw new InvalidDataException($"Form key count limit {DefaultFormOptions.ValueCountLimit} exceeded.");
                                }
                            }
                        }
                    }

                    // Drains any remaining section body that has not been consumed and
                    // reads the headers for the next section.
                    section = await reader.ReadNextSectionAsync();
                }

                // Bind form data to a model
                var dataFile = new DataFile();
                var formValueProvider = new FormValueProvider(
                    BindingSource.Form,
                    new FormCollection(formAccumulator.GetResults()),
                    CultureInfo.CurrentCulture);

                var bindingSuccessful = await TryUpdateModelAsync(dataFile, prefix: "", valueProvider: formValueProvider);
                if (!bindingSuccessful)
                {
                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }
                }

                var dataFileId = _context.DataFile
                    .AsNoTracking()
                    .Where(el => el.FileName == dataFile.FileName || el.FilePath == dataFile.FilePath)
                    .Select(el => el.Id)
                    .SingleOrDefault();

                if (dataFileId == 0)
                {
                    _context.DataFile.Add(dataFile);
                    _context.SaveChanges();
                    dataFileId = dataFile.Id;
                }
                else
                {
                    var df = _context.DataFile.Find(dataFileId);
                    df.FilePath = dataFile.FilePath;
                    df.FileName = dataFile.FileName;
                }

                var (lineItems, errorList) = ExcelHelper.ConvertCsVtoDataTable(dataFile.FilePath, dataFileId);
                var listItems = lineItems;
                var errors = errorList;

                System.IO.File.Delete(dataFile.FilePath);

                if (errors != null && errors.Any())
                {
                    var conn = _context.Database.GetDbConnection();
                    const string query = @"DELETE FROM ERRORLIST;
                                           ALTER TABLE ERRORLIST AUTO_INCREMENT = 1";
                    conn.Execute(query);
                    _context.ErrorList.AddRange(errorList);
                    _context.SaveChanges();
                    return Json(1);
                }

                if (listItems != null && listItems.Any())
                {
                    var conn = _context.Database.GetDbConnection();
                    const string query = @"DELETE FROM TempData;
                                           ALTER TABLE TempData AUTO_INCREMENT = 1";
                    conn.Execute(query);

                    _context.TempData.AddRange(listItems);
                    _context.SaveChanges();

                    return RedirectToAction("IsDescriptionMapped", "Mapping");
                }

                return BadRequest();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpGet]
        public IActionResult CreateWord()
        {
            var fileName = _context.DataFile
                .Select(i => i.FileName)
                .First();

            var previousExtension = fileName.Split('.')[1];

            var wordFile = fileName.Replace(previousExtension, "docx");

            var targetFileDirectory = Path.Combine(_hostingEnvironment.ContentRootPath, "Uploads");
            if (!Directory.Exists(targetFileDirectory))
            {
                Directory.CreateDirectory(targetFileDirectory);
            }

            var filePath = Path.Combine(targetFileDirectory, wordFile);

            WordHelper.CreateWordprocessingDocument(filePath);

            var run = new RunFonts
            {
                Ascii = "Calibri"
            };
            var wp = new WordHelper("14.1%", "26.7%", "59.1%", "24", run);

            var lineItems = (from tempData in _context.TempData
                             join category in _context.Category on tempData.Category equals category.Name
                             join lineItem in _context.LineItem on tempData.Name equals lineItem.Name
                             join mapping in _context.Mapping on new { CategoryId = category.Id, LineItemId = lineItem.Id } equals new { mapping.CategoryId, mapping.LineItemId }
                             orderby mapping.LineItemId, lineItem.LineNumber
                             select new
                             {
                                 Category = category.Name,
                                 mapping.LineItem.Name,
                                 mapping.Description,
                                 mapping.LineItem.LineNumber,
                                 mapping.Id
                             })
                            .ToList();

            var model = (from lineItem in lineItems.GroupBy(i => new { i.Category, i.Description })
                         let lineNumbers = lineItem.Select(i => i.LineNumber)
                         let lineNames = lineItem.Select(i => i.Name)
                         select new LineItemViewModel
                         {
                             Category = lineItem.Key.Category,
                             Description = lineItem.Key.Description,
                             WordLineNumbers = string.Join(',', lineNumbers),
                             Name = string.Join(Environment.NewLine + Environment.NewLine, lineNames)
                         }).ToList();

            wp.AddTable(filePath, model);

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, MimeTypeHelper.GetContentType(filePath), wordFile);
        }
    }
}
