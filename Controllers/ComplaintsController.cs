using Hotel_Backend_API.Data;
using Hotel_Backend_API.DTO.Complain;
using Hotel_Backend_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Hotel_Backend_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComplaintsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public ComplaintsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost("submit")]
        [Authorize]
        public async Task<IActionResult> SubmitComplaint([FromBody] ComplainDTO complaintDTO)
        {

            if (complaintDTO == null)
            {
                return BadRequest("Complaint data is required.");
            }

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            var complaint = new Complaint
            {
                UserId = userIdClaim,  
                HotelId = complaintDTO.HotelId,
                Content = complaintDTO.Content,   
                CreatedAt = DateTime.UtcNow  
            };

            dbContext.Complaints.Add(complaint);
            await dbContext.SaveChangesAsync();

            return Ok("Complaint submitted successfully.");
        }
        [HttpGet("ComplainForHotel/{hotelId}")]
        [Authorize]
        public async Task<IActionResult> GetComplaints()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            var complaints = await dbContext.Complaints
                .Where(c => c.UserId == userIdClaim)
                .Include(c => c.Hotel)
                .ToListAsync();

            var complaintsDTO = complaints.Select(c => new ReturnComplainDTO
            {
                HotelId = c.HotelId,
                Content = c.Content,
                CreatedAt = c.CreatedAt
            }).ToList();

            return Ok(complaintsDTO);
        }
    }
}
