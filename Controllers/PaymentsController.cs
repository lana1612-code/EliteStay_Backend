using Hotel_Backend_API.Data;
using Hotel_Backend_API.DTO.Payment;
using Hotel_Backend_API.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Backend_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "AdminHotel,Admin")]
    public class PaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public PaymentsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }


        [HttpGet("get_All_Payments")]
        public async Task<IActionResult> GetAllPayments(int pageNumber = 1, int pageSize = 10)
        {
            var totalPayments = await dbContext.Payments.CountAsync();
            var payments = await dbContext.Payments
                                           .Skip((pageNumber - 1) * pageSize)
                                           .Take(pageSize)
                                           .Select(p => new PaymentDTO
                                           {
                                               BookingId = p.BookingId,
                                               Amount = p.Amount,
                                               PaymentDate = p.PaymentDate.ToString("yyyy-MM-dd"),
                                               StatusDone = "Yes",
                                           })
                                           .ToListAsync();

            if (!payments.Any())
                return BadRequest("No payments found.");

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


        [HttpGet("get_Payment_by_id/{id}")]
        public async Task<IActionResult> GetPaymentById(int id)
        {
            var payment = await dbContext.Payments
                                          .Where(p => p.Id == id)
                                          .Select(p => new PaymentDTO
                                          {
                                              BookingId = p.BookingId,
                                              Amount = p.Amount,
                                              PaymentDate = p.PaymentDate.ToString("yyyy-MM-dd"),
                                              StatusDone = "Yes",
                                          })
                                          .FirstOrDefaultAsync();

            if (payment == null)
                return NotFound("Payment not found.");

            return Ok(payment);
        }


        [HttpGet("get_Payments_by_Hotel/{hotelId}")]
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
                {
                    BookingId = p.BookingId,
                    Amount = p.Amount,
                    PaymentDate = p.PaymentDate.ToString("yyyy-MM-dd"),
                    StatusDone = "Yes",
                })
                .ToListAsync();

            if (!payments.Any())
                return BadRequest("No payments found for this hotel.");

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


        [HttpPut("update_payment/{id}")]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] UpdatePaymentDTO updatePaymentDto)
        {
            try
            {
                if (updatePaymentDto == null)
                    return BadRequest("Payment data is invalid.");

                var payment = await dbContext.Payments.FindAsync(id);
                if (payment == null)
                    return NotFound($"Payment with id [{id}] not found.");

                if (!DateTime.TryParse(updatePaymentDto.PaymentDate, out DateTime checkOutDate))
                    return BadRequest("Invalid check-in date format. Please use 'yyyy-MM-dd'.");


                payment.Amount = updatePaymentDto.Amount;
                payment.PaymentDate = checkOutDate;
                payment.StatusDone = "Yes";

                dbContext.Payments.Update(payment);
                await dbContext.SaveChangesAsync();

                var response = new
                {
                    Message = "Update Payment success",
                    Data = updatePaymentDto
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while updating the payment.");
            }
        }


        [HttpDelete("delete_payment/{id}")]
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
