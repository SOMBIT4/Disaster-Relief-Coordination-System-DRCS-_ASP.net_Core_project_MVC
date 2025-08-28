using System;

namespace backend.Models.Entities
{
    public class Resource
    {
        public int ResourceID { get; set; }
        public string ResourceType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int ReliefCenterID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
