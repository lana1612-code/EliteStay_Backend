using Hotel_Backend_API.Data;
using Hotel_Backend_API.DTO.Booking;
using Hotel_Backend_API.Models;
using Hotel_Backend_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Hotel_Backend_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly NotificationService notificationService;

        public BookingsController(ApplicationDbContext dbContext, NotificationService notificationService)
        {
            this.dbContext = dbContext;
            this.notificationService = notificationService;
        }


        [HttpGet("get_All_Bookings")]
        public async Task<IActionResult> GetAllBookings(int page = 1, int pageSize = 10)
        {

            try
            {
                if (page < 1)
                    return BadRequest("Page must be greater than or equal to 1.");

                if (pageSize < 1 || pageSize > 50)
                    return BadRequest("Page size must be between 1 and 50.");

                var totalBookings = await dbContext.Bookings.CountAsync();

                var bookings = await dbContext.Bookings
                    .Include(b => b.Guest)
                    .Include(b => b.Room)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                if (bookings == null || !bookings.Any())
                    return NotFound("No bookings found.");

                var bookingDtos = bookings.Select(b => new BookingDTO
                {
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
                return StatusCode(500, "An error occurred while retrieving all bookings.");
            }
        }


        [HttpGet("get_Booking_by_Id/{bookingId}")]
        public async Task<IActionResult> GetBookingById(int bookingId)
        {
            try
            {
                var booking = await dbContext.Bookings
                    .Include(b => b.Guest)
                    .Include(b => b.Room)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null)
                    return NotFound($"No booking found with id [{bookingId}].");

                var bookingDto = new BookingDTO
                {
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


        [HttpGet("get_Booking_by_Hotel_Id/{hotelId}")]
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
                    .Where(b => b.Room.HotelId == hotelId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                if (bookings == null || !bookings.Any())
                    return NotFound("No bookings found for the specified hotel.");

                var bookingDtos = bookings.Select(b => new BookingDTO
                {
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
                return StatusCode(500, "An error occurred while getting bookings for the specified hotel.");
            }
        }


        [HttpGet("get_Booking_for_User_By_User_Id/{userId}")]
        public async Task<IActionResult> GetBookingsByUserId(int userId, int page = 1, int pageSize = 10)
        {
            try
            {
                var totalBookings = await dbContext.Bookings
                    .CountAsync(b => b.Guest.Id == userId);

                var bookings = await dbContext.Bookings
                    .Include(b => b.Guest)
                    .Include(b => b.Room)
                    .Where(b => b.Guest.Id == userId)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                if (bookings == null || !bookings.Any())
                    return NotFound("No bookings found for the specified user.");

                var bookingDtos = bookings.Select(b => new BookingDTO
                {
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
                return StatusCode(500, "An error occurred while getting bookings for the specified user.");
            }
        }


        [HttpGet("get_Booking_within_date_filtering")]
        public async Task<IActionResult> GetAllBookings(int page = 1, int pageSize = 10, string startDate = null, string endDate = null)
        {
            try
            {
                var query = dbContext.Bookings.Include(b => b.Guest).Include(b => b.Room).AsQueryable();

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
                {
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


        [HttpGet("get_Bookings_within_date_filtering_In_Hotel")]
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
                {
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


        [HttpPost("Add_Booking")]
        [Authorize]
        public async Task<IActionResult> AddBooking([FromBody] AddBookingDTO newBookingDto, [FromQuery] string method)
        {
            try
            {
                if (newBookingDto == null)
                    return BadRequest("Booking data is required.");

                if (!DateTime.TryParse(newBookingDto.CheckinDate, out DateTime checkinDate))
                    return BadRequest("Invalid check-in date format. Please use 'yyyy-MM-dd'.");

                if (!DateTime.TryParse(newBookingDto.CheckoutDate, out DateTime checkoutDate))
                    return BadRequest("Invalid check-out date format. Please use 'yyyy-MM-dd'.");

                if (checkoutDate <= checkinDate)
                    return BadRequest("Check-out date must be after check-in date.");

                var userClaims = User.Claims;

                var userIdClaim = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized("User ID not found in token.");
                }

                string userId = userIdClaim;
                var username = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var email = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var phone = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.MobilePhone)?.Value;

                var guest = new Guest
                {
                    Name = email.Split("@")[0] ?? "Unknown",
                    Email = email ?? "Unknown",
                    Phone = phone ?? "Unknown"
                };

                await dbContext.Guests.AddAsync(guest);
                await dbContext.SaveChangesAsync();

                var room = await dbContext.Rooms.FindAsync(newBookingDto.RoomId);
                if (room == null)
                    return NotFound("The Room ID is not correct.");

                if (room.Status != "Available")
                    return BadRequest("The room is not available.");

                var newBooking = new Booking
                {
                    GuestId = guest.Id,
                    RoomId = newBookingDto.RoomId,
                    CheckinDate = checkinDate,
                    CheckoutDate = checkoutDate,
                    TotalPrice = newBookingDto.TotalPrice
                };

                dbContext.Bookings.Add(newBooking);
                await dbContext.SaveChangesAsync();

                room.Status = "Occupied";
                await dbContext.SaveChangesAsync();

                var paymentMethod = method.ToUpper();
                var Status = "NO";
                if (paymentMethod == "CASH")
                {
                    Status = "NO";
                }
                else if (paymentMethod == "CARD")
                {
                    Status = "YES";
                }

                var newPayment = new Payment
                {
                    BookingId = newBooking.Id,
                    Amount = newBookingDto.TotalPrice,
                    PaymentDate = checkoutDate,
                    Method = method,
                    StatusDone = Status,
                };

                dbContext.Payments.Add(newPayment);
                await dbContext.SaveChangesAsync();

                string notificationMessage = $"New booking created for guest {guest.Name} in room {room.RoomNumber}.";
                await notificationService.CreateNotificationAsync(userId, notificationMessage);

                var bookingDto = new BookingDTO
                {
                    GuestName = guest.Name,
                    RoomNumber = room.RoomNumber,
                    CheckinDate = newBooking.CheckinDate.ToString("yyyy-MM-dd"),
                    CheckoutDate = newBooking.CheckoutDate.ToString("yyyy-MM-dd"),
                    TotalPrice = newBooking.TotalPrice
                };
                var response = new
                {
                    Message = "Add Booking Success",
                    Data = bookingDto
                };

                return CreatedAtAction(nameof(GetAllBookings), new { id = newBooking.Id }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while adding the booking.");
            }
        }


        [HttpPut("Update_Booking/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateBooking(int id, [FromBody] UpdateBookingDTO updatedBookingDto)
        {
            try
            {
                if (updatedBookingDto == null)
                    return BadRequest("Invalid booking data.");

                var existingBooking = await dbContext.Bookings.FindAsync(id);
                if (existingBooking == null)
                    return NotFound("Booking not found.");

                if (!DateTime.TryParse(updatedBookingDto.CheckinDate, out DateTime checkinDate))
                    return BadRequest("Invalid check-in date format. Please use 'yyyy-MM-dd'.");

                if (!DateTime.TryParse(updatedBookingDto.CheckoutDate, out DateTime checkoutDate))
                    return BadRequest("Invalid check-out date format. Please use 'yyyy-MM-dd'.");

                if (checkoutDate <= checkinDate)
                    return BadRequest("Check-out date must be after check-in date.");

                existingBooking.GuestId = updatedBookingDto.GuestId;
                existingBooking.RoomId = updatedBookingDto.RoomId;
                existingBooking.CheckinDate = DateTime.Parse(updatedBookingDto.CheckinDate);
                existingBooking.CheckoutDate = DateTime.Parse(updatedBookingDto.CheckoutDate);
                existingBooking.TotalPrice = updatedBookingDto.TotalPrice;

                await dbContext.SaveChangesAsync();

                var userClaims = User.Claims;

                var userIdClaim = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized("User ID not found in token.");
                }

                string userId = userIdClaim;

                string notificationMessage = $"Update your booking with new info.";
                await notificationService.CreateNotificationAsync(userId, notificationMessage);

                return Ok("Update Booking Success");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while updating the booking.");
            }
        }


        [HttpDelete("Delete_Booking/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            try
            {
                var booking = await dbContext.Bookings.FindAsync(id);
                if (booking == null)
                    return NotFound("Booking not found.");

                dbContext.Bookings.Remove(booking);
                await dbContext.SaveChangesAsync();

                var userClaims = User.Claims;

                var userIdClaim = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized("User ID not found in token.");
                }

                string userId = userIdClaim;

                string notificationMessage = $"your booking is Deleted.";
                await notificationService.CreateNotificationAsync(userId, notificationMessage);

                return Ok("Dleleted Success");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while deleting the booking.");
            }
        }
    }
}
