using System.ComponentModel.DataAnnotations;

namespace News.Models
{
    public class Group
    {
        [Key]
        [Required(ErrorMessage = "Enter group's keyword")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Enter group's title, i.e. : Administrator, Moderator")]
        public string Title { get; set; }
        public bool CanAddNews { get; set; }
        public bool CanEditNews { get; set; }
        public bool CanEditComments { get; set; }
        public bool CanEditCategories { get; set; }
        public bool CanEditGroups { get; set; }
        public bool CanEditTemplate { get; set; }
        public bool CanPostWithNoModeration { get; set; }
        public bool CanEditUsers { get; set; }
        public bool CanAccessAdminPanel { get; set; }
        public bool CanUploadImages { get; set; }
        public bool CanUseSpecialTagsInComments { get; set; }
        public bool CanUseSpecialTagsInNews { get; set; }
    }
}