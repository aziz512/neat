namespace News.Models
{
    public class FieldValue
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public string FieldName { get; set; }
        public string Value { get; set; }
    }
}