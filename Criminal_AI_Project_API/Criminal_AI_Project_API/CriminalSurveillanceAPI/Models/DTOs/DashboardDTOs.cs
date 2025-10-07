using System;
using System.Collections.Generic;

namespace Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.DTOs
{
    public class TopCriminalDTO
    {
        public Guid Guid { get; set; }
        public string? CriminalName { get; set; }
        public int CrimeCount { get; set; }
    }

    public class DashboardDTO
    {
        public int TotalCriminals { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalDataTrained { get; set; }
        public List<TopCriminalDTO> TopCriminals { get; set; } = new List<TopCriminalDTO>();
        // Add other aggregated fields as needed
    }
}
