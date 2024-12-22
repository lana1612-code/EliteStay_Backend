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
    public class LikesController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public LikesController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost("like")]
        [Authorize]
        public async Task<IActionResult> LikeRoom([FromQuery] int roomId)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            string userId = userIdClaim;

            var like = new Like
            {
                UserId = userId,
                RoomId = roomId
            };

            dbContext.Likes.Add(like);
            await dbContext.SaveChangesAsync();

            return Ok("Room liked successfully.");
        }


        [HttpGet("user_likes")]
        [Authorize]
        public async Task<IActionResult> GetUserLikes()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            string userId = userIdClaim;
            var likes = await dbContext.Likes
                .Where(l => l.UserId == userId)
                .Include(l => l.Room)
                .ThenInclude(r => r.RoomType)
                .ToListAsync();

            var likeDTOs = likes.Select(l => new
            {
                l.Room.Id,
                l.Room.RoomNumber,
                RoomTypeName = l.Room.RoomType.Name,
                l.Room.RoomType.Description,
                l.Room.RoomType.PricePerNight
            }).ToList();

            return Ok(likeDTOs);

        }

    }
}
