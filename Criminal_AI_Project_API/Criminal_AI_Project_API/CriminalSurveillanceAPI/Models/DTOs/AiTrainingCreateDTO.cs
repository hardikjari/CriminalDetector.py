using System;
using System.ComponentModel.DataAnnotations;

namespace Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.DTOs
{
    public class AiTrainingCreateDTO
    {
        // When the training occurred. Optional, defaults to now
        public DateTime? TrainedAt { get; set; }

        // Number of images trained
        [Required]
        public int NumberOfImagesTrained { get; set; }

    }
}
