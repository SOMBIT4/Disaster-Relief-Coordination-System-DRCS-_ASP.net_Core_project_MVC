using backend.Database;
using backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DRCS.Services
{
    public class RescueTrackingService
    {
        private readonly DrcsContext _context;

        public RescueTrackingService(DrcsContext context)
        {
            _context = context;
        }

        // Create a new tracking record
        public async Task<RescueTracking> CreateTrackingAsync(RescueTracking request)
        {
            // Validate related AidRequest
            var aidRequest = await _context.AidRequests
                .FirstOrDefaultAsync(a => a.RequestID == request.RequestID);

            if (aidRequest == null)
                throw new Exception("Associated Aid Request not found.");

            if (aidRequest.Status == "Completed")
                throw new Exception("Cannot create rescue tracking for a completed aid request.");

            // Allowed tracking statuses on creation
            var allowedStatuses = new[] { "In Progress", "Pending" };
            if (!allowedStatuses.Contains(request.TrackingStatus))
                throw new Exception("Invalid tracking status. Allowed values: In Progress, Pending");

            // Check for existing tracking
            var existing = await _context.RescueTrackings
                .FirstOrDefaultAsync(r => r.RequestID == request.RequestID);

            if (existing != null) return existing;

            request.CreatedAt = request.CreatedAt == default ? DateTime.UtcNow : request.CreatedAt;
            request.UpdatedAt = request.UpdatedAt == default ? DateTime.UtcNow : request.UpdatedAt;

            _context.RescueTrackings.Add(request);
            await _context.SaveChangesAsync();

            return request;
        }

        // Update tracking
        public async Task<RescueTracking?> UpdateTrackingAsync(int id, string? status = null, int? numberOfPeopleHelped = null, DateTime? completionTime = null)
        {
            var tracking = await _context.RescueTrackings.FindAsync(id);
            if (tracking == null) return null;

            // Validate tracking status
            var allowedStatuses = new[] { "In Progress", "Pending", "Completed" };
            if (!string.IsNullOrEmpty(status) && !allowedStatuses.Contains(status))
                throw new Exception("Invalid tracking status. Allowed values: In Progress, Pending, Completed");

            // If marking completed, ensure CompletionTime is set
            if (status == "Completed" && completionTime == null && tracking.CompletionTime == null)
                throw new Exception("CompletionTime must be set when marking as Completed.");

            if (!string.IsNullOrEmpty(status)) tracking.TrackingStatus = status;
            if (numberOfPeopleHelped.HasValue) tracking.NumberOfPeopleHelped = numberOfPeopleHelped.Value;
            if (completionTime.HasValue) tracking.CompletionTime = completionTime;

            tracking.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return tracking;
        }

        // Get a specific tracking record
        public async Task<RescueTracking?> GetTrackingAsync(int id)
        {
            return await _context.RescueTrackings.FindAsync(id);
        }

        // Get all tracking records
        public async Task<List<RescueTracking>> GetAllTrackingAsync()
        {
            return await _context.RescueTrackings.ToListAsync();
        }

        // Delete a tracking record
        public async Task<bool> DeleteTrackingAsync(int id)
        {
            var tracking = await _context.RescueTrackings.FindAsync(id);
            if (tracking == null) return false;

            _context.RescueTrackings.Remove(tracking);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
