using System.ComponentModel.DataAnnotations;

namespace Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.Entities
{
    public class CriminalModel : BaseModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public Guid Guid { get; set; } = Guid.NewGuid();
        public string? CriminalName { get; set; }
        public string? Crime { get; set; }
        public string? Location { get; set; }
        public DateTime DateOfCrime { get; set; }
        public string? ImageUrl { get; set; }
    }
}
