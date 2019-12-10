using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WebCsvParser.Context;

namespace WebCsvParser.Controllers
{
    [Route("errorList")]
    public class ErrorListController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ErrorListController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public JsonResult Get()
        {
            try
            {
                return new JsonResult(new
                {
                    data = _context.ErrorList
                        .OrderBy(i => i.LineNumber)
                        .ToList()
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
