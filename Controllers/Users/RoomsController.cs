using Hotel_Backend_API.Data;
using Hotel_Backend_API.DTO.Booking;
using Hotel_Backend_API.DTO.Room;
using Hotel_Backend_API.DTO.RoomType;
using Hotel_Backend_API.Models;
using Hotel_Backend_API.Services;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;

namespace Hotel_Backend_API.Controllers.Users
{
    [Route("Normal/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly RoomService roomService;
        private readonly LikeRecommendedService likeRecommendedService;
        private readonly SaveRecommendedService saveRecommendedService;

        public RoomsController(ApplicationDbContext dbContext, RoomService roomService,
            LikeRecommendedService likeRecommendedService,
            SaveRecommendedService saveRecommendedService)
        {
            this.dbContext = dbContext;
            this.roomService = roomService;
            this.likeRecommendedService = likeRecommendedService;
            this.saveRecommendedService = saveRecommendedService;
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
                                       .Select(r => new RoomDTOUser
                                       {
                                           Id = r.Id,
                                           IdHotel = r.Hotel.Id,
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
                                       .Select(r => new RoomDTOUser
                                       {
                                           Id = r.Id,
                                           IdHotel = r.Hotel.Id,
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
                    .Select(r => new RoomDTOUser
                    {
                        Id = r.Id,
                        IdHotel = r.Hotel.Id,
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
       
        
        [HttpGet("recommendation/Description")]
        public async Task<IActionResult> GetRecommendations([FromQuery] string descriptionSearchString,
                                             [FromQuery] int pageNumber = 1,
                                             [FromQuery] int pageSize = 100)
        {
            if (string.IsNullOrWhiteSpace(descriptionSearchString))
            {
                return BadRequest("Search string must not be empty.");
            }

            try
            {
                var recommendations = await roomService.GetRoomTypeRecommendationsByDescriptionAsync(descriptionSearchString);

                var recommendedRoomTypeIds = recommendations.Select(r => r.Id).ToList();

                var totalRooms = await dbContext.Rooms
                    .Where(r => recommendedRoomTypeIds.Contains(r.RoomTypeId))
                    .CountAsync(); 

                var rooms = await dbContext.Rooms
                    .Include(r => r.Hotel)
                    .Include(r => r.RoomType)
                    .Where(r => recommendedRoomTypeIds.Contains(r.RoomTypeId))
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new RoomDTOUser
                    {
                        Id = r.Id,
                        IdHotel = r.Hotel.Id,
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
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while fetching recommendations.");
            }
        }

        [HttpGet("recommendation/Like")]
        public async Task<IActionResult> GetRecommendations([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            string userId = userIdClaim;

            var likeRoom = await likeRecommendedService.GetLikedRoomsAsync(userId);

            if (likeRoom == null || !likeRoom.Any())
            {
                return Ok(new
                {
                    TotalCount = 0,
                    PageSize = pageSize,
                    CurrentPage = pageNumber,
                    TotalPages = 0,
                    message = "No recommendations available.",
                    Data = new List<object>() 
                });
            }

            var roomTypeIds = likeRoom.Select(r => r.RoomTypeId).ToList();

            var roomTypes = new List<RoomType>();

            foreach (var roomTypeId in roomTypeIds)
            {
                var roomType = await dbContext.RoomTypes
                                           .Where(rt => rt.Id == roomTypeId)
                                           .FirstOrDefaultAsync();

                var recommendations = await roomService.GetRoomTypeRecommendationsByDescriptionAsync(roomType.Description);

                roomTypes.AddRange(recommendations);
            }
            var recommendedRoomTypeIds = roomTypes.Select(r => r.Id).ToList();

            var totalRooms = await dbContext.Rooms
                .Where(r => recommendedRoomTypeIds.Contains(r.RoomTypeId))
                .CountAsync();

            var rooms = await dbContext.Rooms
                .Include(r => r.Hotel)
                .Include(r => r.RoomType)
                .Where(r => recommendedRoomTypeIds.Contains(r.RoomTypeId))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RoomDTOUser
                {
                    Id = r.Id,
                    IdHotel = r.Hotel.Id,
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
                message = "You might like",
                Data = rooms
            };

            return Ok(response);
        }

        [HttpGet("recommendation/SaveRoom")]
        public async Task<IActionResult> GetRecommendationsSaveRoom([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            string userId = userIdClaim;

            var saveRoom = await saveRecommendedService.GetSaveRoomsAsync(userId);

            if (saveRoom == null || !saveRoom.Any())
            {
                return Ok(new
                {
                    TotalCount = 0,
                    PageSize = pageSize,
                    CurrentPage = pageNumber,
                    TotalPages = 0,
                    message = "No recommendations available.",
                    Data = new List<object>()
                });
            }

            var roomTypeIds = saveRoom.Select(r => r.RoomTypeId).ToList();

            var roomTypes = new List<RoomType>();

            foreach (var roomTypeId in roomTypeIds)
            {
                var roomType = await dbContext.RoomTypes
                                           .Where(rt => rt.Id == roomTypeId)
                                           .FirstOrDefaultAsync();

                var recommendations = await roomService.
                    GetRoomTypeRecommendationsByDescriptionAsync(roomType.Description);

                roomTypes.AddRange(recommendations);
            }
            var recommendedRoomTypeIds = roomTypes.Select(r => r.Id).ToList();

            var totalRooms = await dbContext.Rooms
                .Where(r => recommendedRoomTypeIds.Contains(r.RoomTypeId))
                .CountAsync();

            var rooms = await dbContext.Rooms
                .Include(r => r.Hotel)
                .Include(r => r.RoomType)
                .Where(r => recommendedRoomTypeIds.Contains(r.RoomTypeId))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RoomDTOUser
                {
                    Id = r.Id,
                    IdHotel = r.Hotel.Id,
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
                message = "You might like",
                Data = rooms
            };

            return Ok(response);
        }

    }
}

