using System.ComponentModel.DataAnnotations;

namespace WebCsvParser.Models
{
    public class TempData
    {
        public int Id { get; set; }
        [Required]
        [DataType("varchar(50)")]
        public string Category { get; set; }
        [Required]
        [DataType("smallint")]
        public int? LineNumber { get; set; }
        [Required]
        [DataType("varchar(200)")]
        public string Name { get; set; }
        [DataType("decimal(18,2)")]
        public double? Price { get; set; }
        [DataType("decimal(18,2)")]
        public double? Quantity { get; set; }

        public virtual int DataFileId { get; set; }
        public virtual DataFile DataFile { get; set; }
    }
}
