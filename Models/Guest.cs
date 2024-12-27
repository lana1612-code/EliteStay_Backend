using System.ComponentModel.DataAnnotations;

namespace Hotel_Backend_API.Models
{
    public class Guest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string? imgUser { get; set; }
        public virtual ICollection<Booking> Booking { get; set; }
    }
}
