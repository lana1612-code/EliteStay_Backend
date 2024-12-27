using Hotel_Backend_API.Data;
using Hotel_Backend_API.DTO.Comment;
using Hotel_Backend_API.DTO.Complain;
using Hotel_Backend_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Hotel_Backend_API.Controllers.Users
{
    [Route("Normal/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<AppUser> userManager;

        public CommentsController(ApplicationDbContext dbContext,
                         UserManager<AppUser> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }


        [HttpPost("add_comment")]
        [Authorize]
        public async Task<IActionResult> AddComment([FromBody] CommentDTO commentDto)
        {
            if (commentDto == null)
            {
                return BadRequest("Comment data is required.");
            }

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }
            var hotelExists = await dbContext.Hotels.AnyAsync(h => h.Id == commentDto.HotelId);
            if (!hotelExists)
            {
                return BadRequest($"Hotel with ID [{commentDto.HotelId}] does not exist.");
            }
            var comment = new Comment
            {
                UserId = userIdClaim,
                HotelId = commentDto.HotelId,
                Content = commentDto.Content,
                CreatedAt = DateTime.UtcNow,
            };

            dbContext.Comments.Add(comment);
            await dbContext.SaveChangesAsync();

            return Ok("Comment added successfully.");
        }


        [HttpGet("hotel_comments/{hotelId}")]
        public async Task<IActionResult> GetCommentsForHotel(int hotelId)
        {
            var comments = await dbContext.Comments
                .Where(c => c.HotelId == hotelId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        
            List<GetCommentDTO> returnComment = new List<GetCommentDTO>();
            foreach (var comment in comments)
            {
                var user = await userManager.FindByIdAsync(comment.UserId);

                var responseDto = new GetCommentDTO
                {
                    Id = comment.Id,
                    HotelId = comment.HotelId,
                    username =user.UserName,
                    userImg = user.imgUser,
                    Content = comment.Content,
                    CreatedAt = comment.CreatedAt.ToString("yyyy-MM-dd  HH:mm:ss")
                };
                returnComment.Add(responseDto);
            }

            return Ok(returnComment);
        }


        [HttpDelete("remove_comment/{id}")]
        [Authorize]
        public async Task<IActionResult> RemoveComment(int id)
        {
            var comment = await dbContext.Comments.FindAsync(id);

            if (comment == null)
            {
                return NotFound($"Comment with ID [{id}] not found.");
            }

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;


            if (comment.UserId != userIdClaim)
            {
                return Unauthorized("You are not authorized to delete this comment.");
            }

            dbContext.Comments.Remove(comment);
            await dbContext.SaveChangesAsync();

            return Ok("Comment deleted successfully.");
        }

    }
}
