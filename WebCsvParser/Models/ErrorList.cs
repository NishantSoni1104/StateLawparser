using System.ComponentModel.DataAnnotations;

namespace WebCsvParser.Models
{
    public class ErrorList
    {
        public int Id { get; set; }
        [Required]
        [DataType("LONGTEXT")]
        public string Message { get; set; }
        [Required]
        [DataType("varchar(200)")]
        public string Property { get; set; }
        [Required]
        public int LineNumber { get; set; }
    }
}
