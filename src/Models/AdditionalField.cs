using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace News.Models
{
    public class AdditionalField
    {
        public string Categories { get; set; }
        [Key]
        public string Name { get; set; }
        public string Title { get; set; }
        public string DefaultValue { get; set; }
        public string Type { get; set; }
        [NotMapped]
        public string Value { get; set; }
        [NotMapped]
        public bool Checked { get; set; }



    }
}