using Hotel_Backend_API.Data;
using Hotel_Backend_API.DTO.Acount;
using Hotel_Backend_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Backend_API.Controllers
{
    [Route("Admin/[controller]")]
    [ApiController]
    public class UserAcountsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<AppUser> userManager;

        public UserAcountsController(ApplicationDbContext dbContext, UserManager<AppUser> userManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> getUsers()
        {
            var allUsers = await dbContext.Users.ToListAsync();
            var result = new List<UserDto>();

            foreach (var user in allUsers)
            {
                var roles = await userManager.GetRolesAsync(user);
                if (roles.Contains("Normal"))
                {
                    result.Add(new UserDto
                    {
                        Username = user.UserName,
                        Email = user.Email,
                        Phone = user.PhoneNumber,
                        ImageProfile = user.imgUser
                    });
                }
            }
            return Ok(new
            {
                size = result.Count,
                data = result
            });
        }

    }
}
