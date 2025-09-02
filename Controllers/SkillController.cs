using backend.Models.Entities;
using DRCS.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace DRCS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SkillController : ControllerBase
    {
        private readonly SkillService _service;

        public SkillController(SkillService service)
        {
            _service = service;
        }

        // GET: api/Skill
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                
                var skills = await _service.GetAllAsync();
                return Ok(new { success = true, error = false, data = skills });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = true, message = ex.Message });
            }
        }

        // GET: api/Skill/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
               
                var skill = await _service.GetByIdAsync(id);
                if (skill == null)
                    return NotFound(new { success = false, error = true, message = "Skill not found" });

                return Ok(new { success = true, error = false, data = skill });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = true, message = ex.Message });
            }
        }

        // POST: api/Skill
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SkillDto dto)
        {
            try
            {
                var role = HttpContext.Items["role"]?.ToString();
                if (role != "Admin")
                    return StatusCode(403, new { success = false, error = true, message = "Only admin post skills" });

                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, error = true, message = "Invalid data" });

                // Trim and validate name
                var skillName = dto.SkillName?.Trim();
                if (string.IsNullOrWhiteSpace(skillName))
                    return BadRequest(new { success = false, error = true, message = "Skill name cannot be empty" });

                // Prevent duplicate (case-insensitive)
                var exists = await _service.ExistsByNameAsync(skillName);
                if (exists)
                    return Conflict(new { success = false, error = true, message = $"Skill '{skillName}' already exists" });

                var skill = new Skill
                {
                    SkillName = skillName
                };
                var createdSkill = await _service.CreateAsync(skill);
                return StatusCode(201, new { success = true, error = false, data = createdSkill });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = true, message = ex.Message });
            }
        }

        // PUT: api/Skill/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SkillDto dto)
        {
            try
            {
                var role = HttpContext.Items["role"]?.ToString();
                if (role != "Admin")
                    return StatusCode(403, new { success = false, error = true, message = "Only admin can do it" });

                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, error = true, message = "Invalid data" });

                // Trim and validate name
                var skillName = dto.SkillName?.Trim();
                if (string.IsNullOrWhiteSpace(skillName))
                    return BadRequest(new { success = false, error = true, message = "Skill name cannot be empty" });

                // Prevent duplicate (case-insensitive, excluding current skill)
                var exists = await _service.ExistsByNameAsync(skillName, id);
                if (exists)
                    return Conflict(new { success = false, error = true, message = $"Skill '{skillName}' already exists" });

                var skill = new Skill
                {
                    SkillName = skillName
                };
                var updatedSkill = await _service.UpdateAsync(id, skill);
                if (updatedSkill == null)
                    return NotFound(new { success = false, error = true, message = "Skill not found" });

                return Ok(new { success = true, error = false, data = updatedSkill });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = true, message = ex.Message });
            }
        }

        // DELETE: api/Skill/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var role = HttpContext.Items["role"]?.ToString();
                if (role != "Admin")
                    return StatusCode(403, new { success = false, error = true, message = "Only admin can do it" });
                var deleted = await _service.DeleteAsync(id);
                if (!deleted)
                    return NotFound(new { success = false, error = true, message = "Skill not found" });

                return Ok(new { success = true, error = false, message = "Skill deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = true, message = ex.Message });
            }
        }
    }
    public class SkillDto
    {
        [Required]
        [MinLength(2)]
        public string SkillName { get; set; } = string.Empty;
    }
}
