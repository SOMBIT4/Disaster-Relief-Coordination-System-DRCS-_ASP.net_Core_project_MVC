using backend.Database;
using backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DRCS.Services
{
    public class ResourceService
    {
        private readonly DrcsContext _context;

        public ResourceService(DrcsContext context)
        {
            _context = context;
        }

        // Create a new resource
        public async Task<Resource> CreateResourceAsync(Resource resource)
        {
            resource.CreatedAt = DateTime.UtcNow;
            resource.UpdatedAt = DateTime.UtcNow;

            _context.Resources.Add(resource);
            await _context.SaveChangesAsync();
            return resource;
        }

        // Update an existing resource
        public async Task<Resource> UpdateResourceAsync(int id, Resource updatedData)
        {
            var resource = await _context.Resources.FindAsync(id);
            if (resource == null) throw new KeyNotFoundException("Resource not found.");

            resource.ResourceType = updatedData.ResourceType ?? resource.ResourceType;
            resource.Quantity = updatedData.Quantity;
            resource.ExpirationDate = updatedData.ExpirationDate;
            resource.ReliefCenterID = updatedData.ReliefCenterID;
            resource.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return resource;
        }

        // Delete a resource (cascade delete will remove related AidPreparationResources)
        public async Task DeleteResourceAsync(int id)
        {
            var resource = await _context.Resources.FindAsync(id);
            if (resource == null) throw new KeyNotFoundException("Resource not found.");

            _context.Resources.Remove(resource);
            await _context.SaveChangesAsync();
        }

        // Get a single resource
        public async Task<Resource> GetResourceByIdAsync(int id)
        {
            var resource = await _context.Resources.FindAsync(id);
            if (resource == null) throw new KeyNotFoundException("Resource not found.");
            return resource;
        }

        // Get all resources
        public async Task<List<Resource>> GetAllResourcesAsync()
        {
            return await _context.Resources.ToListAsync();
        }

        // Handle donation (increase existing or create new resource)
        public async Task<Resource> UpdateResourceFromDonationAsync(Donation donation)
        {
            var expirationDate = donation.DateReceived.HasValue
                ? donation.DateReceived.Value.AddMonths(6)
                : DateTime.UtcNow.AddMonths(6);

            var resource = await _context.Resources
                .FirstOrDefaultAsync(r => r.ResourceType == donation.DonationType
                                       && r.ReliefCenterID == donation.AssociatedCenter);

            if (resource != null)
            {
                resource.Quantity += donation.Quantity;
                resource.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                resource = new Resource
                {
                    ResourceType = donation.DonationType,
                    Quantity = donation.Quantity,
                    ExpirationDate = expirationDate,
                    ReliefCenterID = donation.AssociatedCenter,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Resources.Add(resource);
            }

            await _context.SaveChangesAsync();
            return resource;
        }

        // Deduct resource for AidPreparationResource
        public async Task<Resource> DeductResourceForAidPreparationAsync(AidPreparationResource apr)
        {
            var resource = await _context.Resources.FindAsync(apr.ResourceID);
            if (resource == null) throw new KeyNotFoundException("Resource not found.");

            if (resource.Quantity < apr.QuantityUsed)
                throw new InvalidOperationException("Not enough resources available.");

            resource.Quantity -= apr.QuantityUsed;
            resource.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return resource;
        }
    }
}
