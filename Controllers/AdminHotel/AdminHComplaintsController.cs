﻿using Hotel_Backend_API.Data;
using Hotel_Backend_API.DTO.Acount;
using Hotel_Backend_API.DTO.Complain;
using Hotel_Backend_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Hotel_Backend_API.Controllers
{
    [Route("AdminHotel/[controller]")]
    [ApiController]
    [Authorize(Roles = "AdminHotel")]
    public class AdminHComplaintsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<AppUser> userManager;

        public AdminHComplaintsController(ApplicationDbContext dbContext, UserManager<AppUser> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }

        
        [HttpGet("GetAll/{hotelId}")]
        public async Task<IActionResult> GetComplaints(int hotelId)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            var complaints = await dbContext.Complaints
                  .Where(c => c.HotelId == hotelId)
                  .ToListAsync();
            var hotel = await dbContext.Hotels
                  .FirstOrDefaultAsync(c => c.Id == hotelId);
            List<ReturnComplainDTO> returnComplainDto = new List<ReturnComplainDTO>();
            foreach(var Complain in complaints)
            {
                var user = await userManager.FindByIdAsync(Complain.UserId);
                var complaindto = new ReturnComplainDTO 
                {
                    Id = Complain.Id,
                    EmailUser = user.Email,
                    NameUser = user.UserName,
                    hotelName = hotel.Name,
                    imgUser = user.imgUser,
                    Content = Complain.Content,
                    CreatedAt = Complain.CreatedAt,
                };
                returnComplainDto.Add(complaindto);
            }
            var response = new {
            data = returnComplainDto
            };


            return Ok(response);
        }
   
    }
}
