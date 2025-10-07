using System;
using System.ComponentModel.DataAnnotations;

namespace Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.DTOs
{
    public class CriminalEventCreateDTO
    {
        [Required]
        public Guid CriminalGuid { get; set; }

        // When the event occurred; optional - default to now
        public DateTime? EventAt { get; set; }
        public string? Location { get; set; }

    }
}
