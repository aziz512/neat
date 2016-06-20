using System.Collections.Generic;

namespace News.Models
{
    public class MainModel
    {
        public Article Article { get; set; }
        public Comment Comment { get; set; }
        public List<Comment> Comments { get; set; }
    }
}