using System.ComponentModel.DataAnnotations;

namespace Hotel_Backend_API.Models
{
    public class Hotel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        [Range(1, 4)]
        public int Stars { get; set; }
        public string Tags { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public virtual ICollection<Room> Room { get; set; }

    }
}
