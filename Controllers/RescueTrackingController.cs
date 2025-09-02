using backend.Models.Entities;
using DRCS.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DRCS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RescueTrackingController : ControllerBase
    {
        private readonly RescueTrackingService _service;

        public RescueTrackingController(RescueTrackingService service)
        {
            _service = service;
        }

        // GET /api/rescue-tracking
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var data = await _service.GetAllTrackingAsync();
                return Ok(new { success = true, error = false, data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = true, message = ex.Message });
            }
        }

        // GET /api/rescue-tracking/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Show(int id)
        {
            try
            {
                var tracking = await _service.GetTrackingAsync(id);
                if (tracking == null)
                    return NotFound(new { success = false, error = true, message = "Rescue tracking record not found." });

                return Ok(new { success = true, error = false, data = tracking });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = true, message = ex.Message });
            }
        }

        // POST /api/rescue-tracking
        [HttpPost]
        public async Task<IActionResult> Store([FromBody] RescueTrackingDto dto)
        {
            try
            {
                // Admin check
                var role = HttpContext.Items["role"]?.ToString();
                if (role != "Admin")
                    return StatusCode(403, new { success = false, error = true, message = "Only admin can create rescue tracking records." });

                var tracking = new RescueTracking
                {
                    RequestID = dto.RequestID,
                    TrackingStatus = dto.TrackingStatus,
                    OperationStartTime = dto.OperationStartTime,
                    NumberOfPeopleHelped = dto.NumberOfPeopleHelped,
                    CompletionTime = dto.CompletionTime
                };

                var created = await _service.CreateTrackingAsync(tracking);
                return StatusCode(201, new { success = true, error = false, data = created });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = true, message = ex.Message });
            }
        }

        // PUT /api/rescue-tracking/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] RescueTrackingDto dto)
        {
            try
            {
                var tracking = await _service.UpdateTrackingAsync(
                    id,
                    dto.TrackingStatus,
                    dto.NumberOfPeopleHelped,
                    dto.CompletionTime
                );

                if (tracking == null)
                    return NotFound(new { success = false, error = true, message = "Rescue tracking record not found." });

                return Ok(new { success = true, error = false, data = tracking });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = true, message = ex.Message });
            }
        }

        // DELETE /api/rescue-tracking/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _service.DeleteTrackingAsync(id);
                if (!deleted)
                    return NotFound(new { success = false, error = true, message = "Rescue tracking record not found." });

                return Ok(new { success = true, error = false, message = "Rescue tracking record deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = true, message = ex.Message });
            }
        }
    }

    public class RescueTrackingDto
    {
        [Required]
        public int RequestID { get; set; }

        [Required]
        public string TrackingStatus { get; set; } = string.Empty;

        [Required]
        public DateTime? OperationStartTime { get; set; }

        public int NumberOfPeopleHelped { get; set; } = 0;

        public DateTime? CompletionTime { get; set; }
    }
}
