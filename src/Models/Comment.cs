using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace News.Models
{
    public class Comment
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Введите Имя")]
        public string AuthorName { get; set; }
        [Required(ErrorMessage = "Введите ваш комментарий")]
        [AllowHtml]
        public string Content { get; set; }
        public int NewsId { get; set; }
        public DateTime Date { get; set; }

        [NotMapped]
        public bool Checked { get; set; }
    }

}