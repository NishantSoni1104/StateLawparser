using System.ComponentModel.DataAnnotations;

namespace WebCsvParser.Models
{
    public class Mapping
    {
        public long Id { get; set; }
        [Required]
        public virtual int CategoryId { get; set; }
        [Required]
        public virtual int LineItemId { get; set; }
        [Required]
        [DataType("LONGTEXT")]
        public string Description { get; set; }

        public virtual Category Category { get; set; }
        public virtual LineItem LineItem { get; set; }
    }
}
