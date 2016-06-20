using System;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace News.Models
{
    public class EditProfileAdmin
    {
        [Required]
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        [Required]
        public string Group { get; set; }

        [Required(ErrorMessage = "Enter your email"), EmailAddress]
        public string Email { get; set; }
        public HttpPostedFileBase Avatar { get; set; }
        public string CurrentAvatarURL { get; set; }
        [Required]
        public bool IsBanned { get; set; }
        public DateTime BannedDue { get; set; }
    }
}