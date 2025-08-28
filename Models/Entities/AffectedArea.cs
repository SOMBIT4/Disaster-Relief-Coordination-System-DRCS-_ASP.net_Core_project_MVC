using System;

namespace backend.Models.Entities
{
    public class AffectedArea
    {
        public int AreaID { get; set; }   
        public string AreaName { get; set; } = string.Empty;
        public string AreaType { get; set; } = string.Empty;
        public string SeverityLevel { get; set; } = string.Empty;
        public int Population { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
