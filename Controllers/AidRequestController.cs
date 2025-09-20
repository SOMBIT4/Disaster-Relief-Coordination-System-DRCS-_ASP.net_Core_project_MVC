using backend.Models.Entities;
using DRCS.Services;
using Microsoft.AspNetCore.Mvc;

namespace DRCS.Controllers
{
    [ApiController]
    [Route("api/aid-requests")]
    public class AidRequestController : ControllerBase
    {
        private readonly AidRequestService _aidRequestService;

        public AidRequestController(AidRequestService aidRequestService)
        {
            _aidRequestService = aidRequestService;
        }

        // GET: aid-requests
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                
                var aidRequests = await _aidRequestService.GetAllAsync();
                return Ok(aidRequests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error fetching aid requests", details = ex.Message });
            }
        }

        // GET: aid-requests/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserRequests(int userId)
        {
            try
            {
                var role = HttpContext.Items["role"]?.ToString();
                if (role != "User")
                    return StatusCode(403, new { success = false, error = true, message = "Only user can do it" });
                var aidRequests = await _aidRequestService.GetByUserAsync(userId);
                return Ok(aidRequests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error fetching aid requests for user", details = ex.Message });
            }
        }

        // POST: aid-requests
        [HttpPost]
        public async Task<IActionResult> Store([FromBody] AidRequestCreateDto dto)
        {
            try
            {
                // Get logged-in user's ID from JWT
                var userIdClaim = User.FindFirst("userId")?.Value;
                if (userIdClaim == null)
                    return Unauthorized(new { error = "User not authenticated" });

                var userId = int.Parse(userIdClaim);

                // Create the aid request using the service
                var created = await _aidRequestService.CreateWithUserAsync(userId, dto);

                return CreatedAtAction(nameof(Show), new { id = created.RequestID }, created);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error creating aid request", details = ex.Message });
            }
        }

        // GET: aid-requests/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Show(int id)
        {
            try
            {
                var role = HttpContext.Items["role"]?.ToString();
                if (role != "Admin")
                    return StatusCode(403, new { success = false, error = true, message = "Only admin can do it" });
                var aidRequest = await _aidRequestService.GetByIdAsync(id);
                if (aidRequest == null)
                    return NotFound(new { error = "Not found" });

                return Ok(aidRequest);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error fetching aid request", details = ex.Message });
            }
        }

        // PUT: aid-requests/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AidRequestUpdateDto dto)
        {
            try
            {
                var role = HttpContext.Items["role"]?.ToString();
                if (role != "Admin")
                    return StatusCode(403, new { success = false, error = true, message = "Only admin can do it" });
                var updatedRequest = new AidRequest
                {
                    AreaID = dto.AreaID,
                    RequestType = dto.RequestType,
                    Description = dto.Description,
                    UrgencyLevel = dto.UrgencyLevel,
                    Status = dto.Status,
                    NumberOfPeople = dto.NumberOfPeople,
                    UpdatedAt = DateTime.UtcNow
                };

                var updated = await _aidRequestService.UpdateAsync(id, updatedRequest);
                if (!updated)
                    return NotFound(new { error = "Not found or update failed" });

                return Ok(new { message = "Aid request updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error updating aid request", details = ex.Message });
            }
        }

        // DELETE: aid-requests/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Destroy(int id)
        {
            try
            {
                var role = HttpContext.Items["role"]?.ToString();
                if (role != "Admin")
                    return StatusCode(403, new { success = false, error = true, message = "Only admin can do it" });
                var deleted = await _aidRequestService.DeleteAsync(id);
                if (!deleted)
                    return NotFound(new { error = "Not found or delete failed" });

                return Ok(new { message = "Aid request deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error deleting aid request", details = ex.Message });
            }
        }

        // PATCH: aid-requests/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusUpdateDto dto)
        {
            

            try
            {
                
                var affected = await _aidRequestService.UpdateStatusAsync(id, dto.Status);
                return Ok(new { message = "Status updated successfully", affected });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // PATCH: aid-requests/{id}/response-time
        [HttpPatch("{id}/response-time")]
        public async Task<IActionResult> UpdateResponseTime(int id, [FromBody] ResponseTimeUpdateDto dto)
        {
            try
            {
                var role = HttpContext.Items["role"]?.ToString();
                if (role != "Admin")
                    return StatusCode(403, new { success = false, error = true, message = "Only admin can do it" });
                var affected = await _aidRequestService.UpdateResponseTimeAsync(id, dto.Date);
                return Ok(new { success = true, affected, message = "Response time updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        // ===============================
        // DTOs
        // ===============================
        public class AidRequestCreateDto
        {
            public int AreaID { get; set; }
            public string RequestType { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string UrgencyLevel { get; set; } = string.Empty;
            public int NumberOfPeople { get; set; }
        }

        public class AidRequestUpdateDto
        {
            public int AreaID { get; set; }
            public string RequestType { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string UrgencyLevel { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public int NumberOfPeople { get; set; }
        }

        public class StatusUpdateDto
        {
            public string Status { get; set; } = string.Empty;
        }

        public class ResponseTimeUpdateDto
        {
            public DateTime Date { get; set; }
        }
    }
}
