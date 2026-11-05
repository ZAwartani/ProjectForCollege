using Microsoft.EntityFrameworkCore;
using SmartFood.Core.Entity;
using SmartFood.Core.Enums;

namespace SmartFood.Presentation.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Restaurant> Restaurants { get; set; }

        public DbSet<Organization> Organizations { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<VerificationDocument> VerificationDocuments { get; set; }

        public DbSet<CodeOTP> CodeOTPs { get; set; }

        public DbSet<Tokens> Tokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fixed role IDs
            modelBuilder.Entity<Role>()
                .Property(r => r.Id)
                .ValueGeneratedNever();

            // Role seed data
            modelBuilder.Entity<Role>()
                .HasData(
                    new Role
                    {
                        Id = (int)UserType.Admin,
                        RoleName = "Admin"
                    },
                    new Role
                    {
                        Id = (int)UserType.Restaurant,
                        RoleName = "Restaurant"
                    },
                    new Role
                    {
                        Id = (int)UserType.Organization,
                        RoleName = "Organization"
                    }
                );

            // Unique emails
            modelBuilder.Entity<Restaurant>()
                .HasIndex(r => r.Email)
                .IsUnique();

            modelBuilder.Entity<Organization>()
                .HasIndex(o => o.Email)
                .IsUnique();

            // Restaurant role
            modelBuilder.Entity<Restaurant>()
                .HasOne(r => r.Role)
                .WithMany()
                .HasForeignKey(r => r.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Organization role
            modelBuilder.Entity<Organization>()
                .HasOne(o => o.Role)
                .WithMany()
                .HasForeignKey(o => o.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}