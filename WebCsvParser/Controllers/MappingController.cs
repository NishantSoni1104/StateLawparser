using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebCsvParser.Context;
using WebCsvParser.Models;
using WebCsvParser.ViewModels;

namespace WebCsvParser.Controllers
{
    [Route("mapping")]
    public class MappingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MappingController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Call this method when all mapping are already mapped
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            var models = (from mapping in _context.Mapping
                          join category in _context.Category on mapping.CategoryId equals category.Id into categoryResults
                          from cat in categoryResults.DefaultIfEmpty()
                          join lineItem in _context.LineItem on mapping.LineItemId equals lineItem.Id into lineItemResults
                          from lt in lineItemResults.DefaultIfEmpty()
                          where !string.IsNullOrEmpty(mapping.Description)
                          select new LineItemViewModel
                          {
                              Category = cat.Name,
                              Name = lt.Name,
                              Id = mapping.Id,
                              Description = mapping.Description,
                              LineNumber = lt.LineNumber
                          })
                        .ToList();

            return Json(new
            {
                data = models
            });
        }

        /// <summary>
        /// Get all filtered mapping
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        [HttpGet("category")]
        public IActionResult GetAllByCategory([FromQuery]string category)
        {
            var models = (from tempData in _context.TempData
                          join c in _context.Category on tempData.Category equals c.Name into categoryResults
                          from cat in categoryResults.DefaultIfEmpty()
                          join lineItem in _context.LineItem on tempData.Name equals lineItem.Name into lineItemResults
                          from lt in lineItemResults.DefaultIfEmpty()
                          join mapping in _context.Mapping on new { CategoryId = cat.Id, LineItemId = lt.Id } equals
                              new { mapping.CategoryId, mapping.LineItemId } into mappingResults
                          from m in mappingResults.DefaultIfEmpty()
                          where !string.IsNullOrEmpty(m.Description) && cat.Name == category
                          select new LineItemViewModel
                          {
                              Category = cat.Name,
                              Name = lt.Name,
                              Id = m.Id,
                              Description = m.Description,
                              LineNumber = tempData.LineNumber
                          })
                .ToList();

            return Json(new
            {
                data = models
            });
        }

        [HttpPost]
        public IActionResult Post([FromBody]CategoryViewModel model)
        {
            if (!_context.Category
                .Any(i => i.Name == model.Category))
            {
                _context.Category.Add(new Category
                {
                    Name = model.Category
                });
                _context.SaveChanges();
            }

            var categoryId = _context.Category
                .Where(i => i.Name == model.Category)
                .Select(i => i.Id)
                .Single();

            if (!_context.LineItem
                .Any(i => i.Name == model.Name && i.LineNumber == model.LineNumber))
            {
                _context.LineItem.Add(new LineItem
                {
                    Name = model.Name,
                    LineNumber = model.LineNumber
                });
                _context.SaveChanges();
            }

            var lineItemId = _context.LineItem
                .Where(i => i.Name == model.Name && i.LineNumber == model.LineNumber)
                .Select(i => i.Id)
                .Single();

            if (!_context.Mapping
                .Any(i => i.CategoryId == categoryId && i.LineItemId == lineItemId))
            {
                _context.Mapping.Add(new Mapping
                {
                    CategoryId = categoryId,
                    Description = model.Comments,
                    LineItemId = lineItemId
                });
                _context.SaveChanges();
            }

            return Ok();
        }

        [HttpPut]
        public IActionResult Put([FromBody] CategoryViewModel model)
        {
            var categoryId = _context.Category
                .Where(i => i.Name == model.Category)
                .Select(i => i.Id)
                .SingleOrDefault();

            if (categoryId == 0)
            {
                var newCategory = new Category
                {
                    Name = model.Category
                };
                _context.Category.Add(newCategory);

                _context.SaveChanges();

                categoryId = newCategory.Id;

            }

            var lineItemId = _context.LineItem
                .Where(i => i.Name == model.LineItem)
                .Select(i => i.Id)
                .SingleOrDefault();

            if (lineItemId == 0)
            {
                var newLineItem = new LineItem
                {
                    Name = model.Category
                };
                _context.LineItem.Add(newLineItem);

                _context.SaveChanges();

                lineItemId = newLineItem.Id;
            }

            var mapping = _context.Mapping.Find(model.Id);

            mapping.Description = model.Description;
            mapping.CategoryId = categoryId;
            mapping.LineItemId = lineItemId;

            _context.Update(mapping);
            _context.SaveChanges();
            return Ok();
        }

        [HttpDelete]
        public IActionResult Delete([FromQuery] long id)
        {
            var mapping = _context.Mapping.Find(id);
            _context.Remove(mapping);
            _context.SaveChanges();
            return Ok();
        }

        [HttpGet("description")]
        public IActionResult Get(string category, string name)
        {
            return Json(_context.Mapping
                .Where(i => i.Category.Name == category && i.LineItem.Name == name)
                .Select(i => i.Description)
                .SingleOrDefault());
        }

        [HttpGet("description/mapped")]
        public IActionResult IsDescriptionMapped()
        {
            var countTempData = _context.TempData.Count();
            var countMapped = _context.Mapping.Count();

            return Json(countTempData > 0 && countTempData <= countMapped);
        }

        [HttpGet("all/category")]
        public IActionResult GetAllLineNam([FromQuery] string category)
        {
            return Json(new SelectList(_context.Mapping
                .Where(i => i.Category.Name == category)
                .Select(i => i.LineItem.Name)
                .ToList()));
        }
    }
}
