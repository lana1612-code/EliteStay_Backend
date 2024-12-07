using Hotel_Backend_API.Data;
using Hotel_Backend_API.DTO.RoomType;
using Hotel_Backend_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Backend_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize]
    public class RoomTypesController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public RoomTypesController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("get_All_RoomTypes")]
        public async Task<IActionResult> GetAllRoomTypes(int pageNumber = 1, int pageSize = 10)
        {
            var totalRoomTypes = await dbContext.RoomTypes.CountAsync();
            var roomTypes = await dbContext.RoomTypes
                                           .Skip((pageNumber - 1) * pageSize)
                                           .Take(pageSize)
                                           .Select(rt => new RoomTypeDTO
                                           {
                                               Name = rt.Name,
                                               PricePerNight = rt.PricePerNight,
                                               Capacity = rt.Capacity,
                                               Description = rt.Description,
                                               ImageURL = rt.ImageURL
                                           })
                                           .ToListAsync();

            if (!roomTypes.Any())
                return BadRequest("No Room types found.");

            var response = new
            {
                TotalCount = totalRoomTypes,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(totalRoomTypes / (double)pageSize),
                Data = roomTypes
            };

            return Ok(response);
        }


        [HttpGet("get_RoomType_by_id/{id}")]
        public async Task<IActionResult> GetRoomTypeById(int id)
        {
            var roomType = await dbContext.RoomTypes
                                           .Where(rt => rt.Id == id)
                                           .Select(rt => new RoomTypeDTO
                                           {
                                               Name = rt.Name,
                                               PricePerNight = rt.PricePerNight,
                                               Capacity = rt.Capacity,
                                               Description = rt.Description,
                                               ImageURL = rt.ImageURL
                                           })
                                           .FirstOrDefaultAsync();

            if (roomType == null)
                return NotFound("Room type not found.");

            return Ok(roomType);
        }


        [HttpGet("get_RoomTypes_In_Hotel")]
        public async Task<IActionResult> GetRoomTypesInHotel(int hotelId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var totalRoomTypes = await dbContext.RoomTypes
                    .Where(rt => dbContext.Rooms.Any(r => r.RoomTypeId == rt.Id && r.HotelId == hotelId))
                    .CountAsync();

                var roomTypes = await dbContext.RoomTypes
                    .Where(rt => dbContext.Rooms.Any(r => r.RoomTypeId == rt.Id && r.HotelId == hotelId))
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(rt => new RoomTypeDTO
                    {
                        Name = rt.Name,
                        PricePerNight = rt.PricePerNight,
                        Capacity = rt.Capacity,
                        Description = rt.Description,
                        ImageURL = rt.ImageURL
                    })
                    .ToListAsync();

                if (!roomTypes.Any())
                    return NotFound($"No room types found for hotel with ID [{hotelId}].");

                var response = new
                {
                    TotalCount = totalRoomTypes,
                    PageSize = pageSize,
                    CurrentPage = pageNumber,
                    TotalPages = (int)Math.Ceiling(totalRoomTypes / (double)pageSize),
                    Data = roomTypes
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving room types in the hotel.");
            }
        }


        [HttpPost("add_room_type")]
        public async Task<IActionResult> AddRoomType([FromBody] RoomTypeDTO newRoomTypeDto)
        {
            try
            {
                if (newRoomTypeDto == null)
                    return BadRequest("Room type data is required.");

                var newRoomType = new RoomType
                {
                    Name = newRoomTypeDto.Name,
                    PricePerNight = newRoomTypeDto.PricePerNight,
                    Capacity = newRoomTypeDto.Capacity,
                    Description = newRoomTypeDto.Description,
                    ImageURL = newRoomTypeDto.ImageURL
                };

                dbContext.RoomTypes.Add(newRoomType);
                await dbContext.SaveChangesAsync();

                var roomTypeDto = new RoomTypeDTO
                {
                    Name = newRoomType.Name,
                    PricePerNight = newRoomType.PricePerNight,
                    Capacity = newRoomType.Capacity,
                    Description = newRoomType.Description,
                    ImageURL = newRoomType.ImageURL
                };

                var response = new
                {
                    Message = "Add Room Type Success",
                    Data = roomTypeDto
                };

                return CreatedAtAction(nameof(GetAllRoomTypes), new { id = newRoomType.Id }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while adding the room type.");
            }

        }


        [HttpPut("update_room_type/{id}")]
        public async Task<IActionResult> UpdateRoomType(int id, [FromBody] RoomTypeDTO updateRoomTypeDto)

        {
            try
            {
                if (updateRoomTypeDto == null)
                    return BadRequest("Room type data is invalid.");

                var roomType = await dbContext.RoomTypes.FindAsync(id);
                if (roomType == null)
                    return NotFound($"Room type with id [{id}] not found.");

                roomType.Name = updateRoomTypeDto.Name;
                roomType.PricePerNight = updateRoomTypeDto.PricePerNight;
                roomType.Capacity = updateRoomTypeDto.Capacity;
                roomType.Description = updateRoomTypeDto.Description;
                roomType.ImageURL = updateRoomTypeDto.ImageURL;

                dbContext.RoomTypes.Update(roomType);
                await dbContext.SaveChangesAsync();

                var roomTypeDto = new RoomTypeDTO
                {
                    Name = roomType.Name,
                    PricePerNight = roomType.PricePerNight,
                    Capacity = roomType.Capacity,
                    Description = roomType.Description,
                    ImageURL = roomType.ImageURL
                };
                var response = new
                {
                    Message = "Update Room Type Success",
                    Data = roomTypeDto
                };
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while deleting the Room Type");
            }
        }


        [HttpDelete("delete_room_type/{id}")]
        public async Task<IActionResult> DeleteRoomType(int id)
        {
            try
            {
                var roomType = await dbContext.RoomTypes.FindAsync(id);
                if (roomType == null)
                    return NotFound($"Room type with id [{id}] not found.");

                dbContext.RoomTypes.Remove(roomType);
                await dbContext.SaveChangesAsync();
                return Ok($"Room type with id [{id}] deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while deleting the Room Type");
            }
        }

    }
}
