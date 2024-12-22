using Hotel_Backend_API.Data;
using Hotel_Backend_API.DTO.Guest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Backend_API.Controllers
{
    [Route("Admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminGuestsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public AdminGuestsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }


        [HttpGet("{hotelId}")]
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
                                         {Id = g.Id,
                                             Name = g.Name,
                                             Email = g.Email,
                                             Phone = g.Phone
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

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllGuests(int pageNumber = 1, int pageSize = 10)
        {
            var totalGuests = await dbContext.Guests.CountAsync();
            var guests = await dbContext.Guests
                                        .Skip((pageNumber - 1) * pageSize)
                                        .Take(pageSize)
                                        .Select(g => new GuestDTO
                                        {
                                            Name = g.Name,
                                            Email = g.Email,
                                            Phone = g.Phone
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
