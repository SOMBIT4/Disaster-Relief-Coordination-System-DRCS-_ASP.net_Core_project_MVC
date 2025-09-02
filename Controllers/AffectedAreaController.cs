using backend.Models.Entities;
using DRCS.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DRCS.Controllers
{
    [ApiController]
    [Route("api/affected-areas")]
    public class AffectedAreaController : ControllerBase
    {
        private readonly AffectedAreaService _service;

        public AffectedAreaController(AffectedAreaService service)
        {
            _service = service;
        }

        // =====================================
        // PUBLIC: Everyone can access
        // =====================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var areas = await _service.GetAllAsync();
            return Ok(new { success = true, error = false, data = areas });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Show(int id)
        {
            var area = await _service.GetByIDAsync(id);
            if (area == null)
                return NotFound(new { success = false, error = true, message = "Affected area not found" });

            return Ok(new { success = true, error = false, data = area });
        }

        // =====================================
        // RESTRICTED: Only Admin can create/update/delete
        // =====================================
        [HttpPost]
        public async Task<IActionResult> Store([FromBody] AreaRequest request )
        {

            var role = HttpContext.Items["role"]?.ToString();
            if (role != "Admin")
                return StatusCode(403, new { success = false, error = true, message = "Only Admin can create affected area" });
            var area = new AffectedArea
            {
                AreaName = request.AreaName,
                AreaType = request.AreaType,
                SeverityLevel = request.SeverityLevel,
                Population = request.Population
            };
            var created = await _service.CreateAsync(area);
            return StatusCode(201, new { success = true, error = false, data = created });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AreaRequest request)
        {
            var role = HttpContext.Items["role"]?.ToString();
            if (role != "Admin")
                return StatusCode(403, new { success = false, error = true, message = "Only Admin can update affected area" });
            var updatedArea = new AffectedArea
            {
                AreaName = request.AreaName,
                AreaType = request.AreaType,
                SeverityLevel = request.SeverityLevel,
                Population = request.Population
            };
            var success = await _service.UpdateAsync(id, updatedArea);
            if (!success)
                return NotFound(new { success = false, error = true, message = "Affected area not found or update failed" });

            return Ok(new { success = true, error = false, message = "Affected area updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Destroy(int id)
        {
            var role = HttpContext.Items["role"]?.ToString();
            if (role != "Admin")
                return StatusCode(403, new { success = false, error = true, message = "Only Admin can delete affected area" });

            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound(new { success = false, error = true, message = "Affected area not found or delete failed" });

            return Ok(new { success = true, error = false, message = "Affected area deleted successfully" });
        }
    }
     public class AreaRequest
    {
        [Required]
        public string AreaName { get; set; } = string.Empty;
        [Required]
        public string AreaType { get; set; } = string.Empty;
        [Required]
        public string SeverityLevel { get; set; } = string.Empty;
        [Required]
        public int Population { get; set; }
    }
}
