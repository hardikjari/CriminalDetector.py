namespace Criminal_AI_Project_API.CriminalSurveillanceAPI.Models
{
    public class BaseModel
    {
        public string? CreatedBy { get; set; } = "Admin";
        public string? UpdatdBy { get; set; } = "Admin";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
