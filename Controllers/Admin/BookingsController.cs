using Hotel_Backend_API.Data;
using Hotel_Backend_API.DTO.Booking;
using Hotel_Backend_API.Models;
using Hotel_Backend_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Hotel_Backend_API.Controllers
{
    [Route("Admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminBookingsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly NotificationService notificationService;
        private readonly UserManager<AppUser> userManager;
        private readonly totalPriceService totalPriceService;

        public AdminBookingsController(ApplicationDbContext dbContext, NotificationService notificationService,
                                  UserManager<AppUser> userManager, totalPriceService totalPriceService)
        {
            this.dbContext = dbContext;
            this.notificationService = notificationService;
            this.userManager = userManager;
            this.totalPriceService = totalPriceService;
        }


        [HttpGet("{bookingId}")]
        public async Task<IActionResult> GetBookingById(int bookingId)
        {
            try
            {
                var booking = await dbContext.Bookings
                    .Include(b => b.Guest)
                    .Include(b => b.Room)
                    .ThenInclude(r => r.Hotel)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                var bookingDto = new BookingDTO
                {
                    Id = bookingId,
                    GuestName = booking.Guest.Name,
                    RoomNumber = booking.Room.RoomNumber,
                    CheckinDate = booking.CheckinDate.ToString("yyyy-MM-dd"),
                    CheckoutDate = booking.CheckoutDate.ToString("yyyy-MM-dd"),
                    TotalPrice = booking.TotalPrice,
                    hotelName = booking.Room.Hotel.Name
                };

                return Ok(bookingDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving the booking by id.");
            }
        }


        [HttpGet("GetAll")]
        public async Task<IActionResult> GetBookingsByHotelId( int page = 1, int pageSize = 10)
        {
            try
            {
                var totalBookings = await dbContext.Bookings
                    .Include(b => b.Room)
                    .ThenInclude(r => r.Hotel)
                    .CountAsync();

                var bookings = await dbContext.Bookings
                    .Include(b => b.Guest)
                    .Include(b => b.Room)
                    .ThenInclude(r => r.Hotel)
                    .Include(b => b.Room)
                    .ThenInclude(r => r.RoomType)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(b => new BookingDTO
                    {
                        Id = b.Id,
                        GuestName = b.Guest.Name,
                        GuestImg = b.Guest.imgUser.Substring(b.Guest.imgUser.IndexOf("images")),
                        RoomNumber = b.Room.RoomNumber,
                        ImageURL = b.Room.RoomType.ImageURL,
                        RoomName =b.Room.RoomType.Name,
                        CheckinDate = b.CheckinDate.ToString("yyyy-MM-dd"),
                        CheckoutDate = b.CheckoutDate.ToString("yyyy-MM-dd"),
                        TotalPrice = b.TotalPrice,
                        hotelName = b.Room.Hotel.Name 
                    })
                    .ToListAsync();

                var response = new
                {
                    TotalCount = totalBookings,
                    PageSize = pageSize,
                    CurrentPage = page,
                    TotalPages = (int)Math.Ceiling(totalBookings / (double)pageSize),
                    Data = bookings
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while getting bookings for the specified hotel.");
            }
        }


        [HttpGet("GetAll/User/{userId}/{hotelId}")]
        public async Task<IActionResult> GetBookingsByUserIdAndHotelId(int userId, int hotelId, int page = 1, int pageSize = 10)
        {
            try
            {
                var totalBookings = await dbContext.Bookings
                    .Include(b => b.Room)
                    .ThenInclude(r => r.Hotel)
                    .CountAsync(b => b.Guest.Id == userId && b.Room.HotelId == hotelId);

                var bookings = await dbContext.Bookings
                    .Include(b => b.Guest)
                    .Include(b => b.Room)
                    .ThenInclude(r => r.Hotel)
                    .Where(b => b.Guest.Id == userId && b.Room.HotelId == hotelId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var bookingDtos = bookings.Select(b => new BookingDTO
                {Id = b.Id,
                    GuestName = b.Guest.Name,
                    GuestImg = b.Guest.imgUser,
                    hotelName = b.Room.Hotel.Name,
                    RoomNumber = b.Room.RoomNumber,
                    CheckinDate = b.CheckinDate.ToString("yyyy-MM-dd"),
                    CheckoutDate = b.CheckoutDate.ToString("yyyy-MM-dd"),
                    TotalPrice = b.TotalPrice
                }).ToList();

                var response = new
                {
                    TotalCount = totalBookings,
                    PageSize = pageSize,
                    CurrentPage = page,
                    TotalPages = (int)Math.Ceiling(totalBookings / (double)pageSize),
                    Data = bookingDtos
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while getting bookings for the specified user and hotel.");
            }
        }

        /**/
        [HttpGet("GetAll/date_filtering/{hotelId}")]
        public async Task<IActionResult> GetAllBookings(int hotelId, int page = 1, int pageSize = 10, string startDate = null, string endDate = null)   
        {
            try
            {
                var query = dbContext.Bookings
                                      .Include(b => b.Guest)
                                      .Include(b => b.Room)
                                      .Where(b => b.Room.HotelId == hotelId) // Filter by hotelId  
                                      .AsQueryable();

                if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out DateTime parsedStartDate))
                {
                    query = query.Where(b => b.CheckinDate >= parsedStartDate);
                }

                if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out DateTime parsedEndDate))
                {
                    query = query.Where(b => b.CheckoutDate <= parsedEndDate);
                }

                var totalBookings = await query.CountAsync();

                var bookings = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

                var bookingDtos = bookings.Select(b => new BookingDTO
                {Id = b.Id,
                    GuestName = b.Guest.Name,
                    RoomNumber = b.Room.RoomNumber,
                    CheckinDate = b.CheckinDate.ToString("yyyy-MM-dd"),
                    CheckoutDate = b.CheckoutDate.ToString("yyyy-MM-dd"),
                    TotalPrice = b.TotalPrice
                }).ToList();


                var response = new
                {
                    TotalCount = totalBookings,
                    PageSize = pageSize,
                    CurrentPage = page,
                    TotalPages = (int)Math.Ceiling(totalBookings / (double)pageSize),
                    Data = bookingDtos
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while getting bookings.");
            }
        }


        [HttpPut("CheckBooking")]
        public async Task<IActionResult> CheckBooking()
        {

            var userClaims = User.Claims;

            var userIdClaim = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            var usernameClaim = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
            if (usernameClaim == null)
            {
                return Unauthorized("Username not found in token.");
            }

            string userId = userIdClaim;
            string username = usernameClaim;
          

            var now = DateTime.UtcNow;
            var endedBookings = await dbContext.Bookings
                .Where(b => b.CheckoutDate <= now && b.Room.Status == "Occupied")
                .ToListAsync();


            foreach (var booking in endedBookings)
            {
                var room = await dbContext.Rooms.FindAsync(booking.RoomId);
                if (room != null)
                {
                    var guest = await dbContext.Guests.FindAsync(booking.GuestId);
                    if (guest == null)
                    {
                        return NotFound("Guest not found");
                    }

                    var userGuest = await userManager.FindByNameAsync(guest.Name);
                    if (userGuest == null)
                    {
                        return NotFound("Guest user not found in the system");
                    }

                    room.Status = "Available"; 

                    var notificationMessageAdmin = $"Room {room.RoomNumber} is now available.";
                    await notificationService.CreateNotificationAsync(userId, notificationMessageAdmin);

                    var notificationMessageGuest = $"Your booking in room {room.RoomNumber} ends today.";
                    await notificationService.CreateNotificationAsync(userGuest.Id, notificationMessageGuest);

                }
            }

            await dbContext.SaveChangesAsync();

            return Ok("Room statuses updated !!!");

        }
    }
}
