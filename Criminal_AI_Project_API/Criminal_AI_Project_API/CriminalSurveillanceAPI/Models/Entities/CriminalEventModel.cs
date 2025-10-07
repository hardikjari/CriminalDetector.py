using System;
using System.ComponentModel.DataAnnotations;

namespace Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.Entities
{
    public class CriminalEventModel : BaseModel
    {
        [Key]
        public int Id { get; set; }

        // reference to criminal GUID
        public Guid CriminalGuid { get; set; }

        // When the event occurred
        public DateTime EventAt { get; set; } = DateTime.UtcNow;

        // Location where event occurred
        public string? Location { get; set; }

    }
}
