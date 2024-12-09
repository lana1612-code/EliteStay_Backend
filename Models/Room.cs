using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_Backend_API.Models
{
    public class Room
    {

        public int Id { get; set; }

        [ForeignKey(nameof(Hotel))]
        public int HotelId { get; set; }

        [ForeignKey(nameof(RoomType))]
        public int RoomTypeId { get; set; }
        public string RoomNumber { get; set; }
        public string Status { get; set; } = "Available";
        public virtual Hotel Hotel { get; set; }
        public virtual RoomType RoomType { get; set; }

        // public enum RoomStatus
        // {
        //     Available,
        //     Occupied,
        // }
    }
}
