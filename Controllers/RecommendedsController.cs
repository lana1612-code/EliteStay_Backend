using Hotel_Backend_API.Data;
using Hotel_Backend_API.DTO.Hotel;
using Hotel_Backend_API.Services;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Backend_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendedsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContex;
        private readonly HotelService hotelService;

        public RecommendedsController(ApplicationDbContext dbContex,
                                      HotelService hotelService)
        {
            this.dbContex = dbContex;
            this.hotelService = hotelService;
        }
        
        [HttpGet("Get_Trend_Hotel")]
        public async Task<ActionResult> GetTrend()
        {
            var Trends = await dbContex.Recommendeds
                             .GroupBy(r => r.HotelId)
                             .Select(group => new
                             {
                                 HotelId = group.Key,
                                 RatingCount = group.Count(),
                                 MeanRating = Math.Round(group.Average
                                                     (r => r.RatingValue), 1)

                             }).Where(r => r.MeanRating >= 3 && r.RatingCount > 200)
                             .OrderByDescending(r => r.MeanRating)
                             .ToListAsync();

            if (!Trends.Any())
            {
                return NotFound("No Trends found.");
            }
            var hotelDetails = new List<object>();

            foreach (var trend in   Trends)
            {
                var hotel1 = await hotelService.GetHotelByIdAsync(trend.HotelId);
                var hotel = hotel1.Adapt<AddHotel>();
                if (hotel != null)
                {
                    hotelDetails.Add(new
                    {
                        trend.HotelId,
                        trend.RatingCount,
                        trend.MeanRating,
                        HotelDetails = hotel
                    });
                }
            }

            return Ok(hotelDetails);


            return Ok(hotelDetails);
        }
        
        [HttpGet("Get_Recommended_Hotel")]
        public async Task<ActionResult> GetRecommendedHotel()
        {
            return Ok();
        }
    }
}
