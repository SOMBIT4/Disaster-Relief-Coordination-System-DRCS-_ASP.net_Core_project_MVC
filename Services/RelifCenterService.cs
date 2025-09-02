using backend.Database;
using backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DRCS.Services
{
    public class ReliefCenterService
    {
        private readonly DrcsContext _context;

        public ReliefCenterService(DrcsContext context)
        {
            _context = context;
        }

        // -----------------------------
        // Helper: Update volunteer count
        // -----------------------------
        public async Task UpdateVolunteerCountAsync(int centerId)
        {
            var center = await _context.ReliefCenters.FirstOrDefaultAsync(c => c.CenterID == centerId);
            if (center != null)
            {
                // Count active volunteers assigned to this center
                center.NumberOfVolunteersWorking = await _context.Volunteers
                    .CountAsync(v => v.AssignedCenter == centerId && v.Status == "Active");

                center.UpdatedAt = DateTime.UtcNow;
                _context.ReliefCenters.Update(center);
                await _context.SaveChangesAsync();
            }
        }

        // -----------------------------
        // Create a new relief center
        // -----------------------------
        public async Task<ReliefCenter> CreateReliefCenterAsync(ReliefCenter center)
        {
            if (await _context.ReliefCenters
                .AnyAsync(c => c.CenterName.ToLower() == center.CenterName.ToLower()))
            {
                throw new InvalidOperationException($"A relief center with the name '{center.CenterName}' already exists.");
            }

            center.NumberOfVolunteersWorking = 0; // always start at 0
            center.CreatedAt = DateTime.UtcNow;
            center.UpdatedAt = DateTime.UtcNow;

            _context.ReliefCenters.Add(center);
            await _context.SaveChangesAsync();
            return center;
        }

        // -----------------------------
        // Update an existing relief center
        // (do not touch volunteer count here)
        // -----------------------------
        public async Task<ReliefCenter> UpdateReliefCenterAsync(int id, ReliefCenter updatedCenter)
        {
            var existing = await _context.ReliefCenters.FirstOrDefaultAsync(c => c.CenterID == id);
            if (existing == null) throw new KeyNotFoundException("Relief center not found");

            // Check for duplicate name
            bool nameExists = await _context.ReliefCenters
                .AnyAsync(c => c.CenterName.ToLower() == updatedCenter.CenterName.ToLower() && c.CenterID != id);

            if (nameExists)
                throw new InvalidOperationException($"A relief center with the name '{updatedCenter.CenterName}' already exists.");

            existing.CenterName = updatedCenter.CenterName ?? existing.CenterName;
            existing.Location = updatedCenter.Location ?? existing.Location;
            existing.MaxVolunteersCapacity = updatedCenter.MaxVolunteersCapacity;
            existing.ManagerID = updatedCenter.ManagerID;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.ReliefCenters.Update(existing);
            await _context.SaveChangesAsync();

            return existing;
        }
        // -----------------------------
        // Delete a relief center
        // -----------------------------
        public async Task<bool> DeleteReliefCenterAsync(int centerId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            var center = await _context.ReliefCenters.FirstOrDefaultAsync(c => c.CenterID == centerId);
            if (center == null) throw new KeyNotFoundException("Relief center not found");

            // Find all volunteers assigned to this center
            var volunteers = await _context.Volunteers
                .Where(v => v.AssignedCenter == centerId)
                .ToListAsync();

            foreach (var volunteer in volunteers)
            {
                var user = await _context.Users.FindAsync(volunteer.UserID);
                if (user != null && user.RoleName == "Volunteer")
                {
                    user.RoleName = "User"; // downgrade role
                    user.UpdatedAt = DateTime.UtcNow;
                    _context.Users.Update(user);
                }
            }

            // Delete the center (volunteers will be cascade deleted)
            _context.ReliefCenters.Remove(center);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return true;
        }


        // -----------------------------
        // Get a relief center by ID
        // -----------------------------
        public async Task<ReliefCenter?> GetReliefCenterByIdAsync(int id)
        {
            var center = await _context.ReliefCenters.FirstOrDefaultAsync(c => c.CenterID == id);
            if (center == null) throw new KeyNotFoundException("Relief center not found");
            return center;
        }

        // -----------------------------
        // Get all relief centers
        // -----------------------------
        public async Task<List<ReliefCenter>> GetAllReliefCentersAsync()
        {
            return await _context.ReliefCenters.ToListAsync();
        }
    }
}
