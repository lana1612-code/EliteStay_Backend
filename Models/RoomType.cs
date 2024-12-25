using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Hotel_Backend_API.Models
{
    public class RoomType
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
        public string? Description { get; set; }
        public string? ImageURL { get; set; }
        [JsonIgnore]
        public virtual ICollection<Room> Room { get; set; }
    }
}
