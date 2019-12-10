using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebCsvParser.Models
{
    public class LineItem
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int? LineNumber { get; set; }

        public virtual List<Mapping> Mapping { get; set; }
    }
}
