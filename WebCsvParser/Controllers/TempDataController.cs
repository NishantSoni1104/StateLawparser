using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebCsvParser.Context;
using WebCsvParser.Models;
using WebCsvParser.ViewModels;

namespace WebCsvParser.Controllers
{
    [Route("tempData")]
    public class TempDataController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TempDataController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public JsonResult Get()
        {
            const string query = "SELECT * " +
                                 "FROM " +
                                 "( " +
                                 "	SELECT td.id, " +
                                 "		   td.Category, " +
                                 "		   td.Name, " +
                                 "		   m.Description, " +
                                 "         td.LineNumber" +
                                 "	FROM TempData AS td " +
                                 "	LEFT JOIN LineItem AS li ON td.Name = li.Name " +
                                 "	LEFT JOIN Category AS c ON td.Category = c.Name " +
                                 "	LEFT JOIN Mapping AS m ON m.CategoryId = c.Id AND m.LineItemId = li.Id " +
                                 ") as data " +
                                 "WHERE data.Description IS NULL ";

            var conn = _context.Database.GetDbConnection();
            var ab= new JsonResult(new
            {
                data = conn.Query<LineItemViewModel>(query)
            });
            return ab;
        }

        [HttpGet("category")]
        public JsonResult Get([FromQuery]string category)
        {
            const string query = "SELECT * " +
                                 "FROM " +
                                 "( " +
                                 "	SELECT m.id, " +
                                 "		   td.Category, " +
                                 "		   td.Name, " +
                                 "		   m.Description, " +
                                 "          td.LineNumber    ," +
                                 "		   (select CASE WHEN m.Description IS NOT NULL THEN 1 ELSE 0 END) AS 'IsMapped'  " +
                                 "	FROM TempData AS td " +
                                 "	LEFT JOIN LineItem AS li ON td.Name = li.Name " +
                                 "	LEFT JOIN Category AS c ON td.Category = c.Name " +
                                 "	LEFT JOIN Mapping AS m ON m.CategoryId = c.Id AND m.LineItemId = li.Id " +
                                 ") as data " +
                                 "WHERE data.Description IS NULL AND data.Category = @category";

            var conn = _context.Database.GetDbConnection();
            var ab= new JsonResult(new
            {
                data = conn.Query<LineItemViewModel>(query, new { category })
            });
            return ab;
        }

        [HttpDelete("id/{id}")]
        public async Task<IActionResult> Delete([FromRoute]int id)
        {
            var tempData = new TempData { Id = id };
            _context.TempData.Remove(tempData);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
