namespace backend.Models.Entities
{
    public class AidPreparationResource
    {
        public int ID { get; set; }
        public int PreparationID { get; set; }
        public int ResourceID { get; set; }
        public int QuantityUsed { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
