using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace News.Models
{
    public class AddField
    {
        [Required(ErrorMessage = "Enter the name of an additional field")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Enter displayed title")]
        public string Title { get; set; }
        public string DefaultValue { get; set; }

        public List<string> Categories { get; set; }
        [Required(ErrorMessage = "Select the type of a field")]
        public string Type { get; set; }
    }
}