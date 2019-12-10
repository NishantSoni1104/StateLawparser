namespace WebCsvParser.ViewModels
{
    public class LineItemViewModel
    {
        public long? Id { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? LineNumber { get; set; }
        public string WordLineNumbers { get; set; }
    }
}
