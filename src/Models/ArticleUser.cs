using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace News.Models
{
    public class ArticleUser
    {
        [Required(ErrorMessage = "Enter title")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Enter content")]
        [AllowHtml]
        public string Content { get; set; }
        [Required(ErrorMessage = "Choose category")]
        public string Category { get; set; }
        public List<AdditionalField> AdditionalFields { get; set; }
    }
}