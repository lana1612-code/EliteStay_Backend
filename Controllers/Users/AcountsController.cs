using Hotel_Backend_API.Data;
using Hotel_Backend_API.DTO.Acount;
using Hotel_Backend_API.Models;
using Hotel_Backend_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Hotel_Backend_API.Controllers
{
    [Route("Normal/[controller]")]
    [ApiController]
    public class AcountsController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly AuthService authService;
        private readonly ApplicationDbContext dbContext;

        public AcountsController(UserManager<AppUser> userManager,
                 SignInManager<AppUser> signInManager,
                 AuthService authService,
                 ApplicationDbContext dbContext)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.authService = authService;
            this.dbContext = dbContext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new AppUser
            {
                UserName = registerDto.Email.Split('@')[0],
                Email = registerDto.Email,
                PhoneNumber = registerDto.Phone
            };

            var result = await userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Errors = errors });
            }

            return Ok(new { message = "Sign up Success" });
        }


        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDTO loginDTO)
        {
            var user = await userManager.FindByEmailAsync(loginDTO.Email);
            if (user == null)
                return Unauthorized(new { message = "you need to sign up !!" });

            var reult = await signInManager.CheckPasswordSignInAsync(user, loginDTO.Password, false);

            if (!reult.Succeeded)
                return Unauthorized();


           await userManager.AddToRoleAsync(user, "Normal");

            return Ok(new
            {
                Message = "Login Success",
                UserInfo = new
                {
                    Username = user.UserName,
                    Email = user.Email,
                    Phone = user.PhoneNumber,
                    Role = "Normal",
                    Token = await authService.CreateTokenAsync(user, userManager)

                }
            });

        }


        [HttpPost("AddProfileImg")]
        public async Task<IActionResult> AddProfileImg([FromForm] IFormFile img)
        {
            try
            {
                if (img == null || img.Length == 0)
                {
                    return Ok();
                }
                if (!img.ContentType.Contains("image"))
                {
                    return BadRequest("Invalid image format.");
                }
                var userClaims = User.Claims;
                var userIdClaim = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized("User ID not found in token.");
                }

                var user = await userManager.FindByIdAsync(userIdClaim);
                if (user == null)
                {
                    return NotFound("User not found.");
                }
                var username = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var email = userClaims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

                string rootFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "images\\profiles");
                Directory.CreateDirectory(rootFolderPath);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
                string filePath = Path.Combine(rootFolderPath, fileName);
               
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await img.CopyToAsync(stream);
                }
                
                user.imgUser = filePath;
                var result = await userManager.UpdateAsync(user);

                var serach = await dbContext.Guests
                .FirstOrDefaultAsync(b => b.Name == username);

                if (!result.Succeeded)
                {
                    return StatusCode(500, "An error occurred while adding the profile image.");
                }

                var guest = await dbContext.Guests.FirstOrDefaultAsync(b => b.Email == email);
                guest.imgUser = filePath;
                await dbContext.SaveChangesAsync();

                return Ok(new { Message = "Image uploaded successfully.", ImagePath = user.imgUser });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

    }
}
