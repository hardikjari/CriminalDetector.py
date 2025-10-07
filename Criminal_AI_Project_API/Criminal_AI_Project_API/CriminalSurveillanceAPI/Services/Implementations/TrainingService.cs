using Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.DTOs;
using Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.Entities;
using Criminal_AI_Project_API.CriminalSurveillanceAPI.Services.Interfaces;
using CriminalSurveillanceAPI.Repositories.Interfaces;
using System;
using System.Threading.Tasks;

namespace Criminal_AI_Project_API.CriminalSurveillanceAPI.Services.Implementations
{
    public class TrainingService : ITrainingService
    {
        private readonly IGenericRepository<AiTrainingModel> _repo;

        public TrainingService(IGenericRepository<AiTrainingModel> repo)
        {
            _repo = repo;
        }

        public async Task<AiTrainingReadDTO> CreateAsync(AiTrainingCreateDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var model = new AiTrainingModel
            {
                Guid = Guid.NewGuid(),
                TrainedAt = dto.TrainedAt ?? DateTime.UtcNow,
                NumberOfImagesTrained = dto.NumberOfImagesTrained,
            };

            await _repo.AddAsync(model);
            await _repo.SaveChangesAsync();

            return new AiTrainingReadDTO
            {
                Guid = model.Guid,
                TrainedAt = model.TrainedAt,
                NumberOfImagesTrained = model.NumberOfImagesTrained,
                CreatedBy = model.CreatedBy,
                CreatedAt = model.CreatedAt
            };
        }
    }
}
