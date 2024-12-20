using Hotel_Backend_API.Data;
using Hotel_Backend_API.DTO.Booking;
using Hotel_Backend_API.DTO.Room;
using Hotel_Backend_API.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Backend_API.Controllers.Users
{
    [Route("Normal/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public RoomsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }


        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllRooms(int pageNumber = 1, int pageSize = 10)
        {
            var totalRooms = await dbContext.Rooms.CountAsync();

            var rooms = await dbContext.Rooms
                                       .Include(r => r.Hotel)
                                       .Include(r => r.RoomType)
                                       .Skip((pageNumber - 1) * pageSize)
                                       .Take(pageSize)
                                       .Select(r => new RoomDTO
                                       {
                                           Id = r.Id,
                                           NameHotel = r.Hotel.Name,
                                           NameRoomType = r.RoomType.Name,
                                           RoomNumber = r.RoomNumber,
                                           Status = r.Status,
                                           PricePerNight = r.RoomType.PricePerNight,
                                           Description = r.RoomType.Description,
                                           Capacity = r.RoomType.Capacity,
                                           ImageURL = r.RoomType.ImageURL
                                       })
                                       .ToListAsync();

            if (!rooms.Any())
                return BadRequest("No rooms found.");

            var response = new
            {
                TotalCount = totalRooms,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(totalRooms / (double)pageSize),
                Data = rooms
            };

            return Ok(response);
        }


        [HttpGet("GetAll/{hotelId}")]
        public async Task<IActionResult> GetAllRoomsInHotel(int hotelId, int pageNumber = 1, int pageSize = 10)
        {
            var totalRooms = await dbContext.Rooms
                                             .Where(r => r.HotelId == hotelId)
                                             .CountAsync();

            var rooms = await dbContext.Rooms
                                       .Include(r => r.Hotel)
                                       .Include(r => r.RoomType)
                                       .Where(r => r.HotelId == hotelId)
                                       .Skip((pageNumber - 1) * pageSize)
                                       .Take(pageSize)
                                       .Select(r => new RoomDTO
                                       {
                                           Id = r.Id,
                                           NameHotel = r.Hotel.Name,
                                           NameRoomType = r.RoomType.Name,
                                           RoomNumber = r.RoomNumber,
                                           Status = r.Status,
                                           Description = r.RoomType.Description,
                                           Capacity = r.RoomType.Capacity,
                                           ImageURL = r.RoomType.ImageURL
                                       })
                                       .ToListAsync();

            if (!rooms.Any())
                return BadRequest("No rooms found for the specified hotel.");

            var response = new
            {
                TotalCount = totalRooms,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(totalRooms / (double)pageSize),
                Data = rooms
            };

            return Ok(response);
        }


        [HttpGet("GetAll_available")]
        public async Task<IActionResult> GetAllAvailableRooms(int pageNumber = 1, int pageSize = 10)
        {
            var totalRooms = await dbContext.Rooms
                                             .Where(r => r.Status == "available")
                                             .CountAsync();

            var rooms = await dbContext.Rooms
                                       .Include(r => r.Hotel)
                                       .Include(r => r.RoomType)
                                       .Where(r => r.Status == "available")
                                       .Skip((pageNumber - 1) * pageSize)
                                       .Take(pageSize)
                                       .Select(r => new RoomDTO
                                       {
                                           Id = r.Id,
                                           NameHotel = r.Hotel.Name,
                                           NameRoomType = r.RoomType.Name,
                                           RoomNumber = r.RoomNumber,
                                           Status = r.Status,
                                           Description = r.RoomType.Description,
                                           Capacity = r.RoomType.Capacity,
                                           ImageURL = r.RoomType.ImageURL
                                       })
                                       .ToListAsync();

            if (!rooms.Any())
                return BadRequest("No available rooms found.");

            var response = new
            {
                TotalCount = totalRooms,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(totalRooms / (double)pageSize),
                Data = rooms
            };

            return Ok(response);
        }


        [HttpGet("ByCapacity")]
        public async Task<IActionResult> GetRoomsByCapacity(int capacity, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var totalRooms = await dbContext.Rooms
                    .Include(r => r.RoomType)
                    .Where(r => r.RoomType.Capacity == capacity)
                    .CountAsync();

                var rooms = await dbContext.Rooms
                    .Include(r => r.Hotel)
                    .Include(r => r.RoomType)
                    .Where(r => r.RoomType.Capacity == capacity)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new RoomDTO
                    {
                        Id = r.Id,
                        NameHotel = r.Hotel.Name,
                        NameRoomType = r.RoomType.Name,
                        RoomNumber = r.RoomNumber,
                        Status = r.Status.ToString(),
                        Description = r.RoomType.Description,
                        Capacity = r.RoomType.Capacity,
                        ImageURL = r.RoomType.ImageURL
                    })
                    .ToListAsync();

                if (!rooms.Any())
                    return NotFound($"No rooms found with capacity [{capacity}].");

                var response = new
                {
                    TotalCount = totalRooms,
                    PageSize = pageSize,
                    CurrentPage = pageNumber,
                    TotalPages = (int)Math.Ceiling(totalRooms / (double)pageSize),
                    Data = rooms
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving rooms by capacity.");
            }
        }

    }
}
