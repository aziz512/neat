using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace News.Models
{
    public class Article
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Enter the title")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Enter author's username")]
        public string Author { get; set; }
        [Required(ErrorMessage = "Enter content of the article")]
        public string Content { get; set; }
        [Required(ErrorMessage = "Choose category")]
        public string Category { get; set; }
        [Required(ErrorMessage = "Choose a date")]
        public DateTime Date { get; set; }
        [Required]
        public bool Moderated { get; set; }


        [NotMapped]
        public bool Checked { get; set; } = false;
        [NotMapped]
        public List<AdditionalField> AdditionalFields { get; set; }
    }
}