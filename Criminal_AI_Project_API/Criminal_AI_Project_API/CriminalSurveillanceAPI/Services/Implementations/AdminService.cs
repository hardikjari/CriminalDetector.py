using Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.Entities;
using Criminal_AI_Project_API.CriminalSurveillanceAPI.Services.Interfaces;
using CriminalSurveillanceAPI.Repositories.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace Criminal_AI_Project_API.CriminalSurveillanceAPI.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IGenericRepository<AdminModel> _adminRepository;

        public AdminService(IGenericRepository<AdminModel> adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public async Task<AdminModel?> AuthenticateAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return null;

            var admins = await _adminRepository.FindAsync(a => a.email == email);
            var admin = admins.FirstOrDefault();
            if (admin == null) return null;

            // Plain-text password check for now - replace with hashing later.
            if (admin.password != password) return null;

            return admin;
        }
    }
}
