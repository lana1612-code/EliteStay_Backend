using FluentValidation;
using Hotel_Backend_API.Data;
using Hotel_Backend_API.DTO.Booking;
using Hotel_Backend_API.DTO.Hotel;
using Hotel_Backend_API.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Backend_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public HotelsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("get_All_Hotel")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var totalHotels = await dbContext.Hotels.CountAsync();

                var hotels = await dbContext.Hotels
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                if (!hotels.Any())
                    return NotFound("No hotels found.");

                var response = new
                {
                    TotalCount = totalHotels,
                    PageSize = pageSize,
                    CurrentPage = pageNumber,
                    TotalPages = (int)Math.Ceiling(totalHotels / (double)pageSize),
                    Data = hotels.Adapt<IEnumerable<AddHotel>>() // Assuming you have a mapping setup  
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving hotels.");
            }
        }

        [HttpGet("get_Hotel_ById")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var hotel = await dbContext.Hotels.FindAsync(id);
                if (hotel == null)
                    return NotFound();

                var response = hotel.Adapt<AddHotel>();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "an error while get hotel");
            }
        }


        [HttpGet("get_Hotel_ByStars")]
        public async Task<IActionResult> GetByStars(int num_stars, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var totalHotels = await dbContext.Hotels.CountAsync(h => h.Stars == num_stars);

                var hotels = await dbContext.Hotels
                    .Where(h => h.Stars == num_stars)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                if (!hotels.Any())
                    return NotFound($"No hotels found that have [{num_stars}] stars.");

                var response = new
                {
                    TotalCount = totalHotels,
                    PageSize = pageSize,
                    CurrentPage = pageNumber,
                    TotalPages = (int)Math.Ceiling(totalHotels / (double)pageSize),
                    Data = hotels.Adapt<IEnumerable<AddHotel>>()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving hotels by stars.");
            }
        }

        [HttpGet("get_Hotel_ByAddress")]
        public async Task<IActionResult> GetByAddress(string input_address, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var lower_input_address = input_address.ToLower();
                var totalHotels = await dbContext.Hotels
                    .Where(h => h.Address.ToLower() == lower_input_address)
                    .CountAsync();

                var hotels = await dbContext.Hotels
                    .Where(h => h.Address.ToLower() == lower_input_address)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                if (!hotels.Any())
                    return NotFound($"No hotels found in the location [{input_address}].");

                var response = new
                {
                    TotalCount = totalHotels,
                    PageSize = pageSize,
                    CurrentPage = pageNumber,
                    TotalPages = (int)Math.Ceiling(totalHotels / (double)pageSize),
                    Data = hotels.Adapt<IEnumerable<AddHotel>>()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving hotels by address.");
            }
        }

        [HttpPost("Add_New_Hotel")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddHotel(AddHotel newhotel, [FromServices] IValidator<AddHotel> validator)
        {
            try
            {
                var result = validator.Validate(newhotel);
                if (!result.IsValid)
                {
                    var model = new ModelStateDictionary();
                    result.Errors.ForEach(error =>
                    {
                        model.AddModelError(error.PropertyName, error.ErrorMessage);
                    });
                    return ValidationProblem(model);
                }

                var hotel = newhotel.Adapt<Hotel>();
                if (dbContext.Hotels.AsEnumerable()
                        .Any(h => h.Email.Equals(newhotel.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    return BadRequest("This email address is already in use.");
                }
                if (dbContext.Hotels.AsEnumerable()
                        .Any(h => h.Phone.Equals(newhotel.Phone, StringComparison.OrdinalIgnoreCase)))
                {
                    return BadRequest("This phone number is already in use.");
                }
                await dbContext.Hotels.AddAsync(hotel);
                await dbContext.SaveChangesAsync();

                var response = new
                {
                    Message = "Add Hotel Success",
                    Data = newhotel
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "an error while Add hotel");
            }
        }

        [HttpPut("Update_Hotel")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateHotel(int id,
                                  [FromServices] IValidator<AddHotel> validator,
                                  [FromBody] AddHotel updatehotel)
        {
            try
            {
                var result = validator.Validate(updatehotel);
                if (!result.IsValid)
                {
                    var model = new ModelStateDictionary();
                    result.Errors.ForEach(error =>
                    {
                        model.AddModelError(error.PropertyName, error.ErrorMessage);
                    });
                    return ValidationProblem(model);
                }

                var hotel = await dbContext.Hotels.FindAsync(id);

                if (hotel == null)
                    return NotFound($"No hotel found with id [{id}]");

                if (dbContext.Hotels.AsEnumerable()
                    .Any(h => h.Email.Equals(updatehotel.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    return BadRequest("This email address is already in use.");
                }
                if (dbContext.Hotels.AsEnumerable()
                        .Any(h => h.Phone.Equals(updatehotel.Phone, StringComparison.OrdinalIgnoreCase)))
                {
                    return BadRequest("This phone number is already in use.");
                }

                hotel.Name = updatehotel.Name;
                hotel.Address = updatehotel.Address;
                hotel.Phone = updatehotel.Phone;
                hotel.Stars = updatehotel.Stars;
                hotel.Email = updatehotel.Email;
                hotel.Tags = updatehotel.Tags;

                await dbContext.SaveChangesAsync();
                var response = new
                {
                    Message = "Update hotel Success",
                    Data = updatehotel
                };

                return Ok(updatehotel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while updating the hotel.");
            }
        }

        [HttpDelete("Delete_Hotel")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            try
            {
                var hotel = await dbContext.Hotels.FindAsync(id);
                if (hotel == null)
                {
                    return NotFound($"No hotel found with id [{id}]");
                }

                dbContext.Hotels.Remove(hotel);
                await dbContext.SaveChangesAsync();
                return Ok("Hotel deleted successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while deleting the hotel");
            }

        }

        // [Authorize(Roles = "Admin,Normal")]

    }
}
