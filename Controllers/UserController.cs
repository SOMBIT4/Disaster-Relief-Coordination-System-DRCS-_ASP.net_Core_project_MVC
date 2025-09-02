using backend.Database;
using backend.Models.Entities;
using DRCS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace backend.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly AuthService _authService;
        private readonly DrcsContext _db;

        public UserController(UserService userService, AuthService authService, DrcsContext db)
        {
            _userService = userService;
            _authService = authService;
            _db = db;
        }

        // ------------------ GET ALL USERS ------------------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var role = HttpContext.Items["role"]?.ToString();
                if (role != "Admin")
                    return StatusCode(403, new { success = false, error = true, message = "Only admin can view all users" });

                var users = await _userService.GetAllUsers();
                return Ok(new { success = true, error = false, data = users });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = true, message = ex.Message });
            }
        }

        // ------------------ GET SINGLE USER ------------------
        [HttpGet("currentUser")]
        public async Task<IActionResult> Show([FromQuery] int userId)
        {
            if (userId <= 0)
                return BadRequest(new { success = false, error = true, message = "User ID not provided" });

            try
            {
                var role = HttpContext.Items["role"]?.ToString();
                var currentUserId = Convert.ToInt32(HttpContext.Items["userId"]); // set by auth middleware

                if (role != "Admin" && userId != currentUserId)
                    return StatusCode(403, new { success = false, error = true, message = "Access denied" });

                var user = await _userService.GetUserWithVolunteerInfo(userId);
                return Ok(new { success = true, error = false, data = user });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, error = true, message = ex.Message });
            }
        }

        // ------------------ GET ALL VOLUNTEERS ------------------
        [HttpGet("volunteers")]
        public async Task<IActionResult> ShowAllVolunteers()
        {
            try
            {
                var role = HttpContext.Items["role"]?.ToString();
                if (role != "Admin")
                    return StatusCode(403, new { success = false, error = true, message = "Only admin can view all volunteers" });

                var volunteers = await _userService.GetAllVolunteers();
                return Ok(new { success = true, error = false, data = volunteers });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = true, message = ex.Message });
            }
        }

        // ------------------ CREATE USER (Admin only) ------------------
        [HttpPost]
        public async Task<IActionResult> Store([FromBody] CreateUserDto newUserDto)
        {
            try
            {
                var role = HttpContext.Items["role"]?.ToString();
                if (role != "Admin")
                    return StatusCode(403, new { success = false, error = true, message = "Only admin can create users" });

                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, error = true, message = "Invalid data", details = ModelState });

                // Normalize role (default to "User" if null/empty/whitespace)
                var roleName = string.IsNullOrWhiteSpace(newUserDto.RoleName) ? "User" : newUserDto.RoleName;

                // Duplicate email check (fast-fail with Conflict)
                var emailExists = await _db.Users.AnyAsync(u => u.Email == newUserDto.Email);
                if (emailExists)
                    return Conflict(new { success = false, error = true, message = "Email already exists" });

                // Build user entity (password will be hashed inside AuthService)
                var user = new User
                {
                    Email = newUserDto.Email,
                    Name = newUserDto.Name,
                    Password = newUserDto.Password,
                    RoleName = roleName, 
                    PhoneNo = newUserDto.PhoneNo
                };

                User? created;

                if (roleName.Equals("Volunteer", StringComparison.OrdinalIgnoreCase))
                {
                    // Require center + at least one skill
                    if (!newUserDto.AssignedCenterId.HasValue)
                        return BadRequest(new { success = false, error = true, message = "AssignedCenterId is required for volunteers." });

                    if (newUserDto.SkillIds == null || !newUserDto.SkillIds.Any())
                        return BadRequest(new { success = false, error = true, message = "At least one SkillId is required for volunteers." });

                    // Use the exact same flow as public registration (hashing, validations, tx, volunteer-skill mapping)
                    created = await _authService.RegisterVolunteerAsync(
                        user,
                        newUserDto.AssignedCenterId.Value,
                        user.PhoneNo,
                        newUserDto.SkillIds
                    );
                }
                else
                {
                    // Default to plain user using the same hashing logic as Auth registration
                    created = await _authService.RegisterUserAsync(user);
                }

                return StatusCode(201, new { success = true, error = false, data = created });
            }
            catch (InvalidOperationException ex)
            {
                // for known validation errors surfaced by AuthService (e.g., center/skills missing)
                return BadRequest(new { success = false, error = true, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = true, message = $"User creation failed: {ex.Message}" });
            }
        }

        // ------------------ UPDATE USER ------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto updatedUserDto)
        {
            try
            {
                var role = HttpContext.Items["role"]?.ToString();
                var currentUserId = Convert.ToInt32(HttpContext.Items["userId"]);

                if (role != "Admin" && id != currentUserId)
                    return StatusCode(403, new { success = false, error = true, message = "Access denied" });

                var updatedUser = new User
                {
                    Email = updatedUserDto.Email,
                    Name = updatedUserDto.Name,
                    RoleName = updatedUserDto.RoleName,
                    PhoneNo = updatedUserDto.PhoneNo,
                    UpdatedAt = DateTime.UtcNow
                };

                var user = await _userService.UpdateUser(id, updatedUser, updatedUserDto.AssignedCenterId);
                return Ok(new { success = true, error = false, data = user });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { success = false, error = true, message = "User not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = true, message = $"User update failed: {ex.Message}" });
            }
        }

        // ------------------ DELETE USER ------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> Destroy(int id)
        {
            try
            {
                var role = HttpContext.Items["role"]?.ToString();
                if (role != "Admin")
                    return StatusCode(403, new { success = false, error = true, message = "Only admin can delete users" });

                await _userService.DeleteUser(id);
                return Ok(new { success = true, error = false, message = "User deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, error = true, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = true, message = $"User deletion failed: {ex.Message}" });
            }
        }
    }

    // ------------------ DTO ------------------
    public class CreateUserDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(2)]
        public string Name { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty; 
        public string RoleName { get; set; } = "User";
        [Required, Phone]
        public string PhoneNo { get; set; } = string.Empty;
        //for volunteer
        public int? AssignedCenterId { get; set; }
        public List<int> SkillIds { get; set; } = new List<int>();
    }
    public class UpdateUserDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required, MinLength(2)]
        public string Name { get; set; } = string.Empty;
        public string RoleName { get; set; } = "User";
        [Required, Phone]
        public string PhoneNo { get; set; } = string.Empty;
        // Only used if role == "Volunteer"
        public int? AssignedCenterId { get; set; }
        public List<int> SkillIds { get; set; } = new List<int>();
    }
}
