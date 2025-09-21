using System;

namespace backend.Models.Entities
{
    public class Donation
    {
        public int DonationID { get; set; }
        public string DonorName { get; set; } = string.Empty;
        public string DonationType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime? DateReceived { get; set; }
        public int AssociatedCenter { get; set; }
        public int UserID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ReliefCenter ReliefCenter { get; set; } = null!;
    }
}
