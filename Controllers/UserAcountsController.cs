using Hotel_Backend_API.Data;
using Hotel_Backend_API.DTO.Acount;
using Hotel_Backend_API.DTO.sendemail;
using Hotel_Backend_API.Models;
using Hotel_Backend_API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Backend_API.Controllers
{
    [Route("Admin/[controller]")]
    [ApiController]
    public class UserAcountsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly SendEmailService sendEmailService;
        private readonly UserManager<AppUser> userManager;

        public UserAcountsController(ApplicationDbContext dbContext, UserManager<AppUser> userManager,
            SendEmailService sendEmailService)
        {
            this.dbContext = dbContext;
            this.sendEmailService = sendEmailService;
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

        [HttpPost]
        public async Task<IActionResult> SendEmail()
        {
                var receiver = "lanahasan1712@gmail.com";
                var subject = "Test";
                var message = "Hello World";
                await sendEmailService.SendEmail(receiver, subject, message);
                return Ok();
        }


    }
}
