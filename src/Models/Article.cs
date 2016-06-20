using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace News.Models
{
    public class Article
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Введите название")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Введите автора")]
        public string Author { get; set; }
        [Required(ErrorMessage = "Введите тело статьи")]
        public string Content { get; set; }
        [Required(ErrorMessage = "Выберите категорию")]
        public string Category { get; set; }
        [Required(ErrorMessage = "Введите дату")]
        public DateTime Date { get; set; }
        [Required]
        public bool Moderated { get; set; }


        [NotMapped]
        public bool Checked { get; set; } = false;
        [NotMapped]
        public List<AdditionalField> AdditionalFields { get; set; }
    }
}