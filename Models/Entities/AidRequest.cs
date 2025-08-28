using System;

namespace backend.Models.Entities
{
    public class AidRequest
    {
        public int RequestID { get; set; }
        public int UserID { get; set; }
        public int AreaID { get; set; }
        public string RequesterName { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UrgencyLevel { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int NumberOfPeople { get; set; }
        public DateTime? RequestDate { get; set; }
        public TimeSpan? ResponseTime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
