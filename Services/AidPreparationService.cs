using backend.Database;
using backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DRCS.Services
{
    public class AidPreparationService
    {
        private readonly DrcsContext _context;

        public AidPreparationService(DrcsContext context)
        {
            _context = context;
        }

        /////////////////// Aid Preparation ///////////////////

        public async Task<AidPreparation> CreateAidPreparationAsync(int requestID)
        {
            var existing = await _context.AidPreparations
                .FirstOrDefaultAsync(p => p.RequestID == requestID);

            if (existing != null)
                return existing;

            var prep = new AidPreparation
            {
                RequestID = requestID,
                DepartureTime = DateTime.MinValue,
                EstimatedArrival = DateTime.MinValue,
                Status = "Preparing",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.AidPreparations.Add(prep);
            await _context.SaveChangesAsync();
            return prep;
        }

        public async Task UpdateAidPreparationTimesAsync(int preparationID, DateTime departure, DateTime arrival)
        {
            var prep = await _context.AidPreparations.FindAsync(preparationID);
            if (prep == null)
                throw new Exception("Aid preparation record not found.");

            // ✅ Ensure UTC before saving
            prep.DepartureTime = DateTime.SpecifyKind(departure, DateTimeKind.Utc);
            prep.EstimatedArrival = DateTime.SpecifyKind(arrival, DateTimeKind.Utc);
            prep.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }


        public async Task<List<AidPreparation>> GetFullAidPrepDetailsAsync()
        {
            var aidPreps = await _context.AidPreparations.ToListAsync();

            if (aidPreps.Count == 0)
                throw new Exception("No aid preparation records found.");

            return aidPreps;
        }

        public async Task UpdateAidPrepStatusAsync(int preparationID, string status)
        {
            var prep = await _context.AidPreparations.FindAsync(preparationID);
            if (prep == null)
                throw new Exception("Aid preparation record not found.");

            prep.Status = status;
            prep.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<string> GetAidPrepStatusAsync(int preparationID)
        {
            var prep = await _context.AidPreparations.FindAsync(preparationID);
            if (prep == null)
                throw new Exception("Aid preparation record not found.");

            return prep.Status;
        }

        /////////////////// Volunteers ///////////////////

        public async Task<AidPreparationVolunteer> AddVolunteerAsync(int preparationID, int volunteerID)
        {
            var volunteer = await _context.Volunteers.FindAsync(volunteerID);
            if (volunteer == null)
                throw new Exception("Volunteer does not exist in the system.");

            var existing = await _context.AidPreparationVolunteers
                .FirstOrDefaultAsync(v => v.PreparationID == preparationID && v.VolunteerID == volunteerID);

            if (existing != null)
                return existing;

            var prepVolunteer = new AidPreparationVolunteer
            {
                PreparationID = preparationID,
                VolunteerID = volunteerID,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.AidPreparationVolunteers.Add(prepVolunteer);
            await _context.SaveChangesAsync();

            return prepVolunteer;
        }

        public async Task<List<AidPreparationVolunteer>> GetVolunteersAsync(int preparationID)
        {
            return await _context.AidPreparationVolunteers
                .Where(v => v.PreparationID == preparationID)
                .ToListAsync();
        }

        public async Task UpdateVolunteerAsync(int volunteerRecordID, int volunteerID)
        {
            var record = await _context.AidPreparationVolunteers.FindAsync(volunteerRecordID);
            if (record == null)
                throw new Exception("Volunteer record not found.");

            var volunteerExists = await _context.Volunteers.AnyAsync(v => v.VolunteerID == volunteerID);
            if (!volunteerExists)
                throw new Exception("Volunteer does not exist.");
            record.VolunteerID = volunteerID;
            record.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteVolunteerAsync(int volunteerRecordID)
        {
            var record = await _context.AidPreparationVolunteers.FindAsync(volunteerRecordID);
            if (record == null)
                throw new Exception("Volunteer record not found.");

            _context.AidPreparationVolunteers.Remove(record);
            await _context.SaveChangesAsync();
        }

        /////////////////// Resources ///////////////////

        public async Task<AidPreparationResource> AddResourceUsageAsync(int preparationID, int resourceID, int quantity)
        {
            // Fetch the resource first
            var resource = await _context.Resources.FirstOrDefaultAsync(r => r.ResourceID == resourceID);

            if (resource == null)
                throw new Exception("Resource not found.");

            if (resource.Quantity < quantity)
                throw new Exception("Not enough resource available.");

            // Deduct the quantity
            resource.Quantity -= quantity;
            resource.UpdatedAt = DateTime.UtcNow;

            // Create the AidPreparationResource record
            var usage = new AidPreparationResource
            {
                PreparationID = preparationID,
                ResourceID = resourceID,
                QuantityUsed = quantity,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Add both changes in a transaction
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Resources.Update(resource);
                _context.AidPreparationResources.Add(usage);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return usage;
        }


        public async Task<List<AidPreparationResource>> GetResourcesAsync(int preparationID)
        {
            return await _context.AidPreparationResources
                .Where(r => r.PreparationID == preparationID)
                .ToListAsync();
        }

        public async Task UpdateResourceUsageAsync(int usageID, int resourceID, int quantity)
        {
            var usage = await _context.AidPreparationResources.FindAsync(usageID);
            if (usage == null)
                throw new Exception("Resource usage record not found.");

            usage.ResourceID = resourceID;
            usage.QuantityUsed = quantity;
            usage.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteResourceUsageAsync(int usageID)
        {
            var usage = await _context.AidPreparationResources.FindAsync(usageID);
            if (usage == null)
                throw new Exception("Resource usage record not found.");

            _context.AidPreparationResources.Remove(usage);
            await _context.SaveChangesAsync();
        }
    }
}
