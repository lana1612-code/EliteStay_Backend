﻿using Hotel_Backend_API.Data;
using Hotel_Backend_API.DTO.RoomType;
using Hotel_Backend_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace Hotel_Backend_API.Controllers
{
    [Route("Admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminRoomTypesController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public AdminRoomTypesController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllRoomTypes(int pageNumber = 1, int pageSize = 10)
        {
            var totalRoomTypes = await dbContext.RoomTypes.CountAsync();
            var roomTypes = await dbContext.RoomTypes
                .Join(
                      dbContext.Rooms, rt => rt.Id, r => r.RoomTypeId, (rt, r) => 
                      new { RoomType = rt, Room = r })
                .Join(
                      dbContext.Hotels, rr => rr.Room.HotelId, h => h.Id, (rr, h) =>
                      new { rr.RoomType, Hotel = h })
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(rh => new ReturnRoomTypeDTO
                {
                    Id = rh.RoomType.Id,
                    Name = rh.RoomType.Name,
                    hotelName = rh.Hotel.Name,
                    PricePerNight = rh.RoomType.PricePerNight,
                    Capacity = rh.RoomType.Capacity,
                    Description = rh.RoomType.Description,
                    ImageURL = rh.RoomType.ImageURL
                })
                .ToListAsync();

            var response = new
            {
                TotalCount = totalRoomTypes,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(totalRoomTypes / (double)pageSize),
                Data = roomTypes
            };

            return Ok(response);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoomTypeById(int id)
        {
            var roomType = await dbContext.RoomTypes
                                   .Join(
                                       dbContext.Rooms, rt => rt.Id, r => r.RoomTypeId, (rt, r) => 
                                       new { RoomType = rt, Room = r })
                                   .Join(
                                       dbContext.Hotels, rr => rr.Room.HotelId, h => h.Id, (rr, h) =>
                                       new { rr.RoomType, Hotel = h })
                                   .Where(rh => rh.RoomType.Id == id)
                                   .Select(rh => new ReturnRoomTypeDTO
                                   {
                                       Id = rh.RoomType.Id,
                                       Name = rh.RoomType.Name,
                                       hotelName = rh.Hotel.Name,
                                       PricePerNight = rh.RoomType.PricePerNight,
                                       Capacity = rh.RoomType.Capacity,
                                       Description = rh.RoomType.Description,
                                       ImageURL = rh.RoomType.ImageURL
                                   })
                                   .FirstOrDefaultAsync();


            return Ok(roomType);
        }


        [HttpGet("GetAll/{hotelId}")]
        public async Task<IActionResult> GetRoomTypesInHotel(int hotelId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var roomTypeIds = await dbContext.Rooms
                    .Where(r => r.HotelId == hotelId)
                    .Select(r => r.RoomTypeId)
                    .ToListAsync();
                //  return Ok(roomTypeIds);
                var totalRoomTypes = await dbContext.RoomTypes
                    .Where(rt => roomTypeIds.Contains(rt.Id))
                    .CountAsync();

                var roomTypes = await dbContext.RoomTypes
                    .Where(rt => roomTypeIds.Contains(rt.Id))
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(rt => new ReturnRoomTypeDTO
                    {
                        Id = rt.Id,
                        Name = rt.Name,
                        PricePerNight = rt.PricePerNight,
                        Capacity = rt.Capacity,
                        Description = rt.Description,
                        ImageURL = rt.ImageURL
                    })
                    .ToListAsync();

                var response = new
                {
                    TotalCount = totalRoomTypes,
                    PageSize = pageSize,
                    CurrentPage = pageNumber,
                    TotalPages = (int)Math.Ceiling(totalRoomTypes / (double)pageSize),
                    Data = roomTypes
                };

                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving room types in the hotel.");
            }
        }


        [HttpPost()]
        public async Task<IActionResult> AddRoomType(RoomTypeDTO newRoomTypeDto)
        {
            try
            {
                if (newRoomTypeDto == null)
                    return BadRequest("Room type data is required.");

                var newRoomType = new RoomType
                {
                    Name = newRoomTypeDto.Name,
                    PricePerNight = newRoomTypeDto.PricePerNight,
                    Capacity = newRoomTypeDto.Capacity,
                    Description = newRoomTypeDto.Description,
                    ImageURL = newRoomTypeDto.ImageURL
                };

                dbContext.RoomTypes.Add(newRoomType);
                await dbContext.SaveChangesAsync();

                var roomTypeDto = new ReturnRoomTypeDTO
                {
                    Id = newRoomType.Id,
                    Name = newRoomType.Name,
                    PricePerNight = newRoomType.PricePerNight,
                    Capacity = newRoomType.Capacity,
                    Description = newRoomType.Description,
                    ImageURL = newRoomType.ImageURL
                };

                var response = new
                {
                    Message = "Add Room Type Success",
                    data = roomTypeDto
                };

                return CreatedAtAction(nameof(GetAllRoomTypes), new { id = newRoomType.Id }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while adding the room type.");
            }

        }

        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoomType(int id, RoomTypeDTO updateRoomTypeDto)

        {
            try
            {
                if (updateRoomTypeDto == null)
                    return BadRequest("Room type data is invalid.");

                var roomType = await dbContext.RoomTypes.FindAsync(id);
                if (roomType == null)
                    return NotFound($"Room type with id [{id}] not found.");

                roomType.Name = updateRoomTypeDto.Name;
                roomType.PricePerNight = updateRoomTypeDto.PricePerNight;
                roomType.Capacity = updateRoomTypeDto.Capacity;
                roomType.Description = updateRoomTypeDto.Description;
                roomType.ImageURL = updateRoomTypeDto.ImageURL;

                dbContext.RoomTypes.Update(roomType);
                await dbContext.SaveChangesAsync();

                var roomTypeDto = new RoomType
                {
                    Id = roomType.Id,
                    Name = roomType.Name,
                    PricePerNight = roomType.PricePerNight,
                    Capacity = roomType.Capacity,
                    Description = roomType.Description,
                    ImageURL = roomType.ImageURL
                };
                var response = new
                {
                    Message = "Update Room Type Success",
                    Data = roomTypeDto
                };
                return Ok(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while deleting the Room Type");
            }
        }

        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoomType(int id)
        {
            try
            {
                var roomType = await dbContext.RoomTypes.FindAsync(id);
                if (roomType == null)
                    return NotFound($"Room type with id [{id}] not found.");

                dbContext.RoomTypes.Remove(roomType);
                await dbContext.SaveChangesAsync();
                return Ok($"Room type with id [{id}] deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while deleting the Room Type");
            }
        }

    }
}
