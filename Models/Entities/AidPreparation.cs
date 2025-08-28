using System;

namespace backend.Models.Entities
{
    public class AidPreparation
    {
        public int PreparationID { get; set; }   
        public int RequestID { get; set; }
        public DateTime? DepartureTime { get; set; }
        public DateTime? EstimatedArrival { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
