using backend.Models.Entities;
using DRCS.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AidPreparationController : ControllerBase
    {
        private readonly AidPreparationService _aidPrepService;

        public AidPreparationController(AidPreparationService aidPrepService)
        {
            _aidPrepService = aidPrepService;
        }

        #region DTOs
        public class CreateAidPrepRequest
        {
            public int RequestID { get; set; }
        }

        public class UpdateTimesRequest
        {
            public DateTime DepartureTime { get; set; }
            public DateTime EstimatedArrival { get; set; }
        }

        public class AddVolunteerRequest
        {
            public int VolunteerID { get; set; }
        }

        public class UpdateStatusRequest
        {
            public string Status { get; set; } = string.Empty;
        }

        public class AddResourceRequest
        {
            public int ResourceID { get; set; }
            public int QuantityUsed { get; set; }
        }
        #endregion

        // -------------------- Aid Preparation --------------------

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAidPrepRequest request)
        {
            if (request.RequestID <= 0)
                return BadRequest(new { success = false, message = "RequestID is required and must be greater than 0" });

            try
            {
                var aidPrep = await _aidPrepService.CreateAidPreparationAsync(request.RequestID);
                return Created("", new { success = true, data = aidPrep });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPatch("{preparationId}/times")]
        public async Task<IActionResult> UpdateTimes(int preparationId, [FromBody] UpdateTimesRequest request)
        {
            try
            {
                await _aidPrepService.UpdateAidPreparationTimesAsync(preparationId, request.DepartureTime, request.EstimatedArrival);
                return Ok(new { success = true, message = "Times updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    inner = ex.InnerException?.Message,
                    stack = ex.InnerException?.StackTrace
                });
            }

        }

        [HttpGet("full-details")]
        public async Task<IActionResult> GetAllAidPrepDetails()
        {
            try
            {
                var details = await _aidPrepService.GetFullAidPrepDetailsAsync();
                return Ok(new { success = true, data = details });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPatch("{preparationId}/status")]
        public async Task<IActionResult> UpdateStatus(int preparationId, [FromBody] UpdateStatusRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Status))
                return BadRequest(new { success = false, message = "Status is required" });

            try
            {
                await _aidPrepService.UpdateAidPrepStatusAsync(preparationId, request.Status);
                return Ok(new { success = true, message = "Status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{preparationId}/status")]
        public async Task<IActionResult> GetAidPrepStatus(int preparationId)
        {
            try
            {
                var status = await _aidPrepService.GetAidPrepStatusAsync(preparationId);
                return Ok(new { success = true, data = status });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // -------------------- Volunteers --------------------

        [HttpPost("{preparationId}/volunteers")]
        public async Task<IActionResult> AddVolunteer(int preparationId, [FromBody] AddVolunteerRequest request)
        {
            if (request.VolunteerID <= 0)
                return BadRequest(new { success = false, message = "VolunteerID is required and must be greater than 0" });

            try
            {
                var volunteer = await _aidPrepService.AddVolunteerAsync(preparationId, request.VolunteerID);
                return Created("", new { success = true, data = volunteer });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{preparationId}/volunteers")]
        public async Task<IActionResult> GetVolunteers(int preparationId)
        {
            try
            {
                var volunteers = await _aidPrepService.GetVolunteersAsync(preparationId);
                return Ok(new { success = true, data = volunteers });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPatch("volunteers/{volunteerRecordId}")]
        public async Task<IActionResult> UpdateVolunteer(int volunteerRecordId, [FromBody] AddVolunteerRequest request)
        {
            try
            {
                await _aidPrepService.UpdateVolunteerAsync(volunteerRecordId, request.VolunteerID);
                return Ok(new { success = true, message = "Volunteer updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("volunteers/{volunteerRecordId}")]
        public async Task<IActionResult> DeleteVolunteer(int volunteerRecordId)
        {
            try
            {
                await _aidPrepService.DeleteVolunteerAsync(volunteerRecordId);
                return Ok(new { success = true, message = "Volunteer deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // -------------------- Resources --------------------

        [HttpPost("{preparationId}/resources")]
        public async Task<IActionResult> AddResource(int preparationId, [FromBody] AddResourceRequest request)
        {
            if (request.ResourceID <= 0 || request.QuantityUsed <= 0)
                return BadRequest(new { success = false, message = "ResourceID and QuantityUsed must be greater than 0" });

            try
            {
                var usage = await _aidPrepService.AddResourceUsageAsync(preparationId, request.ResourceID, request.QuantityUsed);
                return Created("", new { success = true, data = usage });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{preparationId}/resources")]
        public async Task<IActionResult> GetResources(int preparationId)
        {
            try
            {
                var resources = await _aidPrepService.GetResourcesAsync(preparationId);
                return Ok(new { success = true, data = resources });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPatch("resources/{usageID}")]
        public async Task<IActionResult> UpdateResource(int usageID, [FromBody] AddResourceRequest request)
        {
            if (request.ResourceID <= 0 || request.QuantityUsed <= 0)
                return BadRequest(new { success = false, message = "ResourceID and QuantityUsed must be greater than 0" });

            try
            {
                await _aidPrepService.UpdateResourceUsageAsync(usageID, request.ResourceID, request.QuantityUsed);
                return Ok(new { success = true, message = "Resource usage updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("resources/{usageID}")]
        public async Task<IActionResult> DeleteResource(int usageID)
        {
            try
            {
                await _aidPrepService.DeleteResourceUsageAsync(usageID);
                return Ok(new { success = true, message = "Resource usage deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
