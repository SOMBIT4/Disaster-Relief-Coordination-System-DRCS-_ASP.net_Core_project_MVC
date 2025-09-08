using backend.Models.Entities;
using DRCS.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DRCS.Controllers
{
    [ApiController]
    [Route("api/donations")]
    public class DonationController : ControllerBase
    {
        private readonly DonationService _donationService;

        public DonationController(DonationService donationService)
        {
            _donationService = donationService;
        }

        // POST: /donations
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDonationDto donationDto)
        {
            try
            {
                if (donationDto == null)
                    return BadRequest(new { success = false, error = true, message = "Invalid request data" });

                var role = HttpContext.Items["role"]?.ToString();
                if (role == "Admin" || role == "Volunteer")
                    return StatusCode(403, new { success = false, error = true, message = "Only regular users can create donations" });

                if (!HttpContext.Items.ContainsKey("userId"))
                    return Unauthorized(new { success = false, error = true, message = "User ID not provided" });

                int userId = (int)HttpContext.Items["userId"]!;

                var createdDonation = await _donationService.CreateWithUserAsync(
                    userId,
                    donationDto.DonationType,
                    donationDto.Quantity,
                    donationDto.AssociatedCenter
                );

                return StatusCode(StatusCodes.Status201Created, new
                {
                    success = true,
                    error = false,
                    data = createdDonation
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    error = true,
                    message = "Donation creation failed: " + ex.Message
                });
            }
        }

        // GET: /donations/history
        [HttpGet("history")]
        public async Task<IActionResult> UserDonations()
        {
            try
            {
                if (!HttpContext.Items.ContainsKey("userId"))
                    return Unauthorized(new { success = false, error = true, message = "User ID not provided" });

                int userId = (int)HttpContext.Items["userId"]!;
                var donations = await _donationService.GetUserDonationsAsync(userId);

                return Ok(new { success = true, error = false, data = donations });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = true, message = "Error retrieving donation history: " + ex.Message });
            }
        }

        // GET: /donations/all (Admin only)
        [HttpGet("all")]
        public async Task<IActionResult> AllDonations()
        {
            try
            {
                var role = HttpContext.Items["role"]?.ToString();
                if (role != "Admin")
                    return StatusCode(403, new { success = false, error = true, message = "Only regular users can create donations" });

                var donations = await _donationService.GetAllDonationsAsync();
                return Ok(new { success = true, error = false, data = donations });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    error = true,
                    message = "Error retrieving donations: " + ex.Message
                });
            }
        }
    }

    // DTO for creating donation
    public class CreateDonationDto
    {
        [Required]
        public string DonationType { get; set; } = string.Empty;

        [Required]
        public int Quantity { get; set; }

        

        [Required]
        public int AssociatedCenter { get; set; }
    }
}
