using backend.Database;
using backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DRCS.Services
{
    public class AuthService
    {
        private readonly DrcsContext _db;
        private readonly IConfiguration _config;
        private readonly ReliefCenterService _reliefCenterService;

        public AuthService(DrcsContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
            _reliefCenterService = new ReliefCenterService(db);
        }

        // -----------------------------
        // Register a normal user
        // -----------------------------
        public async Task<User?> RegisterUserAsync(User user)
        {
            if (await _db.Users.AnyAsync(u => u.Email == user.Email))
                throw new InvalidOperationException("Email already exists");

            user.Password = HashPassword(user.Password);
            if (string.IsNullOrEmpty(user.RoleName))
                user.RoleName = "User";
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        // -----------------------------
        // Register a volunteer
        // -----------------------------
        public async Task<User?> RegisterVolunteerAsync(
    User user,
    int assignedCenter,
    string phoneNo,
    List<int> skillIds)
        {
            // Ensure skills exist in the system
            if (!await _db.Skills.AnyAsync())
                throw new InvalidOperationException("No skills available in the system. Please contact admin.");

            // Check if assigned center exists
            var centerExists = await _db.ReliefCenters.AnyAsync(c => c.CenterID == assignedCenter);
            if (!centerExists)
                throw new InvalidOperationException($"Assigned center with ID {assignedCenter} does not exist");

            // Check if email is already taken
            if (await _db.Users.AnyAsync(u => u.Email == user.Email))
                throw new InvalidOperationException("Email already exists");

            // ✅ Validate all requested skills before saving anything
            var distinctSkillIds = skillIds.Distinct().ToList();
            foreach (var skillId in distinctSkillIds)
            {
                var skillExists = await _db.Skills.AnyAsync(s => s.SkillID == skillId);
                if (!skillExists)
                    throw new InvalidOperationException($"Skill with ID {skillId} does not exist");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // Hash password
                user.Password = HashPassword(user.Password);
                user.RoleName = "Volunteer";
                user.PhoneNo = phoneNo;
                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;

                // Save User
                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                // Create Volunteer
                var volunteer = new Volunteer
                {
                    Name = user.Name,
                    ContactInfo = user.PhoneNo,
                    AssignedCenter = assignedCenter,
                    Status = "Active",
                    UserID = user.UserID,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _db.Volunteers.Add(volunteer);
                await _db.SaveChangesAsync();

                // Add volunteer-skill mappings
                foreach (var skillId in distinctSkillIds)
                {
                    _db.VolunteerSkills.Add(new VolunteerSkill
                    {
                        VolunteerID = volunteer.VolunteerID,
                        SkillID = skillId
                    });
                }

                await _db.SaveChangesAsync();

                // Update relief center volunteer count
                await _reliefCenterService.UpdateVolunteerCountAsync(assignedCenter);

                await transaction.CommitAsync();
                return user;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        // -----------------------------
        // Delete a volunteer completely (Volunteer + User)
        // -----------------------------
        public async Task DeleteVolunteerCompletelyAsync(int volunteerId)
        {
            var volunteer = await _db.Volunteers.FindAsync(volunteerId);
            if (volunteer == null) return;

            int? centerId = volunteer.AssignedCenter;
            int userId = volunteer.UserID;

            // Remove volunteer record
            _db.Volunteers.Remove(volunteer);

            // Remove associated user record
            var user = await _db.Users.FindAsync(userId);
            if (user != null)
                _db.Users.Remove(user);

            await _db.SaveChangesAsync();

            // Update volunteer count
            if (centerId.HasValue)
            {
                await _reliefCenterService.UpdateVolunteerCountAsync(centerId.Value);
            }
        }




        // -----------------------------
        // Update volunteer status
        // -----------------------------
        public async Task UpdateVolunteerStatusAsync(int volunteerId, string newStatus)
        {
            var volunteer = await _db.Volunteers.FindAsync(volunteerId);
            if (volunteer != null)
            {
                volunteer.Status = newStatus;
                volunteer.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                if (volunteer.AssignedCenter.HasValue)
                {
                    await _reliefCenterService.UpdateVolunteerCountAsync(volunteer.AssignedCenter.Value);
                }
            }
        }
       
        // -----------------------------
        // Login with JWT
        // -----------------------------
        public async Task<string?> LoginAsync(string email, string password)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !VerifyPassword(password, user.Password))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["JWT:Secret"]
                ?? throw new InvalidOperationException("JWT:Secret is not configured."));

            var claims = new[]
            {
                new Claim("userId", user.UserID.ToString()),
                new Claim("role", user.RoleName),
                new Claim("type", "access")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(4),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // -----------------------------
        // Logout
        // -----------------------------
        public bool Logout(int userId)
        {
            return userId > 0;
        }

        // -----------------------------
        // Password hashing
        // -----------------------------
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string password, string hashed)
        {
            return HashPassword(password) == hashed;
        }
    }
}
