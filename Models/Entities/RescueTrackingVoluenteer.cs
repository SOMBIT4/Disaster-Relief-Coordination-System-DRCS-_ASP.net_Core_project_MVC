namespace backend.Models.Entities
{
    public class RescueTrackingVolunteer
    {
        public int ID { get; set; }
        public int TrackingID { get; set; }
        public int VolunteerID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
