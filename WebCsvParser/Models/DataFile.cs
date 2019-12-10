using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebCsvParser.Models
{
    public class DataFile
    {
        public int Id { get; set; }
        [Required]
        public string FileName { get; set; }
        [Required]
        public string FilePath { get; set; }

        public virtual List<TempData> LineItems { get; set; }
    }
}
