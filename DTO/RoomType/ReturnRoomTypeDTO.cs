using System.ComponentModel.DataAnnotations.Schema;

namespace Hotel_Backend_API.DTO.RoomType
{
    public class ReturnRoomTypeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string hotelName { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
        public string? Description { get; set; }
        public string ImageURL { get; set; }
    }
}
