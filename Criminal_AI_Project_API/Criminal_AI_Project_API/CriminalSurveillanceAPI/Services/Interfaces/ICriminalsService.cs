using Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Criminal_AI_Project_API.CriminalSurveillanceAPI.Services.Interfaces
{
    public interface ICriminalsService
    {
        Task<CriminalReadDTO> CreateAsync(CriminalCreateDTO dto);
        Task<PagedResult<CriminalReadDTO>> GetAllAsync(Models.DTOs.SieveModel sieve);
        Task<CriminalReadDTO?> GetByGuidAsync(Guid guid);
        Task<CriminalReadDTO?> UpdateAsync(CriminalUpdateDTO dto);
        Task<bool> DeleteByGuidAsync(Guid guid);

    }
}
