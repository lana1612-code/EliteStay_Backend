using Hotel_Backend_API.Data;
using Hotel_Backend_API.DTO.Payment;
using Hotel_Backend_API.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Backend_API.Controllers
{
    [Route("AdminHotel/[controller]")]
    [ApiController]
    [Authorize(Roles = "AdminHotel")]
    public class AdminHPaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public AdminHPaymentsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentById(int id)
        {
            var payment = await dbContext.Payments
                                          .Where(p => p.Id == id)
                                          .Select(p => new PaymentDTO
                                          {
                                              Id = p.Id,
                                              BookingId = p.BookingId,
                                              Amount = p.Amount,
                                              PaymentDate = p.PaymentDate.ToString("yyyy-MM-dd"),
                                              StatusDone = "Yes",
                                          })
                                          .FirstOrDefaultAsync();

            return Ok(payment);
        }


        [HttpGet("GetAll/{hotelId}")]
        public async Task<IActionResult> GetPaymentsByHotelId(int hotelId, int pageNumber = 1, int pageSize = 10)
        {
            var totalPayments = await dbContext.Payments
                .Include(p => p.Booking)
                .ThenInclude(b => b.Room)
                .Where(p => p.Booking.Room.HotelId == hotelId)
                .CountAsync();

            var payments = await dbContext.Payments
                .Include(p => p.Booking)
                .ThenInclude(b => b.Room)
                .Where(p => p.Booking.Room.HotelId == hotelId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PaymentDTO
                {Id = p.Id,
                    BookingId = p.BookingId,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate.ToString("yyyy-MM-dd"),
                    StatusDone = "Yes",
                })
                .ToListAsync();

            var response = new
            {
                TotalCount = totalPayments,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(totalPayments / (double)pageSize),
                Data = payments
            };

            return Ok(response);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            try
            {
                var payment = await dbContext.Payments.FindAsync(id);
                if (payment == null)
                    return NotFound($"Payment with id [{id}] not found.");

                dbContext.Payments.Remove(payment);
                await dbContext.SaveChangesAsync();
                return Ok($"Payment with id [{id}] deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while deleting the payment.");
            }
        }
    }
}
