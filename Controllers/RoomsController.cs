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
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public RoomsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }


        [HttpGet("get_All_Rooms")]
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
                                           NameHotel = r.Hotel.Name,
                                           NameRoomType = r.RoomType.Name,
                                           RoomNumber = r.RoomNumber,
                                           Status = r.Status
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


        [HttpGet("get_Room_by_id/{id}")]
        public async Task<IActionResult> GetRoomById(int id)
        {
            var room = await dbContext.Rooms
                                       .Include(r => r.Hotel)
                                       .Include(r => r.RoomType)
                                       .Where(r => r.Id == id)
                                       .Select(r => new RoomDTO
                                       {
                                           NameHotel = r.Hotel.Name,
                                           NameRoomType = r.RoomType.Name,
                                           RoomNumber = r.RoomNumber,
                                           Status = r.Status
                                       })
                                       .FirstOrDefaultAsync();

            if (room == null)
                return NotFound("Room not found.");

            return Ok(room);
        }


        [HttpGet("get_All_Rooms_in_hotel/{hotelId}")]
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
                                           NameHotel = r.Hotel.Name,
                                           NameRoomType = r.RoomType.Name,
                                           RoomNumber = r.RoomNumber,
                                           Status = r.Status
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


        [HttpGet("get_All_Rooms_in_hotel_with_status_available/{hotelId}")]
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
                                       {
                                           NameHotel = r.Hotel.Name,
                                           NameRoomType = r.RoomType.Name,
                                           RoomNumber = r.RoomNumber,
                                           Status = r.Status
                                       })
                                       .ToListAsync();

            if (!rooms.Any())
                return BadRequest("No available rooms found for the specified hotel.");

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


        [HttpGet("get_All_Rooms_in_hotel_with_status_Occupied/{hotelId}")]
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
                                       {
                                           NameHotel = r.Hotel.Name,
                                           NameRoomType = r.RoomType.Name,
                                           RoomNumber = r.RoomNumber,
                                           Status = r.Status
                                       })
                                       .ToListAsync();

            if (!rooms.Any())
                return BadRequest("No Occupied rooms found for the specified hotel.");

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


        [HttpGet("get_All_Available_Rooms")]
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
                                           NameHotel = r.Hotel.Name,
                                           NameRoomType = r.RoomType.Name,
                                           RoomNumber = r.RoomNumber,
                                           Status = r.Status
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


        [HttpGet("get_Rooms_ByCapacity")]
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
                        NameHotel = r.Hotel.Name,
                        NameRoomType = r.RoomType.Name,
                        RoomNumber = r.RoomNumber,
                        Status = r.Status.ToString()
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

        [Authorize]
        [HttpPost("add_room")]
        [Authorize(Roles = "AdminHotel")]
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

                var response = new
                {
                    Message = "Add Room Success",
                    Data = newRoomDto
                };

                return CreatedAtAction(nameof(GetRoomById), new { id = newRoom.Id }, response);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while adding the room.");
            }
        }


        [HttpPut("update_room/{id}")]
        [Authorize(Roles = "AdminHotel")]
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


        [HttpPut("update_room_To_Available/{id}")]
        [Authorize(Roles = "AdminHotel")]
        public async Task<IActionResult> UpdateRoom(int id)
        {
            try
            {
                var room = await dbContext.Rooms.FindAsync(id);
                if (room == null)
                    return NotFound($"Room with id [{id}] not found.");

                room.Status = "Available";

              var data =  room.Adapt<RoomDTO>();

                dbContext.Rooms.Update(room);
                await dbContext.SaveChangesAsync();
                var response = new
                {
                    Message = "Update Room Success",
                    Data = data
                };
                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while updating the room Status.");
            }
        }



        [HttpDelete("delete_room/{id}")]
        [Authorize(Roles = "AdminHotel")]
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
