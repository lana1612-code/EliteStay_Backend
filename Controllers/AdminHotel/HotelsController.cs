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

namespace Hotel_Backend_API.Controllers
{
    [Route("AdminHotel/[controller]")]
    [ApiController]
    [Authorize(Roles = "AdminHotel")]
    public class AdminHHotelsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly HotelService hotelService;

        public AdminHHotelsController(ApplicationDbContext dbContext,
                                HotelService hotelService)
        {
            this.dbContext = dbContext;
            this.hotelService = hotelService;
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

    }
}
