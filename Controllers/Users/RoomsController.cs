using Hotel_Backend_API.Data;
using Hotel_Backend_API.DTO.Booking;
using Hotel_Backend_API.DTO.Room;
using Hotel_Backend_API.Models;
using Hotel_Backend_API.Services;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Backend_API.Controllers.Users
{
    [Route("Normal/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly RoomService roomService;

        public RoomsController(ApplicationDbContext dbContext, RoomService roomService)
        {
            this.dbContext = dbContext;
            this.roomService = roomService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllRooms(int pageNumber = 1, int pageSize = 10)
        {
            var totalRooms = await dbContext.Rooms.CountAsync();

            var rooms = await dbContext.Rooms
                                       .Include(r => r.Hotel)
                                       .Include(r => r.RoomType)
                                       .Skip((pageNumber - 1) * pageSize)
                                       .Take(pageSize)
                                       .Select(r => new RoomDTO
                                       {
                                           Id = r.Id,
                                           NameHotel = r.Hotel.Name,
                                           NameRoomType = r.RoomType.Name,
                                           RoomNumber = r.RoomNumber,
                                           Status = r.Status,
                                           PricePerNight = r.RoomType.PricePerNight,
                                           Description = r.RoomType.Description,
                                           Capacity = r.RoomType.Capacity,
                                           ImageURL = r.RoomType.ImageURL
                                       })
                                       .ToListAsync();

            var response = new
            {
                TotalCount = totalRooms,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(totalRooms / (double)pageSize),
                Data = rooms
            };

            return Ok(response);
        }


        [HttpGet("GetAll_available")]
        public async Task<IActionResult> GetAllAvailableRooms(int pageNumber = 1, int pageSize = 10)
        {
            var totalRooms = await dbContext.Rooms
                                             .Where(r => r.Status == "available")
                                             .CountAsync();

            var rooms = await dbContext.Rooms
                                       .Include(r => r.Hotel)
                                       .Include(r => r.RoomType)
                                       .Where(r => r.Status == "available")
                                       .Skip((pageNumber - 1) * pageSize)
                                       .Take(pageSize)
                                       .Select(r => new RoomDTO
                                       {
                                           Id = r.Id,
                                           NameHotel = r.Hotel.Name,
                                           NameRoomType = r.RoomType.Name,
                                           RoomNumber = r.RoomNumber,
                                           Status = r.Status,
                                           Description = r.RoomType.Description,
                                           Capacity = r.RoomType.Capacity,
                                           ImageURL = r.RoomType.ImageURL
                                       })
                                       .ToListAsync();

            var response = new
            {
                TotalCount = totalRooms,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(totalRooms / (double)pageSize),
                Data = rooms
            };

            return Ok(response);
        }


        [HttpGet("ByCapacity")]
        public async Task<IActionResult> GetRoomsByCapacity(int capacity, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var totalRooms = await dbContext.Rooms
                    .Include(r => r.RoomType)
                    .Where(r => r.RoomType.Capacity == capacity)
                    .CountAsync();

                var rooms = await dbContext.Rooms
                    .Include(r => r.Hotel)
                    .Include(r => r.RoomType)
                    .Where(r => r.RoomType.Capacity == capacity)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new RoomDTO
                    {
                        Id = r.Id,
                        NameHotel = r.Hotel.Name,
                        NameRoomType = r.RoomType.Name,
                        RoomNumber = r.RoomNumber,
                        Status = r.Status.ToString(),
                        Description = r.RoomType.Description,
                        Capacity = r.RoomType.Capacity,
                        ImageURL = r.RoomType.ImageURL
                    })
                    .ToListAsync();

                var response = new
                {
                    TotalCount = totalRooms,
                    PageSize = pageSize,
                    CurrentPage = pageNumber,
                    TotalPages = (int)Math.Ceiling(totalRooms / (double)pageSize),
                    Data = rooms
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving rooms by capacity.");
            }
        }


        [HttpGet("recommendation_Description")]
        public async Task<IActionResult> GetRecommendations([FromQuery] string descriptionSearchString, int numOfRecommendations = 100)
        {
            if (string.IsNullOrWhiteSpace(descriptionSearchString))
            {
                return BadRequest("Search string must not be empty.");
            }

            try
            {
                var recommendations = await roomService.GetRoomTypeRecommendationsByDescriptionAsync(descriptionSearchString, numOfRecommendations);

                var recommendedRoomTypeIds = recommendations.Select(r => r.Id).ToList();

                var rooms = await dbContext.Rooms
                                           .Include(r => r.Hotel)
                                           .Include(r => r.RoomType)
                                           .Where(r => recommendedRoomTypeIds.Contains(r.RoomTypeId))
                                           .Select(r => new RoomDTO
                                           {
                                               Id = r.Id,
                                               NameHotel = r.Hotel.Name,
                                               NameRoomType = r.RoomType.Name,
                                               RoomNumber = r.RoomNumber,
                                               Status = r.Status,
                                               PricePerNight = r.RoomType.PricePerNight,
                                               Description = r.RoomType.Description,
                                               Capacity = r.RoomType.Capacity,
                                               ImageURL = r.RoomType.ImageURL
                                           })
                                           .ToListAsync();

                return Ok(rooms);

            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while fetching recommendations.");
            }
        }


    }
}
