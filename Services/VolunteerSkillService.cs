using backend.Database;
using backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DRCS.Services
{
    public class VolunteerSkillService
    {
        private readonly DrcsContext _context;

        public VolunteerSkillService(DrcsContext context)
        {
            _context = context;
        }

        // Get all skills for a volunteer
        public async Task<IEnumerable<Skill>> GetSkillsForVolunteerAsync(int volunteerId)
        {
            return await (
                from vs in _context.VolunteerSkills
                join s in _context.Skills on vs.SkillID equals s.SkillID
                where vs.VolunteerID == volunteerId
                select s
            ).ToListAsync();
        }

        // Assign a skill to a volunteer
        public async Task<VolunteerSkill> AssignSkillAsync(int volunteerId, int skillId)
        {
            var exists = await _context.VolunteerSkills
                .AnyAsync(vs => vs.VolunteerID == volunteerId && vs.SkillID == skillId);

            if (exists)
                throw new Exception("Skill already assigned to this volunteer");

            var volunteerSkill = new VolunteerSkill
            {
                VolunteerID = volunteerId,
                SkillID = skillId
            };

            _context.VolunteerSkills.Add(volunteerSkill);
            await _context.SaveChangesAsync();

            return volunteerSkill;
        }

        // Remove a skill from a volunteer
        public async Task<bool> RemoveSkillAsync(int volunteerId, int skillId)
        {
            var volunteerSkill = await _context.VolunteerSkills
                .FirstOrDefaultAsync(vs => vs.VolunteerID == volunteerId && vs.SkillID == skillId);

            if (volunteerSkill == null) return false;

            _context.VolunteerSkills.Remove(volunteerSkill);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<int?> GetVolunteerIdByUserIdAsync(int userId)
        {
            var volunteer = await _context.Volunteers.FirstOrDefaultAsync(v => v.UserID == userId);
            return volunteer?.VolunteerID;
        }

    }
}
