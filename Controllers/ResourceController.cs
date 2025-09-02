using backend.Database;
using backend.Models.Entities;
using DRCS.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DRCS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResourceController : ControllerBase
    {
        private readonly ResourceService _service;
        private readonly DrcsContext _context;

        public ResourceController(ResourceService service, DrcsContext context)
        {
            _service = service;
            _context = context;
        }

        // GET: api/resource
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var resources = await _service.GetAllResourcesAsync();
                return Ok(new { success = true, error = false, data = resources });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = true, message = ex.Message });
            }
        }

        // GET: api/resource/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Show(int id)
        {
            try
            {
                var resource = await _service.GetResourceByIdAsync(id);
                return Ok(new { success = true, error = false, data = resource });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, error = true, message = ex.Message });
            }
        }

        // POST: api/resource
        [HttpPost]
        public async Task<IActionResult> Store([FromBody] ResourceDto dto)
        {
            var role = HttpContext.Items["role"]?.ToString();
            if (role != "Admin")
                return StatusCode(403, new { success = false, error = true, message = "Only admin can create resources" });

            try
            {
                // Normalize resource type
                var normalizedType = dto.ResourceType.Trim().ToLower();

                // Check duplicate in the same relief center
                var exists = await _context.Resources
                    .AnyAsync(r => r.ResourceType.ToLower() == normalizedType && r.ReliefCenterID == dto.ReliefCenterID);

                if (exists)
                    return BadRequest(new { success = false, message = "Resource with this type already exists for the relief center." });

                var resource = new Resource
                {
                    ResourceType = dto.ResourceType.Trim(),
                    Quantity = dto.Quantity,
                    ExpirationDate = dto.ExpirationDate,
                    ReliefCenterID = dto.ReliefCenterID
                };

                var created = await _service.CreateResourceAsync(resource);
                return StatusCode(201, new { success = true, error = false, data = created });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = true, message = "Resource creation failed: " + ex.Message });
            }
        }

        // PUT: api/resource/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ResourceDto dto)
        {
            var role = HttpContext.Items["role"]?.ToString();
            if (role != "Admin")
                return StatusCode(403, new { success = false, error = true, message = "Only admin can update resources" });

            try
            {
                // Normalize resource type
                var normalizedType = dto.ResourceType.Trim().ToLower();

                // Check duplicate in the same relief center excluding current resource
                var exists = await _context.Resources
                    .AnyAsync(r => r.ResourceType.ToLower() == normalizedType && r.ReliefCenterID == dto.ReliefCenterID && r.ResourceID != id);

                if (exists)
                    return BadRequest(new { success = false, message = "Another resource with this type already exists for the relief center." });

                var resource = new Resource
                {
                    ResourceType = dto.ResourceType.Trim(),
                    Quantity = dto.Quantity,
                    ExpirationDate = dto.ExpirationDate,
                    ReliefCenterID = dto.ReliefCenterID
                };

                var updated = await _service.UpdateResourceAsync(id, resource);
                return Ok(new { success = true, error = false, data = updated });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = true, message = "Resource update failed: " + ex.Message });
            }
        }

        // DELETE: api/resource/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Destroy(int id)
        {
            var role = HttpContext.Items["role"]?.ToString();
            if (role != "Admin")
                return StatusCode(403, new { success = false, error = true, message = "Only admin can delete resources" });

            try
            {
                await _service.DeleteResourceAsync(id);
                return Ok(new { success = true, error = false, message = "Resource deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = true, message = "Failed to delete resource: " + ex.Message });
            }
        }

        // Remaining donation and aid-prep endpoints remain unchanged...
    }

    public class ResourceDto
    {
        [Required]
        public string ResourceType { get; set; } = string.Empty;

        [Required]
        public int Quantity { get; set; }

        public DateTime? ExpirationDate { get; set; }

        [Required]
        public int ReliefCenterID { get; set; }
    }
}
