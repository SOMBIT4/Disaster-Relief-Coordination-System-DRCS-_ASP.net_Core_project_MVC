using backend.Models.Entities;
using DRCS.Services;
using Microsoft.AspNetCore.Mvc;

namespace DRCS.Controllers
{
    [ApiController]
    [Route("api/volunteers/{volunteerId}/skills")]
    public class VolunteerSkillController : ControllerBase
    {
        private readonly VolunteerSkillService _service;

        public VolunteerSkillController(VolunteerSkillService service)
        {
            _service = service;
        }

        // GET: api/volunteers/{volunteerId}/skills
        [HttpGet]
        public async Task<IActionResult> GetSkillsForVolunteer(int volunteerId)
        {
            try
            {
                var role = HttpContext.Items["role"]?.ToString();
                var currentUserId = HttpContext.Items["userId"] as int?;

                // Map UserID → VolunteerID for the logged-in user
                int? currentVolunteerId = await _service.GetVolunteerIdByUserIdAsync(currentUserId.Value);

                if (role != "Admin" && currentVolunteerId != volunteerId)
                    return StatusCode(403, new { success = false, error = true, message = "Access denied" });

                var skills = await _service.GetSkillsForVolunteerAsync(volunteerId);
                return Ok(new { success = true, error = false, data = skills });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = true, message = ex.Message });
            }
        }

        // POST: api/volunteers/{volunteerId}/skills/{skillId}
        [HttpPost("{skillId}")]
        public async Task<IActionResult> AssignSkill(int volunteerId, int skillId)
        {
            try
            {
                var role = HttpContext.Items["role"]?.ToString();
                var currentUserId = HttpContext.Items["userId"] as int?;
                int? currentVolunteerId = await _service.GetVolunteerIdByUserIdAsync(currentUserId.Value);

                if (role != "Admin" && currentVolunteerId != volunteerId)
                    return StatusCode(403, new { success = false, error = true, message = "Access denied" });

                var assignment = await _service.AssignSkillAsync(volunteerId, skillId);
                return StatusCode(201, new { success = true, error = false, data = assignment });
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { success = false, error = true, message = ex.Message });
            }
        }

        // DELETE: api/volunteers/{volunteerId}/skills/{skillId}
        [HttpDelete("{skillId}")]
        public async Task<IActionResult> RemoveSkill(int volunteerId, int skillId)
        {
            try
            {
                var role = HttpContext.Items["role"]?.ToString();
                var currentUserId = HttpContext.Items["userId"] as int?;
                int? currentVolunteerId = await _service.GetVolunteerIdByUserIdAsync(currentUserId.Value);

                if (role != "Admin" && currentVolunteerId != volunteerId)
                    return StatusCode(403, new { success = false, error = true, message = "Access denied" });

                var removed = await _service.RemoveSkillAsync(volunteerId, skillId);
                if (!removed)
                    return NotFound(new { success = false, error = true, message = "Skill not found for this volunteer" });

                return Ok(new { success = true, error = false, message = "Skill removed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = true, message = ex.Message });
            }
        }
    }
}
