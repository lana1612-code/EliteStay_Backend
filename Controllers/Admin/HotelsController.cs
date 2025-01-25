using FluentValidation;
using Hotel_Backend_API.Data;
using Hotel_Backend_API.DTO.Booking;
using Hotel_Backend_API.DTO.Hotel;
using Hotel_Backend_API.Models;
using Hotel_Backend_API.Services;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace Hotel_Backend_API.Controllers
{
    [Route("Admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminHotelsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly HotelService hotelService;

        public AdminHotelsController(ApplicationDbContext dbContext,
                                HotelService hotelService)
        {
            this.dbContext = dbContext;
            this.hotelService = hotelService;
        }

        [HttpGet("GetAllID")]
        public async Task<IActionResult> GetAllIdHotel()
        {
            var ids = await dbContext.Hotels
                                   .Select(h => h.Id)
                                   .ToListAsync();
                                  
            var response = new
            {
                Id = ids
            };
                                  
            return Ok(response);
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var totalHotels = await dbContext.Hotels.CountAsync();

                var hotels = await dbContext.Hotels
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

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
                return StatusCode(500, "An error occurred while retrieving hotels.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var hotel = await hotelService.GetHotelByIdAsync(id);

                var response = hotel.Adapt<AddHotel>();
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500, "an error while get hotel");
            }
        }

        [HttpGet("ByStars")]
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

        [HttpGet("ByAddress")]
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

        [HttpPost()]
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
                newhotel.Id = hotel.Id;
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHotel(int id,[FromServices] IValidator<AddHotel> validator,
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

                if (dbContext.Hotels.Where(h => h.Id != id).AsEnumerable()
                   .Any(h => h.Phone.Equals(updatehotel.Phone, StringComparison.OrdinalIgnoreCase)))
                {
                    return BadRequest("This phone number is already in use.");
                }
           
           
                if (dbContext.Hotels.Where(h => h.Id != id).AsEnumerable()
                   .Any(h => h.Email.Equals(updatehotel.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    return BadRequest("This email address is already in use.");
                }
                /*
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
                */

                hotel.Name = updatehotel.Name;
                hotel.Address = updatehotel.Address;
                hotel.Phone = updatehotel.Phone;
                hotel.Stars = updatehotel.Stars;
                hotel.Email = updatehotel.Email;
                hotel.Tags = updatehotel.Tags;
                hotel.profileIMG = updatehotel.profileIMG;

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

        [HttpDelete("{id}")]
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


        [HttpGet("recommendationUsingTags")]
        public async Task<IActionResult> GetRecommendations([FromQuery] string tagsSearchString, int numOfRecommendations = 10)
        {
            if (string.IsNullOrWhiteSpace(tagsSearchString))
            {
                return BadRequest("Search string must not be empty.");
            }
            try
            {
                var recommendations = await hotelService.GetHotelRecommendationsByTagsAsync(tagsSearchString, numOfRecommendations);
                
                var response  = recommendations.Adapt<IEnumerable<AddHotel>>();

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        /*******************/
        [HttpPost("import")]
        public async Task<IActionResult> ImportHotels(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty.");

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        if (worksheet == null)
                            return BadRequest("No worksheet found in the Excel file.");

                        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                        {
                            var hotel = new Hotel
                            {
                                Name = worksheet.Cells[row, 1].Text,
                                Address = worksheet.Cells[row, 2].Text,
                                Stars = int.Parse(worksheet.Cells[row, 3].Text),
                                profileIMG = worksheet.Cells[row, 4].Text,
                                Tags = worksheet.Cells[row, 5].Text,
                                Phone = worksheet.Cells[row, 6].Text,
                                Email = worksheet.Cells[row, 7].Text,
                            };

                            dbContext.Hotels.Add(hotel);
                        }

                        await dbContext.SaveChangesAsync();
                    }
                }

                return Ok("Hotels have been successfully imported.");
            }
            catch (Exception e)
            {
                return StatusCode(500, $"An error occurred: {e.Message}");
            }
        }

        /*******************/
        [HttpPost("importRooms")]
        public async Task<IActionResult> ImportRooms(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty.");

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        if (worksheet == null)
                            return BadRequest("No worksheet found in the Excel file.");

                        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                        {
                            var room = new Room
                            {
                                HotelId = int.Parse( worksheet.Cells[row, 1].Text),
                                RoomTypeId = int.Parse(worksheet.Cells[row, 2].Text),
                                RoomNumber = worksheet.Cells[row, 3].Text,
                                Status = worksheet.Cells[row, 4].Text,
                            };

                            dbContext.Rooms.Add(room);
                        }

                        await dbContext.SaveChangesAsync();
                    }
                }

                return Ok("Rooms have been successfully imported.");
            }
            catch (Exception e)
            {
                if (e.InnerException != null)
                {
                    return StatusCode(500, $"An error occurred: {e.Message}, Inner Exception: {e.InnerException.Message}");
                }
                return StatusCode(500, $"An error occurred: {e.Message}");
            }
        }

        /*******************/
        [HttpPost("importRoomTypes")]
        public async Task<IActionResult> ImportRoomTypes(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty.");

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        if (worksheet == null)
                            return BadRequest("No worksheet found in the Excel file.");

                        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                        {
                            var room = new RoomType
                            {
                                Name = worksheet.Cells[row, 1].Text,
                                PricePerNight = decimal.Parse(worksheet.Cells[row, 2].Text),
                                Capacity = int.Parse(worksheet.Cells[row, 3].Text),
                                Description = worksheet.Cells[row, 4].Text,
                                ImageURL = worksheet.Cells[row, 5].Text,
                            };

                            dbContext.RoomTypes.Add(room);
                        }

                        await dbContext.SaveChangesAsync();
                    }
                }

                return Ok("Room Types have been successfully imported.");
            }
            catch (Exception e)
            {
                return StatusCode(500, $"An error occurred: {e.Message}");
            }
        }

        /*******************/
        [HttpPost("importComplaint")]
        public async Task<IActionResult> ImportComplaint(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty.");

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        if (worksheet == null)
                            return BadRequest("No worksheet found in the Excel file.");

                        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                        {
                            var room = new Complaint
                            {
                                UserId = worksheet.Cells[row, 1].Text,
                                HotelId = int.Parse(worksheet.Cells[row, 2].Text),
                                Content = worksheet.Cells[row, 3].Text,
                                CreatedAt =DateTime.Parse( worksheet.Cells[row, 4].Text),
                            };

                            dbContext.Complaints.Add(room);
                        }

                        await dbContext.SaveChangesAsync();
                    }
                }

                return Ok("Complaints have been successfully imported.");
            }
            catch (Exception e)
            {
                return StatusCode(500, $"An error occurred: {e.Message}");
            }
        }

        /*******************/
        [HttpPost("importComment")]
        public async Task<IActionResult> ImportComment(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty.");

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        if (worksheet == null)
                            return BadRequest("No worksheet found in the Excel file.");

                        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                        {
                            var room = new Comment
                            {
                                UserId = worksheet.Cells[row, 1].Text,
                                HotelId = int.Parse(worksheet.Cells[row, 2].Text),
                                Content = worksheet.Cells[row, 3].Text,
                                CreatedAt = DateTime.Parse(worksheet.Cells[row, 4].Text),
                            };

                            dbContext.Comments.Add(room);
                        }

                        await dbContext.SaveChangesAsync();
                    }
                }

                return Ok("Comments have been successfully imported.");
            }
            catch (Exception e)
            {
                return StatusCode(500, $"An error occurred: {e.Message}");
            }
        }

        /*******************/
        [HttpPost("importRating")]
        public async Task<IActionResult> ImportRating(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty.");

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        if (worksheet == null)
                            return BadRequest("No worksheet found in the Excel file.");

                        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                        {
                            var room = new Rating
                            {
                                UserId = worksheet.Cells[row, 1].Text,
                                HotelId = int.Parse(worksheet.Cells[row, 2].Text),
                                RatingValue = double.Parse( worksheet.Cells[row, 3].Text),
                                RatedAt = DateTime.Parse(worksheet.Cells[row, 4].Text),
                            };

                            dbContext.Ratings.Add(room);
                        }

                        await dbContext.SaveChangesAsync();
                    }
                }

                return Ok("Ratings have been successfully imported.");
            }
            catch (Exception e)
            {
                return StatusCode(500, $"An error occurred: {e.Message}");
            }
        }




      // add data from api
      /* Guest
       * AdminHotel
       * Payment
       * Booking
       */
    }
}
