using System;
using System.ComponentModel.DataAnnotations;

namespace News.Models
{
    public class LogEntry
    {
        [Key]
        public string Username { get; set; }
        public string Action { get; set; }
        public DateTime Date { get; set; }
    }
}