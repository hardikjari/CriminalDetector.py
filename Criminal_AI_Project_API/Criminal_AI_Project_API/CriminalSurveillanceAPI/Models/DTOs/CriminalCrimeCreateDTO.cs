using System.ComponentModel.DataAnnotations;

namespace Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.DTOs
{
    public class CriminalCrimeCreateDTO
    {
        [Required]
        public string CrimeType { get; set; } = string.Empty;
        public string? CrimeDescription { get; set; }
    }
}
