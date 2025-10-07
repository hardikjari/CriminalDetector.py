using System;
using System.Collections.Generic;

namespace Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.DTOs
{
    public class CriminalReadDTO
    {
        public Guid Guid { get; set; }
        public string? CriminalName { get; set; }
        public string? Crime { get; set; }
        public string? Location { get; set; }
        public DateTime DateOfCrime { get; set; }
        public string? ImageUrl { get; set; }

        public List<CriminalCrimeReadDTO>? Crimes { get; set; }
    }
}

