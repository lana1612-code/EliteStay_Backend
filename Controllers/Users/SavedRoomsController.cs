using Hotel_Backend_API.Data;
using Hotel_Backend_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Hotel_Backend_API.Controllers.Users
{
    [Route("Normal/[controller]")]
    [ApiController]
    public class SavedRoomsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public SavedRoomsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost("save")]
        [Authorize]
        public async Task<IActionResult> SaveRoom([FromQuery] int roomId)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }
            var room = await dbContext.Rooms.FindAsync(roomId);
            if (room == null)
                return NotFound($"Room with id [{roomId}] not found.");

            string userId = userIdClaim;

            var savedRoom = new SavedRoom
            {
                UserId = userId,
                RoomId = roomId
            };

            dbContext.SavedRooms.Add(savedRoom);
            await dbContext.SaveChangesAsync();

            return Ok("Room saved successfully.");
        }


        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetSavedRooms()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            string userId = userIdClaim;
            var savedRooms = await dbContext.SavedRooms
                 .Where(sr => sr.UserId == userId)
                 .Include(sr => sr.Room)
                    .ThenInclude(r => r.RoomType)
                 .GroupBy(sr => sr.Room.Id)
                 .Select(g => g.FirstOrDefault())
                 .ToListAsync();

            var savedRoomDTOs = savedRooms.Select(sr => new
            {
                sr.Room.Id,
                sr.Room.RoomNumber,
                RoomTypeName = sr.Room.RoomType.Name,
                sr.Room.RoomType.ImageURL,
                sr.Room.RoomType.Description,
                sr.Room.RoomType.PricePerNight,
                sr.SavedAt
            }).ToList();

            return Ok(savedRoomDTOs);
        }


        [HttpDelete("remove/{roomId}")]
        [Authorize]
        public async Task<IActionResult> RemoveSavedRoom(int roomId)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            var savedRoom = await dbContext.SavedRooms
                .FirstOrDefaultAsync(sr => sr.RoomId == roomId && sr.UserId == userIdClaim);

            if (savedRoom == null)
            {
                return NotFound($"Saved room with ID [{roomId}] not found for the current user.");
            }

            dbContext.SavedRooms.Remove(savedRoom);
            await dbContext.SaveChangesAsync();

            return Ok("Saved room removed successfully.");
        }

    }
}
