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
            // Check if role is Volunteer and required fields are provided
            if (request.RoleName.Equals("Volunteer", StringComparison.OrdinalIgnoreCase))
            {
                if (!assignedCenter.HasValue)
                {
                    return BadRequest(new
                    {
                        success = false,
                        error = true,
                        message = "For role 'Volunteer', you must provide an assigned center."
                    });
                }

                if (request.SkillIds == null || !request.SkillIds.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        error = true,
                        message = "For role 'Volunteer', you must provide at least one skill."
                    });
                }
            }

            User user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Password = request.Password,
                PhoneNo = request.PhoneNo,
                RoleName = request.RoleName
            };

            User? createdUser;

            if (user.RoleName == "Volunteer" && assignedCenter.HasValue && request.SkillIds.Any())
            {
                // Pass skill ids to service
                createdUser = await _authService.RegisterVolunteerAsync(
                    user,
                    assignedCenter.Value,
                    user.PhoneNo,
                    request.SkillIds
                );
            }
            else
            {
                createdUser = await _authService.RegisterUserAsync(user);
            }

            if (createdUser == null)
                return BadRequest(new { success = false, message = "Registration failed" });

            var tokens = GenerateTokens(createdUser);

            return Created("", new
            {
                success = true,
                error = false,
                message = "Registration successful",
                user_info = createdUser,
                access_token = tokens.accessToken,
                refresh_token = tokens.refreshToken
            });
        }

        // LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest login)
        {
            if (string.IsNullOrEmpty(login.Email) || string.IsNullOrEmpty(login.Password))
                return BadRequest(new { success = false, message = "Email and Password are required" });

            // Call AuthService.LoginAsync → returns JWT string
            var jwtToken = await _authService.LoginAsync(login.Email, login.Password);
            if (jwtToken == null)
                return Unauthorized(new { success = false, error = true, message = "Invalid email or password" });

            // Fetch user info for response
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == login.Email);
            if (user == null)
                return Unauthorized(new { success = false, error = true, message = "User not found" });

            // Generate refresh token
            var refreshToken = GenerateRefreshToken(user);

            return Ok(new
            {
                success = true,
                error = false,
                message = "Login successful",
                user_info = user,
                access_token = jwtToken,
                refresh_token = refreshToken
            });
        }

        // LOGOUT
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            if (HttpContext.Items["userId"] is int userId && _authService.Logout(userId))
                return Ok(new { success = true, error = false, message = "Logged out successfully" });

            return StatusCode(500, new { success = false, error = true, message = "Failed to log out" });
        }

        // CURRENT USER INFO
        [HttpGet("me")]
        public IActionResult Me()
        {
            if (HttpContext.Items["userId"] is not int userId)
                return Unauthorized();

            var role = HttpContext.Items["role"]?.ToString() ?? "User";

            return Ok(new { userId, role });
        }

        // REFRESH TOKEN
        [HttpGet("token/refresh")]
        public IActionResult RefreshToken([FromHeader(Name = "X-Refresh-Token")] string? refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { message = "Refresh token not provided" });

            var principal = ValidateToken(refreshToken, "refresh");
            if (principal == null) return Unauthorized(new { message = "Invalid refresh token" });

            var userIdClaim = principal.FindFirst("userId")?.Value;
            var roleClaim = principal.FindFirst("role")?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { message = "Invalid refresh token claims" });

            var secret = _config["JWT:Secret"] ?? throw new InvalidOperationException("JWT:Secret is not configured.");
            var key = Encoding.ASCII.GetBytes(secret);
            var tokenHandler = new JwtSecurityTokenHandler();

            var accessToken = tokenHandler.WriteToken(new JwtSecurityToken(
                claims: new[] {
                    new Claim("userId", userId.ToString()),
                    new Claim("role", roleClaim ?? "User"),
                    new Claim("type", "access")
                },
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            ));

            return Ok(new { access_token = accessToken });
        }

        // =======================
        // HELPER METHODS
        // =======================
        private (string accessToken, string refreshToken) GenerateTokens(User user)
        {
            var secret = _config["JWT:Secret"] ?? throw new InvalidOperationException("JWT:Secret is not configured.");
            var key = Encoding.ASCII.GetBytes(secret);
            var tokenHandler = new JwtSecurityTokenHandler();

            var accessToken = tokenHandler.WriteToken(new JwtSecurityToken(
                claims: new[] {
                    new Claim("userId", user.UserID.ToString()),
                    new Claim("role", user.RoleName),
                    new Claim("type", "access")
                },
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            ));

            var refreshToken = tokenHandler.WriteToken(new JwtSecurityToken(
                claims: new[] {
                    new Claim("userId", user.UserID.ToString()),
                    new Claim("role", user.RoleName),
                    new Claim("type", "refresh")
                },
                expires: DateTime.UtcNow.AddHours(5),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            ));

            return (accessToken, refreshToken);
        }

        private string GenerateRefreshToken(User user)
        {
            var secret = _config["JWT:Secret"] ?? throw new InvalidOperationException("JWT:Secret is not configured.");
            var key = Encoding.ASCII.GetBytes(secret);
            var tokenHandler = new JwtSecurityTokenHandler();

            var refreshToken = tokenHandler.WriteToken(new JwtSecurityToken(
                claims: new[] {
                    new Claim("userId", user.UserID.ToString()),
                    new Claim("role", user.RoleName),
                    new Claim("type", "refresh")
                },
                expires: DateTime.UtcNow.AddHours(5),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            ));

            return refreshToken;
        }

        private ClaimsPrincipal? ValidateToken(string token, string expectedType)
        {
            var secret = _config["JWT:Secret"] ?? throw new InvalidOperationException("JWT:Secret is not configured.");
            var key = Encoding.ASCII.GetBytes(secret);
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(token, new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = validatedToken as JwtSecurityToken;
                var typeClaim = jwtToken?.Claims.FirstOrDefault(c => c.Type == "type")?.Value;
                if (typeClaim != expectedType) return null;

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
    //DTO
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
    public class RegisterRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; } = string.Empty;
        [Required]
        [Phone]
        public string PhoneNo { get; set; } = string.Empty;
        
        public string RoleName { get; set; } = "User";
        public List<int> SkillIds { get; set; } = new List<int>();
    }
}
