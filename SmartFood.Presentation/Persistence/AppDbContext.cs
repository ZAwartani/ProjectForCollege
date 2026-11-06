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

            modelBuilder.Entity<Role>()
                .Property(r => r.Id)
                .ValueGeneratedNever();

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

            modelBuilder.Entity<Restaurant>()
                .HasIndex(r => r.Email)
                .IsUnique();

            modelBuilder.Entity<Organization>()
                .HasIndex(o => o.Email)
                .IsUnique();

            modelBuilder.Entity<Tokens>()
                .HasOne(t => t.Organization)
                .WithMany(o => o.Tokens)
                .HasForeignKey(t => t.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CodeOTP>()
                .HasOne(r => r.Organization)
                .WithMany(o => o.CodeOTPs)
                .HasForeignKey(r => r.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CodeOTP>()
                .HasOne(r => r.Restaurant)
                .WithMany(o => o.CodeOTPs)
                .HasForeignKey(r => r.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            // Restaurant role
            modelBuilder.Entity<Restaurant>()
                .HasOne(r => r.Role)
                .WithMany(r => r.Restaurants)
                .HasForeignKey(r => r.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Organization role
            modelBuilder.Entity<Organization>()
                .HasOne(o => o.Role)
                .WithMany(r => r.Organizations)
                .HasForeignKey(o => o.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Organization's selected verification document
            modelBuilder.Entity<Organization>()
                .HasOne(o => o.VerificationDocument)
                .WithMany()
                .HasForeignKey(o => o.VerificationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Restaurant's selected verification document
            modelBuilder.Entity<Restaurant>()
                .HasOne(r => r.VerificationDocument)
                .WithMany()
                .HasForeignKey(r => r.VerificationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Document's organization owner
            modelBuilder.Entity<VerificationDocument>()
                .HasOne(d => d.Organization)
                .WithMany()
                .HasForeignKey(d => d.OrgnizationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Document's restaurant owner
            modelBuilder.Entity<VerificationDocument>()
                .HasOne(d => d.Restaurant)
                .WithMany()
                .HasForeignKey(d => d.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VerificationDocument>()
                .HasIndex(x => x.OrgnizationId);

            modelBuilder.Entity<VerificationDocument>()
                .HasIndex(x => x.RestaurantId);
        }
    }
}