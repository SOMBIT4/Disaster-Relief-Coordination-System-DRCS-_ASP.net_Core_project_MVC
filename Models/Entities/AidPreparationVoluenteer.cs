namespace backend.Models.Entities
{
    public class AidPreparationVolunteer
    {
        public int ID { get; set; }
        public int PreparationID { get; set; }
        public int VolunteerID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
