using System.ComponentModel.DataAnnotations;

namespace Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.Entities
{
    public class CriminalCrimesModel : BaseModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public Guid Guid { get; set; } = Guid.NewGuid();
        public int CriminalId { get; set; }
        public string? CrimeType { get; set; }
        public string? CrimeDescription { get; set; }
    }
}
