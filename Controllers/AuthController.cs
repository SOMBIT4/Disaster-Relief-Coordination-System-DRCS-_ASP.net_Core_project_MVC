using backend.Database;
using backend.Models.Entities;
using DRCS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DRCS.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly DrcsContext _db;
        private readonly IConfiguration _config;

        public AuthController(AuthService authService, DrcsContext db, IConfiguration config)
        {
            _authService = authService;
            _db = db;
            _config = config;
        }

        // REGISTER
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, [FromQuery] int? assignedCenter = null)
        {
            if (request.RoleName.Equals("Volunteer", StringComparison.OrdinalIgnoreCase))
            {
                if (!assignedCenter.HasValue)
                    return BadRequest(new { success = false, error = true, message = "Assigned center required" });

                if (request.SkillIds == null || !request.SkillIds.Any())
                    return BadRequest(new { success = false, error = true, message = "At least one skill required" });
            }

            User user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Password = request.Password,
                PhoneNo = request.PhoneNo,
                RoleName = request.RoleName
            };

            User? createdUser = request.RoleName == "Volunteer" && assignedCenter.HasValue && request.SkillIds.Any()
                ? await _authService.RegisterVolunteerAsync(user, assignedCenter.Value, user.PhoneNo, request.SkillIds)
                : await _authService.RegisterUserAsync(user);

            if (createdUser == null)
                return BadRequest(new { success = false, message = "Registration failed" });

            var token = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken(user);

            // Set cookies
            SetCookie("access_token", token, 4);
            SetCookie("refresh_token", refreshToken, 5);

            return Created("", new
            {
                success = true,
                error = false,
                message = "Registration successful",
                user_info = createdUser
            });
        }

        // LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest login)
        {
            if (string.IsNullOrEmpty(login.Email) || string.IsNullOrEmpty(login.Password))
                return BadRequest(new { success = false, message = "Email and Password required" });

            var jwtToken = await _authService.LoginAsync(login.Email, login.Password);
            if (jwtToken == null)
                return Unauthorized(new { success = false, error = true, message = "Invalid email or password" });

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == login.Email);
            if (user == null)
                return Unauthorized(new { success = false, error = true, message = "User not found" });

            var refreshToken = GenerateRefreshToken(user);

            // Set cookies
            SetCookie("access_token", jwtToken, 4);
            SetCookie("refresh_token", refreshToken, 5);

            return Ok(new
            {
                success = true,
                error = false,
                message = "Login successful",
                user_info = user
            });
        }

        // LOGOUT
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");
            return Ok(new { success = true, error = false, message = "Logged out successfully" });
        }

        // HELPER: Create tokens
        private string GenerateAccessToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(_config["JWT:Secret"] ?? throw new InvalidOperationException("JWT:Secret missing"));
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Role, user.RoleName),
                new Claim("type", "access")
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            );

            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(_config["JWT:Secret"] ?? throw new InvalidOperationException("JWT:Secret missing"));
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Role, user.RoleName),
                new Claim("type", "refresh")
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(5),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            );

            return tokenHandler.WriteToken(token);
        }

        private void SetCookie(string name, string value, int hours)
        {
            Response.Cookies.Append(name, value, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // false for localhost
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(hours)
            });
        }
    }

    // DTOs
    public class LoginRequest
    {
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        [Required] public string Name { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required, MinLength(6)] public string Password { get; set; } = string.Empty;
        [Required, Phone] public string PhoneNo { get; set; } = string.Empty;
        public string RoleName { get; set; } = "User";
        public List<int> SkillIds { get; set; } = new();
    }
}
