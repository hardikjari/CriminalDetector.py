using Criminal_AI_Project_API.CriminalSurveillanceAPI.Data;
using Criminal_AI_Project_API.CriminalSurveillanceAPI.Services.Implementations;
using Criminal_AI_Project_API.CriminalSurveillanceAPI.Services.Interfaces;
using CriminalSurveillanceAPI.Repositories.Implementations;
using CriminalSurveillanceAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Criminal_AI_Project_API.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection AddProjectDependencies(this IServiceCollection services, IConfiguration configuration)
        {

            // Add CORS policy
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            // 1. Database Context
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Add HttpClient factory for outgoing HTTP calls
            services.AddHttpClient();

            // 2. Generic repository
            services.AddScoped(typeof(IGenericRepository<>),
                typeof(GenericRepository<>));

            // 3. Application services
            services.AddScoped<IAdminService,AdminService>();
            services.AddScoped<ICriminalsService, CriminalsService>();
            services.AddScoped<ITrainingService, TrainingService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IDashboardService, DashboardService>();

            return services;
        }
    }
}
