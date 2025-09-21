using backend.Database;
using backend.Models.Entities;
using DRCS.Controllers;
using Microsoft.EntityFrameworkCore;

namespace DRCS.Services
{
    public class AidRequestService
    {
        private readonly DrcsContext _context;

        public AidRequestService(DrcsContext context)
        {
            _context = context;
        }

        public async Task<List<AidRequest>> GetAllAsync()
        {
            return await _context.AidRequests.ToListAsync();
        }

        public async Task<List<AidRequest>> GetByUserAsync(int userId)
        {
            return await _context.AidRequests
                .Where(r => r.UserID == userId)
                .ToListAsync();
        }

        public async Task<AidRequest?> GetByIdAsync(int id)
        {
            return await _context.AidRequests
                .FirstOrDefaultAsync(r => r.RequestID == id);
        }


        // -----------------------------
        //  create using logged-in user
        // -----------------------------
        public async Task<AidRequest> CreateWithUserAsync(int userId, AidRequestController.AidRequestCreateDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            // Check if user already has a pending request for the same area
            var existingRequest = await _context.AidRequests
                .Where(r => r.UserID == userId && r.AreaID == dto.AreaID && r.Status == "Pending")
                .FirstOrDefaultAsync();

            if (existingRequest != null)
                throw new InvalidOperationException("You already have a pending aid request for this area.");

            var aidRequest = new AidRequest
            {
                UserID = userId,
                RequesterName = user.Name,
                ContactInfo = user.PhoneNo,
                AreaID = dto.AreaID,
                RequestType = dto.RequestType,
                Description = dto.Description,
                UrgencyLevel = dto.UrgencyLevel,
                NumberOfPeople = dto.NumberOfPeople,
                Status = "Pending",
                RequestDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.AidRequests.Add(aidRequest);
            await _context.SaveChangesAsync();
            return aidRequest;
        }

        public async Task<bool> UpdateAsync(int id, AidRequest request)
        {
            var existing = await _context.AidRequests.FindAsync(id);
            if (existing == null) return false;

            _context.Entry(existing).CurrentValues.SetValues(request);
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _context.AidRequests.FindAsync(id);
            if (existing == null) return false;

            _context.AidRequests.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> UpdateStatusAsync(int id, string status)
        {
            var aidRequest = await _context.AidRequests.FindAsync(id);
            if (aidRequest == null) return 0;

            aidRequest.Status = status;
            aidRequest.UpdatedAt = DateTime.UtcNow;
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateResponseTimeAsync(int id, DateTime responseTime)
        {
            var aidRequest = await _context.AidRequests.FindAsync(id);
            if (aidRequest == null) return 0;

            aidRequest.ResponseTime = responseTime.TimeOfDay;
            aidRequest.UpdatedAt = DateTime.UtcNow;
            return await _context.SaveChangesAsync();
        }
    }
}
