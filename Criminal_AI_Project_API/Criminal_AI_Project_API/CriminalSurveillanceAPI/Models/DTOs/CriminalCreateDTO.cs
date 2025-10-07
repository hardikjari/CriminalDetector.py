using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.DTOs
{
    public class CriminalCreateDTO
    {
        [Required]
        public string CriminalName { get; set; } = string.Empty;
        public string? Crime { get; set; }
        public string? Location { get; set; }
        public DateTime DateOfCrime { get; set; }
    // Base64-encoded image data (data URI or raw base64). Will be saved to disk by the API.
    public string? ImageBase64 { get; set; }

        // Optional list of crimes details to attach
        public List<CriminalCrimeCreateDTO>? Crimes { get; set; }
    }
}
