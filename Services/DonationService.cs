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

        
        // Create donation with logged-in user (transaction-safe)
        public async Task<Donation> CreateWithUserAsync(int userId, string donationType, int quantity, int associatedCenter)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    throw new InvalidOperationException("User not found");

                var center = await _context.ReliefCenters
                    .Include(rc => rc.Resources)
                    .FirstOrDefaultAsync(rc => rc.CenterID == associatedCenter);

                if (center == null)
                    throw new InvalidOperationException("Relief center not found");

                // Create the donation entry
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

                // Update resources table
                var resource = center.Resources.FirstOrDefault(r => r.ResourceType == donationType);
                if (resource != null)
                {
                    // Resource exists → update quantity
                    resource.Quantity += quantity;
                    resource.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Create new resource entry
                    var newResource = new Resource
                    {
                        ReliefCenterID = center.CenterID,
                        ResourceType = donationType,
                        Quantity = quantity,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.Resources.Add(newResource);
                }

                await _context.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();

                return donation;
            }
            catch
            {
                // Rollback if anything fails
                await transaction.RollbackAsync();
                throw;
            }
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
