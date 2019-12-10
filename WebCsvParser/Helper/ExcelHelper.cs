using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using WebCsvParser.Models;

namespace WebCsvParser.Helper
{
    public static class ExcelHelper
    {
        public static (List<TempData> listItems, List<ErrorList> errorList) ConvertCsVtoDataTable(string strFilePath, int dataFileId)
        {
            var lstLineItems = new List<TempData>();
            var errorList = new List<ErrorList>();
            var category = string.Empty;
            var excelLineNumber = 1;
            try
            {
                using (var sr = new StreamReader(strFilePath))
                {
                    while (!sr.EndOfStream)
                    {
                        var stringData = sr.ReadLine();
                        try
                        {
                            if (!string.IsNullOrEmpty(stringData))
                            {
                                if (stringData.Contains('"'))
                                {
                                    stringData = stringData.Replace("\"", "");
                                }

                                if (stringData.Contains("...")) {
                                    excelLineNumber++;
                                    continue;
                                }
                                if (stringData.Contains("****"))
                                {
                                    excelLineNumber++;
                                    continue;
                                }

                                if (stringData.All(i => i == '.'))
                                {
                                    excelLineNumber++;
                                    continue;
                                }

                                stringData = stringData.TrimEnd(new char[] { ',', '.' });

                                if (stringData.StartsWith('#'))
                                {
                                    errorList.Add(new ErrorList
                                    {
                                        LineNumber = excelLineNumber,
                                        Message = "Cell start with =",
                                        Property = stringData
                                    });
                                }

                                if (!stringData.All(i => char.IsLetter(i) || char.IsUpper(i) || char.IsWhiteSpace(i) || char.IsPunctuation(i)))
                                {
                                    if (!char.IsDigit(stringData[0]))
                                    {
                                        errorList.Add(new ErrorList
                                        {
                                            LineNumber = excelLineNumber,
                                            Message = "Line number is missing",
                                            Property = stringData
                                        });
                                    }
                                    else if (!double.TryParse(stringData.Substring(0, 3), out _))
                                    {
                                        errorList.Add(new ErrorList
                                        {
                                            LineNumber = excelLineNumber,
                                            Message = "Linenumber format is not valid",
                                            Property = stringData
                                        });
                                    }
                                }

                                if (stringData.Any(i => char.IsNumber(i) || char.IsSymbol(i)))
                                {
                                    var commaIndex = stringData.IndexOf('.');
                                    var isLineNumber = int.TryParse(Regex.Match(stringData.Substring(0, commaIndex), @"\d+").Value, out _);
                                    if (!isLineNumber)
                                    {
                                        errorList.Add(new ErrorList
                                        {
                                            LineNumber = excelLineNumber,
                                            Message = "Invalid category",
                                            Property = stringData
                                        });
                                    }
                                }

                                if (!stringData.Any(char.IsDigit) && stringData.All(i => char.IsLetter(i) || char.IsWhiteSpace(i) || char.IsPunctuation(i)))
                                {
                                    if (stringData != "#NAME?")
                                        category = stringData;
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(stringData))
                                    {
                                        var dr = new TempData { Category = category };

                                        var commaIndex = stringData.IndexOf('.');
                                        var isLineNumber = int.TryParse(Regex.Match(stringData.Substring(0, commaIndex), @"\d+").Value, out var lineNumber);
                                        dr.LineNumber = isLineNumber ? (short?)lineNumber : null;

                                        var data = stringData.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);

                                        if (!string.IsNullOrWhiteSpace(data[1]))
                                        {
                                            if (data.Length > 2)
                                            {
                                                var trim = data[1].Substring(0, data[1].Length - 1).Trim(new char[] { ' ', '{', '}' });
                                                dr.Name = trim;
                                                //var lastSpaceIndex = trim.LastIndexOf(' ');
                                                //if (char.IsLetter(stringData[lastSpaceIndex + 1]))
                                                //{
                                                //    dr.Name = trim;
                                                //}
                                                //else
                                                //{
                                                //    //if (stringData[lastSpaceIndex + 2] == '.')
                                                //    //    dr.Name = trim.Substring(0, lastSpaceIndex -1);
                                                //    //else
                                                //    dr.Name = trim.Substring(0, lastSpaceIndex);
                                                //}


                                                var spaceSplitedRows = stringData.Split(' ');
                                                if (spaceSplitedRows.Length > 2)
                                                {
                                                    var hasPrice = double.TryParse(spaceSplitedRows[spaceSplitedRows.Length - 2], out var quantity);
                                                    double.TryParse(spaceSplitedRows[spaceSplitedRows.Length - 1], out var price);
                                                    if (hasPrice)
                                                    {
                                                        dr.Quantity = quantity;
                                                        dr.Price = price;
                                                    }
                                                    else
                                                    {
                                                        // here price is actually quantity
                                                        dr.Quantity = price;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                dr.Name = data[1].Trim();
                                            }
                                        }
                                        else
                                        {
                                            dr.Name = stringData.Substring(stringData.IndexOf('.', stringData.IndexOf('.') + 1));
                                        }

                                        if (dr.Name.Contains("..."))
                                        {
                                            excelLineNumber++;
                                            continue;
                                        }

                                        dr.DataFileId = dataFileId;
                                        lstLineItems.Add(dr);
                                    }

                                }

                            }


                            excelLineNumber++;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return (lstLineItems, errorList);
            }

            return (lstLineItems, errorList);
        }

    }
}
