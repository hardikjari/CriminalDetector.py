using System;

namespace Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.DTOs
{
    // Minimal Sieve-like model to accept filter/sort/pagination/search from query string
    public class SieveModel
    {
        // Example: "CriminalName@=*john" or "Crime==Robbery"
        public string? Filters { get; set; }

        // Example: "-DateOfCrime,CriminalName"
        public string? Sorts { get; set; }

        // Page number (1-based)
        public int Page { get; set; } = 1;

        // Page size
        public int PageSize { get; set; } = 10;

        // Optional generic search term to apply to common fields
        public string? Search { get; set; }
    }
}
