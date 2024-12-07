using System.ComponentModel.DataAnnotations;

namespace Hotel_Backend_API.DTO.Acount
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
