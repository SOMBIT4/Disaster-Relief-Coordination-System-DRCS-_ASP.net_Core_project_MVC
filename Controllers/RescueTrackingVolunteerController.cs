using backend.Models.Entities;
using DRCS.Services;
using Microsoft.AspNetCore.Mvc;

namespace DRCS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RescueTrackingVolunteerController : ControllerBase
    {
        private readonly RescueTrackingVolunteerService _service;

        public RescueTrackingVolunteerController(RescueTrackingVolunteerService service)
        {
            _service = service;
        }

        // GET: api/RescueTrackingVolunteer
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var volunteers = await _service.GetAllAsync();
                return Ok(new { success = true, error = false, data = volunteers });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = true, message = ex.Message });
            }
        }

        // POST: api/RescueTrackingVolunteer
        // Only Admin can assign a volunteer to a rescue operation
        [HttpPost]
        public async Task<IActionResult> AssignVolunteer([FromBody] RescueTrackingVolunteerRequest request)
        {
            try
            {
               

                // Create the volunteer assignment
                var volunteerAssignment = await _service.CreateAsync(request.TrackingID, request.VolunteerID);

                return StatusCode(201, new
                {
                    success = true,
                    error = false,
                    data = volunteerAssignment
                });
            }
            catch (Exception ex)
            {
                // Return 400 for validation/business rule errors
                return BadRequest(new
                {
                    success = false,
                    error = true,
                    message = "Failed to assign volunteer: " + ex.Message
                });
            }
        }
    }

    // Request DTO
    public class RescueTrackingVolunteerRequest
    {
        public int TrackingID { get; set; }
        public int VolunteerID { get; set; }
    }
}
