using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Store.Models;

namespace Store.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Showroom> Showrooms { get; set; }
        public DbSet<Motorcycle> Motorcycles { get; set; }
        public DbSet<InquiryHeader> InquiryHeaders { get; set; }
        public DbSet<InquiryDetail> InquiryDetails { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<MotorcycleImage> MotorcycleImages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Showroom>()
                .HasMany(s => s.Motorcycles)
                .WithOne(m => m.Showroom)
                .HasForeignKey(m => m.ShowroomId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<InquiryHeader>()
                .HasOne(i => i.Showroom)
                .WithMany()
                .HasForeignKey(i => i.ShowroomId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<InquiryDetail>()
                .HasOne(d => d.Motorcycle)
                .WithMany()
                .HasForeignKey(d => d.MotorcycleId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ShoppingCart>()
                .HasOne(c => c.Motorcycle)
                .WithMany()
                .HasForeignKey(c => c.MotorcycleId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}