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

            modelBuilder.Entity<VerificationDocument>()
                .HasIndex(x => x.OrgnizationId);

            modelBuilder.Entity<VerificationDocument>()
                .HasIndex(x => x.RestaurantId);

            modelBuilder.Entity<Organization>()
                .HasIndex(o => o.Email)
                .IsUnique();

            modelBuilder.Entity<CodeOTP>()
                .HasIndex(x => x.IdOrganization);

            modelBuilder.Entity<CodeOTP>()
                .HasIndex(x => x.IdRestaurant);



            modelBuilder.Entity<Tokens>()
                .HasIndex(x => x.IdOrganization);

            modelBuilder.Entity<Tokens>()
                .HasIndex(x => x.IdRestaurant);

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
                .OnDelete(DeleteBehavior.Cascade);

            // Restaurant's selected verification document
            modelBuilder.Entity<Restaurant>()
                .HasOne(r => r.VerificationDocument)
                .WithMany()
                .HasForeignKey(r => r.VerificationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Document's organization owner
            modelBuilder.Entity<VerificationDocument>()
                .HasOne(d => d.Organization)
                .WithMany()
                .HasForeignKey(d => d.OrgnizationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Document's restaurant owner
            modelBuilder.Entity<VerificationDocument>()
                .HasOne(d => d.Restaurant)
                .WithMany()
                .HasForeignKey(d => d.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}