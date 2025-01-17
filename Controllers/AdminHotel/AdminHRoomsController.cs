using Hotel_Backend_API.Data;
using Hotel_Backend_API.DTO.Booking;
using Hotel_Backend_API.DTO.Room;
using Hotel_Backend_API.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Backend_API.Controllers
{
    [Route("AdminHotel/[controller]")]
    [ApiController]
    [Authorize(Roles = "AdminHotel")]

    public class AdminHRoomsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public AdminHRoomsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoomById(int id)
        {
            var room = await dbContext.Rooms
                                       .Include(r => r.Hotel)
                                       .Include(r => r.RoomType)
                                       .Where(r => r.Id == id)
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
                                       .FirstOrDefaultAsync();

            return Ok(room);
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
                                           PricePerNight = r.RoomType.PricePerNight,
                                           Description = r.RoomType.Description,
                                           Capacity = r.RoomType.Capacity,
                                           ImageURL = r.RoomType.ImageURL
                                       })
                                       .ToListAsync();

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


        [HttpGet("GetAll_available/{hotelId}")]
        public async Task<IActionResult> GetAllRoomsInHotelAvailable(int hotelId, int pageNumber = 1, int pageSize = 10)
        {
            var totalRooms = await dbContext.Rooms
                                             .Where(r => r.HotelId == hotelId && r.Status == "available")  
                                             .CountAsync();

            var rooms = await dbContext.Rooms
                                       .Include(r => r.Hotel)
                                       .Include(r => r.RoomType)
                                       .Where(r => r.HotelId == hotelId && r.Status == "available")   
                                       .Skip((pageNumber - 1) * pageSize)
                                       .Take(pageSize)
                                       .Select(r => new RoomDTO
                                       {   Id = r.Id,
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


        [HttpGet("GetAll_Occupied/{hotelId}")]
        public async Task<IActionResult> GetAllRoomsInHotelOccupied(int hotelId, int pageNumber = 1, int pageSize = 10)
        {
            var totalRooms = await dbContext.Rooms
                                             .Where(r => r.HotelId == hotelId && r.Status == "Occupied")
                                             .CountAsync();

            var rooms = await dbContext.Rooms
                                       .Include(r => r.Hotel)
                                       .Include(r => r.RoomType)
                                       .Where(r => r.HotelId == hotelId && r.Status == "Occupied")
                                       .Skip((pageNumber - 1) * pageSize)
                                       .Take(pageSize)
                                       .Select(r => new RoomDTO
                                       {Id = r.Id,
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
                        PricePerNight = r.RoomType.PricePerNight,
                        Description = r.RoomType.Description,
                        Capacity = r.RoomType.Capacity,
                        ImageURL = r.RoomType.ImageURL
                    })
                    .ToListAsync();

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

        [HttpPost()]
        public async Task<IActionResult> AddRoom([FromBody] AddRoomDTO newRoomDto)
        {
            try
            {
                if (newRoomDto == null)
                    return BadRequest("Room data is required.");

                var newRoom = new Room
                {
                    HotelId = newRoomDto.HotelId,
                    RoomTypeId = newRoomDto.RoomTypeId,
                    RoomNumber = newRoomDto.RoomNumber,
                    Status = "Available"
                };

                dbContext.Rooms.Add(newRoom);
                await dbContext.SaveChangesAsync();

                var result = new 
                {
                    Id = newRoom.Id,
                    HotelId = newRoomDto.HotelId,
                    RoomTypeId = newRoomDto.RoomTypeId,
                    RoomNumber = newRoomDto.RoomNumber,
                    Status = "Available"
                };

                var response = new
                {
                    Message = "Add Room Success",
                    data = result
                };

                return CreatedAtAction(nameof(GetRoomById), new { id = newRoom.Id }, response);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while adding the room.");
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] UpdateRoomDTO updateRoomDto)
        {
            try
            {
                if (updateRoomDto == null)
                    return BadRequest("Room data is invalid.");

                var room = await dbContext.Rooms.FindAsync(id);
                if (room == null)
                    return NotFound($"Room with id [{id}] not found.");

                room.RoomNumber = updateRoomDto.RoomNumber;
                room.Status = updateRoomDto.Status;

                dbContext.Rooms.Update(room);
                await dbContext.SaveChangesAsync();
                var response = new
                {
                    Message = "Update Room Success",
                    Data = updateRoomDto
                };
                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while updating the room.");
            }
        }


        [HttpPut("status/{id}")]
        public async Task<IActionResult> UpdateRoom(int id)
        {
            try
            {
                var room = await dbContext.Rooms.FindAsync(id);
                if (room == null)
                    return NotFound($"Room with id [{id}] not found.");

                room.Status = "Available";


                dbContext.Rooms.Update(room);
                await dbContext.SaveChangesAsync();
                var response = new
                {
                    Message = "Update Room status Success",
                };
                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while updating the room Status.");
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            try
            {
                var room = await dbContext.Rooms.FindAsync(id);
                if (room == null)
                    return NotFound($"Room with id [{id}] not found.");

                dbContext.Rooms.Remove(room);
                await dbContext.SaveChangesAsync();
                return Ok($"Room with id [{id}] deleted successfully.");
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while deleting the room.");
            }
        }
    }
}
