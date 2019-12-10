using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebCsvParser.Context;

namespace WebCsvParser.Controllers
{
    [Route("category")]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Any category that is fully or partially mapped
        /// </summary>
        /// <returns></returns>
        [HttpGet("all/mapped")]
        public JsonResult GetAllMappedCategory() => new JsonResult(new SelectList(_context.Category
            .Select(i => i.Name)));


        /// <summary>
        /// All categories from temp data
        /// </summary>
        /// <returns></returns>
        [HttpGet("all")]
        public JsonResult GetAllCategory() => new JsonResult(new SelectList(_context.TempData
            .Select(i => i.Category)
            .Distinct()));
    }
}
