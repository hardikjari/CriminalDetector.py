using System.Threading.Tasks;
using Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.Entities;

namespace Criminal_AI_Project_API.CriminalSurveillanceAPI.Services.Interfaces
{
    public interface IAdminService
    {
        Task<AdminModel?> AuthenticateAsync(string email, string password);
    }
}
