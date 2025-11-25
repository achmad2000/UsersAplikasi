using Pengguna.Models;
using Microsoft.EntityFrameworkCore;
using Pengguna.Models;

namespace Pengguna.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Technician> Technicians { get; set; }
        public DbSet<UserModel> Users { get; set; }
        public DbSet<OrderModel> Orders { get; set; }
        public DbSet<AdminNet> AdminNets { get; set; }
        public DbSet<ActiveOrder> ActiveOrders { get; set; }
        public DbSet<WaitingResponOrder> WaitingResponOrders { get; set; }
        public DbSet<ServiceLog> ServiceLogs { get; set; }
        public DbSet<ServiceLogDetail> ServiceLogDetails { get; set; }
        public DbSet<ServiceItem> ServiceItems { get; set; }
        public DbSet<SelesaiService> SelesaiServices { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Konfigurasi relasi manual biar EF gak bingung
            modelBuilder.Entity<OrderModel>()
                .HasOne(o => o.Customer)
                .WithMany(u => u.CustomerOrders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderModel>()
                .HasOne(o => o.Technician)
                .WithMany(u => u.TechnicianOrders)
                .HasForeignKey(o => o.TechnicianId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
