using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebCsvParser.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required]
        [DataType("varchar(50)")]
        public string Name { get; set; }

        public virtual List<Mapping> Mapping { get; set; }
    }
}
