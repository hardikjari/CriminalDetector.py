using Criminal_AI_Project_API.CriminalSurveillanceAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Criminal_AI_Project_API.CriminalSurveillanceAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        // Define DbSets for your entities here
        public DbSet<AdminModel> tbl_Admin { get; set; }
        public DbSet<CriminalModel> tbl_Criminals { get; set; }
        public DbSet<CriminalCrimesModel> tbl_CriminalCrimes { get; set; }
        public DbSet<AiTrainingModel> tbl_AiTrainings { get; set; }
        public DbSet<CriminalEventModel> tbl_CriminalEvents { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    modelBuilder.Entity<AdminModel>().HasData
        //        (
        //            new AdminModel
        //            {
        //                admin_id = 1,
        //                CreatedBy = "Admin",
        //                CreatedAt = DateTime.UtcNow,
        //                email = "admin@gmail.com",
        //                Guid = Guid.NewGuid(),
        //                IsDeleted = false,
        //                password  = "admin@123",
        //                UpdatdBy = "Admin",
        //                UpdatedAt = DateTime.UtcNow,
        //                username = "Admin"
        //            }
        //        );
        //}
    }
}
