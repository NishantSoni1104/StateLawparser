using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCsvParser.Context;
using WebCsvParser.Filter;
using WebCsvParser.ViewModels;

namespace WebCsvParser.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [GenerateAntiforgeryTokenCookieForAjax]
        public IActionResult Index()
        
        
        {
            return View(_context.DataFile
                .AsNoTracking()
                .Select(i => new DataFileViewModel
                {
                    FileName = i.FileName,
                    FilePath = i.FilePath,
                    Id = i.Id
                })
                .ToList());
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
