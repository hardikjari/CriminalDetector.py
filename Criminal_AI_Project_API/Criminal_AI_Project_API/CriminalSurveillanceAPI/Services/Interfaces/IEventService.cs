using Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.DTOs;
using System.Threading.Tasks;

namespace Criminal_AI_Project_API.CriminalSurveillanceAPI.Services.Interfaces
{
    public interface IEventService
    {
        Task<CriminalEventReadDTO> CreateAsync(CriminalEventCreateDTO dto);
    }
}
