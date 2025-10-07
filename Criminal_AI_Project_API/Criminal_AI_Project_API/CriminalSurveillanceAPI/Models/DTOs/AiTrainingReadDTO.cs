using System;

namespace Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.DTOs
{
    public class AiTrainingReadDTO
    {
        public Guid Guid { get; set; }
        public DateTime TrainedAt { get; set; }
        public int NumberOfImagesTrained { get; set; }
        public string? ModelName { get; set; }
        public string? Notes { get; set; }

        // BaseModel fields
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
