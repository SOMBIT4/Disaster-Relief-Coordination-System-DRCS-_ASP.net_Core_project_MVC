namespace backend.Models.Entities
{
    public class ReliefCenter
    {
        public int CenterID { get; set; }
        public string CenterName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int NumberOfVolunteersWorking { get; set; }
        public int MaxVolunteersCapacity { get; set; }
        public int ManagerID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
