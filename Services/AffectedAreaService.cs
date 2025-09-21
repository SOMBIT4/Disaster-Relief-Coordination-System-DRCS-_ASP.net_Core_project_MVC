using backend.Database;
using backend.Models.Entities;

using Microsoft.EntityFrameworkCore;

namespace DRCS.Services
{
    public class AffectedAreaService
    {
        private readonly DrcsContext _context;

        public AffectedAreaService(DrcsContext context)
        {
            _context = context;
        }

        // Get all affected areas
        public async Task<List<AffectedArea>> GetAllAsync()
        {
            return await _context.AffectedAreas.ToListAsync();
        }

        // Get by ID
        public async Task<AffectedArea?> GetByIDAsync(int id)
        {
            return await _context.AffectedAreas.FindAsync(id);
        }

        // Create new affected area
        public async Task<AffectedArea> CreateAsync(AffectedArea area)
        {
            await _context.AffectedAreas.AddAsync(area);
            await _context.SaveChangesAsync();
            return area;
        }

        // Update an affected area
        public async Task<bool> UpdateAsync(int id, AffectedArea updatedArea)
        {
            var area = await _context.AffectedAreas.FindAsync(id);
            if (area == null) return false;

            area.AreaName = updatedArea.AreaName;
            area.AreaType = updatedArea.AreaType;
            area.SeverityLevel = updatedArea.SeverityLevel;
            area.Population = updatedArea.Population;

            await _context.SaveChangesAsync();
            return true;
        }

        // Delete an affected area
        public async Task<bool> DeleteAsync(int id)
        {
            var area = await _context.AffectedAreas.FindAsync(id);
            if (area == null) return false;

            _context.AffectedAreas.Remove(area);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
