using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebProjesi.Models;

namespace WebProjesi.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Service> Services { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<TrainerAvailability> TrainerAvailabilities { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        public DbSet<TrainerService> TrainerServices { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<TrainerService>()
                .HasKey(x => new { x.TrainerId, x.ServiceId });

            builder.Entity<TrainerService>()
                .HasOne(x => x.Trainer)
                .WithMany(t => t.TrainerServices)
                .HasForeignKey(x => x.TrainerId);

            builder.Entity<TrainerService>()
                .HasOne(x => x.Service)
                .WithMany(s => s.TrainerServices)
                .HasForeignKey(x => x.ServiceId);
        }
    }
}
