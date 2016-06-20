using System.ComponentModel.DataAnnotations;

namespace News.Models
{
    public class Category
    {
        [Required(ErrorMessage = "Enter category's title")]
        public string Title { get; set; }
        [Key]
        [Required(ErrorMessage = "Enter category's title")]
        public string Keyword { get; set; }

        public string Description { get; set; }
        public string Keywords { get; set; }
    }
}