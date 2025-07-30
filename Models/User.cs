using System.ComponentModel.DataAnnotations;

namespace SignInAPI.Models
{
    public class User
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
