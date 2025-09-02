using backend.Database;
using backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DRCS.Services
{
    public class SkillService
    {
        private readonly DrcsContext _context;

        public SkillService(DrcsContext context)
        {
            _context = context;
        }

        // Get all skills
        public async Task<IEnumerable<Skill>> GetAllAsync()
        {
            return await _context.Skills.ToListAsync();
        }

        // Get a single skill by ID
        public async Task<Skill?> GetByIdAsync(int id)
        {
            return await _context.Skills.FindAsync(id);
        }

        // Create a new skill
        public async Task<Skill> CreateAsync(Skill skill)
        {
            _context.Skills.Add(skill);
            await _context.SaveChangesAsync();
            return skill;
        }

        // Update a skill
        public async Task<Skill?> UpdateAsync(int id, Skill updatedSkill)
        {
            var skill = await _context.Skills.FindAsync(id);
            if (skill == null) return null;

            skill.SkillName = updatedSkill.SkillName;
            await _context.SaveChangesAsync();

            return skill;
        }

        // Delete a skill
        public async Task<bool> DeleteAsync(int id)
        {
            var skill = await _context.Skills.FindAsync(id);
            if (skill == null) return false;

            _context.Skills.Remove(skill);
            await _context.SaveChangesAsync();
            return true;
        }
        // Check if a skill with the same name exists
        public async Task<bool> ExistsByNameAsync(string skillName, int? excludeId = null)
        {
            var query = _context.Skills.AsQueryable();

            if (excludeId.HasValue)
                query = query.Where(s => s.SkillID != excludeId.Value);

            return await query.AnyAsync(s => s.SkillName.Trim().ToLower() == skillName.Trim().ToLower());

        }
    }
}
