using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WebCsvParser.ViewModels
{
    public class DataFileViewModel
    {
        public int Id { get; set; }
        [Required]
        public string FileName { get; set; }
        [Required]
        public string FilePath { get; set; }
        [Required]
        public IFormFile Csv { get; set; }
    }
}
