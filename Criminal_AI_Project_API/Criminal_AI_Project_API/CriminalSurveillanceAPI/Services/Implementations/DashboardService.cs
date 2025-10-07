using Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.DTOs;
using Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.Entities;
using Criminal_AI_Project_API.CriminalSurveillanceAPI.Services.Interfaces;
using CriminalSurveillanceAPI.Repositories.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Criminal_AI_Project_API.CriminalSurveillanceAPI.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IGenericRepository<CriminalModel> _criminalRepo;
        private readonly IGenericRepository<AdminModel> _adminRepo;
        private readonly IGenericRepository<AiTrainingModel> _trainingRepo;
        private readonly IGenericRepository<CriminalCrimesModel> _crimesRepo;

        public DashboardService(IGenericRepository<CriminalModel> criminalRepo,
            IGenericRepository<AdminModel> adminRepo,
            IGenericRepository<AiTrainingModel> trainingRepo,
            IGenericRepository<CriminalCrimesModel> crimesRepo)
        {
            _criminalRepo = criminalRepo;
            _adminRepo = adminRepo;
            _trainingRepo = trainingRepo;
            _crimesRepo = crimesRepo;
        }

        public async Task<DashboardDTO> GetDashboardAsync()
        {
            var totalCriminals = (await _criminalRepo.GetAllAsync()).Count();
            var totalAdmins = (await _adminRepo.GetAllAsync()).Count();
            var totalDataTrained = (await _trainingRepo.GetAllAsync()).Sum(t => t.NumberOfImagesTrained);

            var crimes = await _crimesRepo.GetAllAsync();
            var top = crimes.GroupBy(c => c.CriminalId)
                .Select(g => new { CriminalId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            var criminals = await _criminalRepo.GetAllAsync();

            var topList = new List<TopCriminalDTO>();
            foreach (var t in top)
            {
                var crim = criminals.FirstOrDefault(c => c.Id == t.CriminalId);
                if (crim != null)
                {
                    topList.Add(new TopCriminalDTO
                    {
                        Guid = crim.Guid,
                        CriminalName = crim.CriminalName,
                        CrimeCount = t.Count
                    });
                }
            }

            return new DashboardDTO
            {
                TotalCriminals = totalCriminals,
                TotalAdmins = totalAdmins,
                TotalDataTrained = totalDataTrained,
                TopCriminals = topList
            };
        }
    }
}
