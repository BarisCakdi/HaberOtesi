using System.ComponentModel.DataAnnotations;

namespace HaberOtesi.Models
{
    public class LoginModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }

        public string? RedirectUrl { get; set; }
    }
}
