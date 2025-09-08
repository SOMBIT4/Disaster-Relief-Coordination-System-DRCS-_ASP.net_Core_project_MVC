using backend.Database;
using backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DRCS.Services
{
    public class DonationService
    {
        private readonly DrcsContext _context;

        public DonationService(DrcsContext context)
        {
            _context = context;
        }

        // Create donation with logged-in user
        public async Task<Donation> CreateWithUserAsync(int userId, string donationType, int quantity, int associatedCenter)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");

            var donation = new Donation
            {
                UserID = userId,
                DonorName = user.Name, // auto-filled
                DonationType = donationType,
                Quantity = quantity,
                DateReceived = DateTime.UtcNow,
                AssociatedCenter = associatedCenter,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Donations.Add(donation);
            await _context.SaveChangesAsync();

            return donation;
        }

        // Get donations by user
        public async Task<List<object>> GetUserDonationsAsync(int userId)
        {
            return await _context.Donations
                .Where(d => d.UserID == userId)
                .Select(d => new
                {
                    d.DonationID,
                    d.DonationType,
                    d.Quantity,
                    d.DateReceived,
                    d.AssociatedCenter,
                    d.CreatedAt,
                    d.UpdatedAt,
                    d.DonorName
                })
                .ToListAsync<object>();
        }

        // Get all donations (admin)
        public async Task<List<object>> GetAllDonationsAsync()
        {
            return await _context.Donations
                .Select(d => new
                {
                    d.DonationID,
                    d.DonationType,
                    d.Quantity,
                    d.DateReceived,
                    d.AssociatedCenter,
                    d.CreatedAt,
                    d.UpdatedAt,
                    d.DonorName,
                    d.UserID
                })
                .ToListAsync<object>();
        }
    }
}
