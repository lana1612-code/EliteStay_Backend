using Hotel_Backend_API.Data;
using Hotel_Backend_API.DTO.Guest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Backend_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GuestsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public GuestsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("get_All_Guests")]
        [Authorize(Roles = "Admin")]
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

            if (!guests.Any())
                return BadRequest("No guests found.");

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


        [HttpGet("get_All_Guests_In_Hotel")]
        [Authorize(Roles = "AdminHotel,Admin")]
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

            if (!guests.Any())
                return BadRequest("No guests found for the specified hotel.");

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


        [HttpGet("get_Guest_by_id/{id}")]
        [Authorize]
        public async Task<IActionResult> GetGuestById(int id)
        {
            var guest = await dbContext.Guests
                                        .Where(g => g.Id == id)
                                        .Select(g => new GuestDTO
                                        {Id = g.Id,
                                            Name = g.Name,
                                            Email = g.Email,
                                            Phone = g.Phone
                                        })
                                        .FirstOrDefaultAsync();

            if (guest == null)
                return NotFound("Guest not found.");

            return Ok(guest);
        }

    }
}
