namespace backend.Models.Entities
{
    public class Volunteer
    {
        public int VolunteerID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
        public int? AssignedCenter { get; set; }
        public string Status { get; set; } = string.Empty;
        public int UserID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
