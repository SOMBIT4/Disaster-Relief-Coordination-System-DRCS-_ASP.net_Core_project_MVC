using DRCS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace DRCS.Controllers
{
    [ApiController]
    [Route("api/volunteers/{volunteerId}")]
    public class VolunteerTaskController : ControllerBase
    {
        private readonly VolunteerTaskService _volunteerTaskService;

        public VolunteerTaskController(VolunteerTaskService volunteerTaskService)
        {
            _volunteerTaskService = volunteerTaskService;
        }

        /// <summary>
        /// Get aid preparation tasks for a given volunteer
        /// Only Admin or the specific volunteer can access
        /// </summary>
        [HttpGet("aid-prep-tasks")]
        public async Task<IActionResult> GetAidPrepTasks(int volunteerId)
        {
            try
            {
                var role = HttpContext.Items["role"]?.ToString();
                var userId = HttpContext.Items["userId"]?.ToString(); // middleware sets this

                if (role != "Volunteer" && userId != volunteerId.ToString())
                {
                    return StatusCode(403, new
                    {
                        success = false,
                        error = true,
                        message = "Forbidden: Only admin or the volunteer can access these tasks"
                    });
                }

                var tasks = await _volunteerTaskService.GetAidPreparationTasksAsync(volunteerId);
                return Ok(new { success = true, error = false, data = tasks });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = true, message = ex.Message });
            }
        }

        /// <summary>
        /// Get rescue tracking tasks for a given volunteer
        /// Only Admin or the specific volunteer can access
        /// </summary>
        [HttpGet("rescue-tracking-tasks")]
        public async Task<IActionResult> GetRescueTrackingTasks(int volunteerId)
        {
            try
            {
                var role = HttpContext.Items["role"]?.ToString();
                var userId = HttpContext.Items["userId"]?.ToString();

                if (role != "Volunteer" && userId != volunteerId.ToString())
                {
                    return StatusCode(403, new
                    {
                        success = false,
                        error = true,
                        message = "Forbidden: Only admin or the volunteer can access these tasks"
                    });
                }

                var tasks = await _volunteerTaskService.GetRescueTrackingTasksAsync(volunteerId);
                return Ok(new { success = true, error = false, data = tasks });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = true, message = ex.Message });
            }
        }
    }
}
