using Hotel_Backend_API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Hotel_Backend_API.Services
{
    public class AuthService
    {
        private readonly IConfiguration configuration;

        public AuthService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public async Task<string> CreateTokenAsync(AppUser user, UserManager<AppUser> userManger)
        {
            var authClaims = new List<Claim>
             { 
                 new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                 new Claim(ClaimTypes.GivenName, user.UserName),
                 new Claim(ClaimTypes.Email, user.Email),
                 new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? "")
             };

            var userRoles = await userManger.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("jwt")["secretKey"]));
            var token = new JwtSecurityToken(
                audience: "Hotel Guests",
                issuer: "Hotel-App-Guests",
                claims: authClaims,
                signingCredentials: new SigningCredentials(authKey, SecurityAlgorithms.HmacSha256Signature),
                expires: DateTime.UtcNow.AddDays(1)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
