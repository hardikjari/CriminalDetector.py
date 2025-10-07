using Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.DTOs;
using Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.Entities;
using Criminal_AI_Project_API.CriminalSurveillanceAPI.Services.Interfaces;
using CriminalSurveillanceAPI.Repositories.Interfaces;
using System;
using System.Threading.Tasks;

namespace Criminal_AI_Project_API.CriminalSurveillanceAPI.Services.Implementations
{
    public class EventService : IEventService
    {
        private readonly IGenericRepository<CriminalEventModel> _repo;

        public EventService(IGenericRepository<CriminalEventModel> repo)
        {
            _repo = repo;
        }

        public async Task<CriminalEventReadDTO> CreateAsync(CriminalEventCreateDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            var model = new CriminalEventModel
            {
                CriminalGuid = dto.CriminalGuid,
                EventAt = dto.EventAt ?? DateTime.UtcNow,
                Location = dto.Location,
            };

            await _repo.AddAsync(model);
            await _repo.SaveChangesAsync();

            return new CriminalEventReadDTO
            {
                CriminalGuid = model.CriminalGuid,
                EventAt = model.EventAt,
                Location = model.Location,
                CreatedBy = model.CreatedBy,
                CreatedAt = model.CreatedAt
            };
        }
    }
}
