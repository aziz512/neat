using System.ComponentModel.DataAnnotations;
using System.Web;

namespace News.Models
{
    public class EditProfile
    {
        [Required]
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string OldPassword { get; set; }
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Enter your email")]
        public string Email { get; set; }
        public HttpPostedFileBase Avatar { get; set; }
    }
}