using FluentValidation;
using Hotel_Backend_API.Data;
using Hotel_Backend_API.DTO.Booking;
using Hotel_Backend_API.DTO.Hotel;
using Hotel_Backend_API.Models;
using Hotel_Backend_API.Services;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using static OfficeOpenXml.ExcelErrorValue;

namespace Hotel_Backend_API.Controllers.Users
{
    [Route("Normal/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly HotelService hotelService;

        public HotelsController(ApplicationDbContext dbContext,
                                HotelService hotelService)
        {
            this.dbContext = dbContext;
            this.hotelService = hotelService;
        }

        [HttpGet("GetAllID")]
        public async Task<IActionResult> GetAllIdHotel()
        {
            var ids = await dbContext.Hotels
                                   .Select(h => h.Id)
                                   .ToListAsync();

            var response = new
            {
                Id = ids
            };

            return Ok(response);
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var totalHotels = await dbContext.Hotels.CountAsync();

                var hotels = await dbContext.Hotels
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var response = new
                {
                    TotalCount = totalHotels,
                    PageSize = pageSize,
                    CurrentPage = pageNumber,
                    TotalPages = (int)Math.Ceiling(totalHotels / (double)pageSize),
                    Data = hotels.Adapt<IEnumerable<AddHotel>>() // Assuming you have a mapping setup  
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving hotels.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var hotel = await hotelService.GetHotelByIdAsync(id);

                var response = hotel.Adapt<AddHotel>();
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500, "an error while get hotel");
            }
        }

        [HttpGet("ByStars")]
        public async Task<IActionResult> GetByStars(int num_stars, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var totalHotels = await dbContext.Hotels.CountAsync(h => h.Stars == num_stars);

                var hotels = await dbContext.Hotels
                    .Where(h => h.Stars == num_stars)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var response = new
                {
                    TotalCount = totalHotels,
                    PageSize = pageSize,
                    CurrentPage = pageNumber,
                    TotalPages = (int)Math.Ceiling(totalHotels / (double)pageSize),
                    Data = hotels.Adapt<IEnumerable<AddHotel>>()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving hotels by stars.");
            }
        }

        [HttpGet("ByAddress")]
        public async Task<IActionResult> GetByAddress(string input_address, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var lower_input_address = input_address.ToLower();
                var totalHotels = await dbContext.Hotels
                    .Where(h => h.Address.ToLower() == lower_input_address)
                    .CountAsync();

                var hotels = await dbContext.Hotels
                    .Where(h => h.Address.ToLower() == lower_input_address)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var response = new
                {
                    TotalCount = totalHotels,
                    PageSize = pageSize,
                    CurrentPage = pageNumber,
                    TotalPages = (int)Math.Ceiling(totalHotels / (double)pageSize),
                    Data = hotels.Adapt<IEnumerable<AddHotel>>()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving hotels by address.");
            }
        }

        [HttpGet("AllTage")]
        public async Task<IActionResult> GetAllTage()
        {
            List<string> tags = new List<string>();
            var hotels = await dbContext.Hotels.Select(h=> h.Tags).ToListAsync();
            foreach (var hotel in hotels)
            {
                var s = hotel.Split(',');
                foreach (var word in s)
                {
                    tags.Add(word.Trim());
                }
            }

            return Ok(tags.Distinct());
        }

        [HttpGet("recommendationUsingTags")]
        public async Task<IActionResult> GetRecommendations([FromQuery] string tagsSearchString, int numOfRecommendations = 10)
        {
            if (string.IsNullOrWhiteSpace(tagsSearchString))
            {
                return BadRequest("Search string must not be empty.");
            }
            try
            {
                var recommendations = await hotelService.GetHotelRecommendationsByTagsAsync(tagsSearchString, numOfRecommendations);

                var response = recommendations.Adapt<IEnumerable<AddHotel>>();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

    }
}
