using Microsoft.AspNetCore.Mvc;
using WebCsvParser.Context;

namespace WebCsvParser.Controllers
{
    [Route("lineitem")]
    public class LineItemController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LineItemController(ApplicationDbContext context)
        {
            _context = context;
        }        
    }
}
