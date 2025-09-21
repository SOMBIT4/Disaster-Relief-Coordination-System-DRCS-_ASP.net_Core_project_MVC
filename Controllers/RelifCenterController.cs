using backend.Models.Entities;
using DRCS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DRCS.Controllers
{
    [ApiController]
    [Route("api/relief-centers")]
    public class ReliefCenterController : ControllerBase
    {
        private readonly ReliefCenterService _reliefCenterService;

        public ReliefCenterController(ReliefCenterService reliefCenterService)
        {
            _reliefCenterService = reliefCenterService;
        }

        // GET: api/relief-centers
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var centers = await _reliefCenterService.GetAllReliefCentersAsync();
                return Ok(new { success = true, error = false, data = centers });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = true, message = ex.Message });
            }
        }

        // GET: api/relief-centers/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Show(int id)
        {
            try
            {
                var center = await _reliefCenterService.GetReliefCenterByIdAsync(id);
                return Ok(new { success = true, error = false, data = center });
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(new { success = false, error = true, message = e.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = true, message = ex.Message });
            }
        }

        // POST: api/relief-centers
        [HttpPost]
        [Authorize] // Require authentication
        public async Task<IActionResult> Store([FromBody] ReliefCenterDto dto)
        {
            try
            {
                var role = HttpContext.Items["role"]?.ToString();
                if (role != "Admin")
                    return Forbid("Only Admins can create relief centers.");

                var center = new ReliefCenter
                {
                    CenterName = dto.CenterName,
                    Location = dto.Location,
                    MaxVolunteersCapacity = dto.MaxVolunteersCapacity,
                    ManagerID = dto.ManagerID
                };
                var created = await _reliefCenterService.CreateReliefCenterAsync(center);
                return StatusCode(201, new { success = true, error = false, data = created });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = true, message = ex.Message });
            }
        }

        // PUT: api/relief-centers/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] ReliefCenterDto dto)
        {
            try
            {
                var role = HttpContext.Items["role"]?.ToString();
                if (role != "Admin")
                    return Forbid("Only Admins can update relief centers.");
                var center = new ReliefCenter
                {
                    CenterName = dto.CenterName,
                    Location = dto.Location,
                    MaxVolunteersCapacity = dto.MaxVolunteersCapacity,
                    ManagerID = dto.ManagerID
                };
                var updated = await _reliefCenterService.UpdateReliefCenterAsync(id, center);
                return Ok(new { success = true, error = false, data = updated });
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(new { success = false, error = true, message = e.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = true, message = ex.Message });
            }
        }

        // DELETE: api/relief-centers/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Destroy(int id)
        {
            try
            {
                var role = HttpContext.Items["role"]?.ToString();
                if (role != "Admin")
                    return Forbid("Only Admins can delete relief centers.");

                await _reliefCenterService.DeleteReliefCenterAsync(id);
                return Ok(new { success = true, error = false, message = "Relief center deleted" });
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(new { success = false, error = true, message = e.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = true, message = ex.Message });
            }
        }
    }
    public class ReliefCenterDto
    {
        [Required]
        public string CenterName { get; set; } = string.Empty;

        [Required]
        public string Location { get; set; } = string.Empty;

        [Required]
        public int MaxVolunteersCapacity { get; set; }

        [Required]
        public int ManagerID { get; set; }
    }
}
