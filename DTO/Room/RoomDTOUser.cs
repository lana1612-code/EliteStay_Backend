﻿namespace Hotel_Backend_API.DTO.Room
{
    public class RoomDTOUser
    {
        public int Id { get; set; }
        public int IdHotel { get; set; }
        public string NameRoomType { get; set; }
        public string RoomNumber { get; set; }
        public string Status { get; set; }
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
        public string Description { get; set; }
        public string ImageURL { get; set; }

    }
}
