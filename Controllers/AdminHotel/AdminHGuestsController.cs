using Hotel_Backend_API.Data;
using Hotel_Backend_API.DTO.Guest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Backend_API.Controllers
{
    [Route("AdminHotel/[controller]")]
    [ApiController]
    [Authorize(Roles = "AdminHotel")]
    public class AdminHGuestsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public AdminHGuestsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }


        [HttpGet("GetAll/{hotelId}")]
        public async Task<IActionResult> GetAllGuests(int hotelId, int pageNumber = 1, int pageSize = 10)
        {
            var totalGuests = await dbContext.Bookings
                                              .Where(b => b.Room.HotelId == hotelId)
                                              .Select(b => b.GuestId)
                                              .Distinct()
                                              .CountAsync();

            var guestIds = await dbContext.Bookings
                                           .Where(b => b.Room.HotelId == hotelId)
                                           .Select(b => b.GuestId)
                                           .Distinct()
                                           .Skip((pageNumber - 1) * pageSize)
                                           .Take(pageSize)
                                           .ToListAsync();

            var guests = await dbContext.Guests
                                         .Where(g => guestIds.Contains(g.Id))
                                         .Select(g => new GuestDTO
                                         {
                                             Id = g.Id,
                                             Name = g.Name,
                                             Email = g.Email,
                                             Phone = g.Phone,
                                             imgUser = g.imgUser.Substring(g.imgUser.IndexOf("images"))
                                         })
                                         .ToListAsync();

            var response = new
            {
                TotalCount = totalGuests,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(totalGuests / (double)pageSize),
                Data = guests
            };

            return Ok(response);
        }


    }
}
