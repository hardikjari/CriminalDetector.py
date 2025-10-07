using System;

namespace Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.DTOs
{
    public class CriminalEventReadDTO
    {
        public Guid CriminalGuid { get; set; }
        public DateTime EventAt { get; set; }
        public string? Location { get; set; }
        public string? Details { get; set; }

        // BaseModel
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
