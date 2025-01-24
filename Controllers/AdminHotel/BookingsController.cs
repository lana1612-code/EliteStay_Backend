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
    [Route("AdminHotel/[controller]")]
    [ApiController]
    [Authorize(Roles = "AdminHotel")]
    public class AdminHBookingsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly NotificationService notificationService;
        private readonly UserManager<AppUser> userManager;
        private readonly totalPriceService totalPriceService;

        public AdminHBookingsController(ApplicationDbContext dbContext, NotificationService notificationService,
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
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                var bookingDto = new BookingDTO
                {
                    Id = bookingId,
                    GuestName = booking.Guest.Name,
                    RoomNumber = booking.Room.RoomNumber,
                    CheckinDate = booking.CheckinDate.ToString("yyyy-MM-dd"),
                    CheckoutDate = booking.CheckoutDate.ToString("yyyy-MM-dd"),
                    TotalPrice = booking.TotalPrice
                };

                return Ok(bookingDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving the booking by id.");
            }
        }


        [HttpGet("GetAll/{hotelId}")]
        public async Task<IActionResult> GetBookingsByHotelId(int hotelId, int page = 1, int pageSize = 10)
        {
            try
            {
                var hotel = await dbContext.Hotels.FindAsync(hotelId);
                if (hotel == null)
                    return NotFound($"No hotel found with id [{hotelId}]");

                var totalBookings = await dbContext.Bookings
                    .CountAsync(b => b.Room.HotelId == hotelId);

                var bookings = await dbContext.Bookings
                    .Include(b => b.Guest)
                    .Include(b => b.Room)
                    .ThenInclude(b=>b.Hotel)
                    .Include(b => b.Room)
                    .ThenInclude(r => r.RoomType)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var bookingDtos = bookings.Select(b => new BookingDTO
                {
                    Id = b.Id,
                    GuestName = b.Guest.Name,
                    hotelName =b.Room.Hotel.Name,
                    RoomName = b.Room.RoomType.Name,
                    RoomNumber = b.Room.RoomNumber,
                    ImageURL = b.Room.RoomType.ImageURL,
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
                    .CountAsync(b => b.Guest.Id == userId && b.Room.HotelId == hotelId);

                var bookings = await dbContext.Bookings
                    .Include(b => b.Guest)
                    .Include(b => b.Room)
                    .Where(b => b.Guest.Id == userId && b.Room.HotelId == hotelId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

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
                return StatusCode(500, "An error occurred while getting bookings for the specified user and hotel.");
            }
        }


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
            var adminHotelUser = await dbContext.AdminHotels
                    .FirstOrDefaultAsync(m => m.userName.ToLower() == username.ToLower());

            if (adminHotelUser == null)
            {
                return Unauthorized("Admin hotel user not found or does not have permissions.");
            }

            var now = DateTime.UtcNow;
            var endedBookings = await dbContext.Bookings
                .Where(b => b.CheckoutDate <= now && b.Room.Status == "Occupied" && b.Room.Hotel.Id == adminHotelUser.HotelId)
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
