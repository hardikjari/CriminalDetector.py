using System;

namespace Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.DTOs
{
    public class CriminalCrimeReadDTO
    {
        public Guid Guid { get; set; }
        public int CriminalId { get; set; }
        public string? CrimeType { get; set; }
        public string? CrimeDescription { get; set; }
    }
}
