﻿using Hotel_Backend_API.Data;
using Hotel_Backend_API.DTO.Payment;
using Hotel_Backend_API.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Backend_API.Controllers
{
    [Route("Admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminPaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public AdminPaymentsController(ApplicationDbContext dbContext)
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
                                              hotelName = p.Booking.Room.Hotel.Name
                                          })
                                          .FirstOrDefaultAsync();

            return Ok(payment);
        }


        [HttpGet("GetAll")]
        public async Task<IActionResult> GetPaymentsByHotelId(int pageNumber = 1, int pageSize = 10)
        {
            var totalPayments = await dbContext.Payments
                  .Include(p => p.Booking)
                  .ThenInclude(b => b.Room)
                  .ThenInclude(r => r.Hotel)
                  .CountAsync();

            var payments = await dbContext.Payments
                  .Include(p => p.Booking)
                  .ThenInclude(b => b.Room)
                  .ThenInclude(r => r.Hotel)
                  .Skip((pageNumber - 1) * pageSize)
                  .Take(pageSize)
                  .Select(p => new PaymentDTO
                  {
                      Id = p.Id,
                      BookingId = p.BookingId,
                      Amount = p.Amount,
                      PaymentDate = p.PaymentDate.ToString("yyyy-MM-dd"),
                      StatusDone = p.StatusDone,
                      hotelName = p.Booking.Room.Hotel.Name
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
