using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SauGYM.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SauGYM.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Modellerimizi veritabanı tablolarına dönüştürüyoruz
        public DbSet<Service> Services { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        // İlişkileri ve başlangıç verilerini yapılandırmak için (Opsiyonel ama önerilir)
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Örnek: Bir hizmetin fiyatı hassas veri olduğu için SQL ayarı
            builder.Entity<Service>()
                .Property(s => s.Price)
                .HasColumnType("decimal(18,2)");
        }
    }
}