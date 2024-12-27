using Hotel_Backend_API.Data;
using Hotel_Backend_API.DTO.Hotel;
using Hotel_Backend_API.DTO.Rating;
using Hotel_Backend_API.Models;
using Hotel_Backend_API.Services;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Hotel_Backend_API.Controllers
{
    [Route("AdminHotel/[controller]")]
    [ApiController]
    [Authorize(Roles = "AdminHotel")]
    public class AdminHRatingsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<AppUser> userManager;
        private readonly HotelService hotelService;

        public AdminHRatingsController(ApplicationDbContext dbContext , 
            UserManager<AppUser> userManager,
             HotelService hotelService)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.hotelService = hotelService;
        }

        [HttpGet("GetAll/{hotelId}")]
        public async Task<IActionResult> GetRatingsForHotel(int hotelId)
        {
            var ratings = await dbContext.Ratings
                .Where(r => r.HotelId == hotelId)
                .OrderByDescending(r => r.RatedAt)
                .ToListAsync();

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
                    Id = rating.Id,
                    UserName = user.UserName,
                    userImg = user.imgUser,
                    RatingValue = rating.RatingValue
                });
            }

            return Ok(ratingdto);
        }


    }
}
