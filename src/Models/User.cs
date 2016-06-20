using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace News.Models
{
    public class User
    {

        [Required(ErrorMessage = "Enter username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Enter password")]
        public string Password { get; set; }

        public string Token { get; set; }

        [Required(ErrorMessage = "Select group")]
        public string Group { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public bool EmailVerified { get; set; }
        public string VerificationKey { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool IsBanned { get; set; }
        public DateTime BannedDue { get; set; }

        [Key]
        public int Id { get; set; }

        [NotMapped]
        public bool Checked { get; set; } = false;

        public string AvatarURL { get; set; }
    }
}