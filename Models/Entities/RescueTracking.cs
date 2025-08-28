using System;

namespace backend.Models.Entities
{
    public class RescueTracking
    {
        public int TrackingID { get; set; }
        public int RequestID { get; set; }
        public string TrackingStatus { get; set; } = string.Empty;
        public DateTime? OperationStartTime { get; set; }
        public int NumberOfPeopleHelped { get; set; }
        public DateTime? CompletionTime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
