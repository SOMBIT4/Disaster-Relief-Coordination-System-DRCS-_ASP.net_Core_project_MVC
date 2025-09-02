using backend.Models.Entities;
using backend.Database;
using Microsoft.EntityFrameworkCore;

namespace DRCS.Services
{
    public class RescueTrackingVolunteerService
    {
        private readonly DrcsContext _context;

        public RescueTrackingVolunteerService(DrcsContext context)
        {
            _context = context;
        }

        // Get all RescueTrackingVolunteer records
        public async Task<List<RescueTrackingVolunteer>> GetAllAsync()
        {
            return await _context.RescueTrackingVolunteers.ToListAsync();
        }

        // Create a new RescueTrackingVolunteer record
        public async Task<RescueTrackingVolunteer> CreateAsync(int trackingId, int volunteerId)
        {
            // Fetch the tracking record
            var tracking = await _context.RescueTrackings.FindAsync(trackingId);
            if (tracking == null)
                throw new Exception("Rescue tracking not found.");

            // Only allow assignment if tracking is not completed
            if (tracking.TrackingStatus == "Completed")
                throw new Exception("Cannot assign volunteers to a completed rescue operation.");

            // Check if the volunteer exists
            var volunteerExists = await _context.Volunteers.AnyAsync(v => v.VolunteerID == volunteerId);
            if (!volunteerExists)
                throw new Exception("Volunteer not found.");

            // Prevent duplicate assignment
            var alreadyAssigned = await _context.RescueTrackingVolunteers
                .AnyAsync(v => v.TrackingID == trackingId && v.VolunteerID == volunteerId);

            if (alreadyAssigned)
                throw new Exception("Volunteer is already assigned to this rescue tracking.");

            var newRecord = new RescueTrackingVolunteer
            {
                TrackingID = trackingId,
                VolunteerID = volunteerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.RescueTrackingVolunteers.Add(newRecord);
            await _context.SaveChangesAsync();

            return newRecord;
        }
    }
}
