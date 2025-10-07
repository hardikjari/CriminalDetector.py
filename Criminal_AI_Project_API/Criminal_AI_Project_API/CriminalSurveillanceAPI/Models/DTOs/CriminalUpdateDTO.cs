using System;
using System.Collections.Generic;

namespace Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.DTOs
{
    public class CriminalUpdateDTO
    {
        // Guid of the criminal to update
        public Guid Guid { get; set; }

        // Optional fields to update. Null means leave unchanged (except empty string allowed).
        public string? CriminalName { get; set; }
        public string? Crime { get; set; }
        public string? Location { get; set; }
        public DateTime? DateOfCrime { get; set; }

        // Optional new image as base64; if provided will replace existing image
        public string? ImageBase64 { get; set; }

        // Optional list of crimes for full replacement: if provided, existing crimes will be removed and replaced
        public List<CriminalCrimeCreateDTO>? Crimes { get; set; }
    }
}
