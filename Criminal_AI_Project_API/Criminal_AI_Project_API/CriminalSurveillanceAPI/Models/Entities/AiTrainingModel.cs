using System;
using System.ComponentModel.DataAnnotations;

namespace Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.Entities
{
    public class AiTrainingModel : BaseModel
    {
        [Key]
        public int Id { get; set; }

        public Guid Guid { get; set; } = Guid.NewGuid();

        // When the training occurred
        public DateTime TrainedAt { get; set; } = DateTime.UtcNow;

        // Number of images trained by the AI in this session
        public int NumberOfImagesTrained { get; set; }

    }   
}
