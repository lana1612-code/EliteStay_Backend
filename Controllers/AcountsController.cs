using Hotel_Backend_API.DTO.Acount;
using Hotel_Backend_API.Models;
using Hotel_Backend_API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_Backend_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AcountsController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly AuthService authService;

        public AcountsController(UserManager<AppUser> userManager,
                 SignInManager<AppUser> signInManager,
                 AuthService authService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.authService = authService;
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
                    Token = await authService.CreateTokenAsync(user, userManager)
                }
            });

        }


        [HttpPut("AssignRole")]
        public async Task<IActionResult> AssignRole(string userId, string roleName)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var result = await userManager.AddToRoleAsync(user, "Normal");
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok("Role assigned successfully");
        }
   
    
    }


}
