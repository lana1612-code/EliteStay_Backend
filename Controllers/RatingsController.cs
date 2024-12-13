using Hotel_Backend_API.Data;
using Hotel_Backend_API.DTO.Rating;
using Hotel_Backend_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Hotel_Backend_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<AppUser> userManager;

        public RatingsController(ApplicationDbContext dbContext , 
            UserManager<AppUser> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }

        [HttpPost("add_rating")]
        [Authorize]
        public async Task<IActionResult> AddRating([FromBody] AddRatingDto ratingDto)
        {
            if (ratingDto == null)
            {
                return BadRequest("Rating data is required.");
            }

            var hotel = await dbContext.Hotels.FindAsync(ratingDto.HotelId);
            if (hotel == null)
                return NotFound("No Hotel found.");

            if (ratingDto.RatingValue < 1 ||
                ratingDto.RatingValue > 5 ||
                ratingDto.RatingValue % 0.5 != 0)
            {
                return BadRequest("Rating value must be between 1 and 5, with a step of 0.5.");
            }

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("User ID not found in token.");

            var rating = new Rating
            {
                UserId = userIdClaim,
                HotelId = ratingDto.HotelId,
                RatingValue = ratingDto.RatingValue
            };

            dbContext.Ratings.Add(rating);
            await dbContext.SaveChangesAsync();

            return Ok("Rating added successfully.");
        }


        [HttpGet("all_ratings")]
        public async Task<IActionResult> GetAllRatings()
        {
            var ratings = await dbContext.Ratings
                .OrderByDescending(r => r.RatedAt)
                .ToListAsync();

            if (!ratings.Any())
            {
                return NotFound("No ratings found.");
            }

            List<RatingDTO> ratingdto = new List<RatingDTO>();

            foreach (var rating in ratings)
            {
                var user = await userManager.FindByIdAsync(rating.UserId);
                if (user == null)
                {
                    return NotFound("User not found");
                }
                var hotel = await dbContext.Hotels.FindAsync(rating.HotelId);
                if (hotel == null)
                    return NotFound("No Hotel found.");

                ratingdto.Add(new RatingDTO
                {
                    UserName = user.UserName,
                    HotelName = hotel.Name,
                    RatingValue = rating.RatingValue
                });
            }

            return Ok(ratingdto);
        }


        [HttpGet("hotel_ratings/{hotelId}")]
        public async Task<IActionResult> GetRatingsForHotel(int hotelId)
        {
            var ratings = await dbContext.Ratings
                .Where(r => r.HotelId == hotelId)
                .OrderByDescending(r => r.RatedAt)
                .ToListAsync();

            if (!ratings.Any())
            {
                return NotFound($"No ratings found for hotel with ID [{hotelId}].");
            }

            List<returnRatingDTO> ratingdto = new List<returnRatingDTO>();

            foreach (var rating in ratings)
            {
                var user = await userManager.FindByIdAsync(rating.UserId);
                if (user == null)
                {
                    return NotFound("User not found");
                }
                var hotel = await dbContext.Hotels.FindAsync(rating.HotelId);
                if (hotel == null)
                    return NotFound("No Hotel found.");

                ratingdto.Add(new returnRatingDTO
                {
                    UserName = user.UserName,
                    RatingValue = rating.RatingValue
                });
            }

            return Ok(ratingdto);
        }
    }
}
