
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
        public DbSet<Admin> Admins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================
            // ROLE CONFIGURATION
            // =========================

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

            // =========================
            // UNIQUE EMAILS
            // =========================

            modelBuilder.Entity<Restaurant>()
                .HasIndex(r => r.Email)
                .IsUnique();

            modelBuilder.Entity<Admin>()
                .HasIndex(r => r.Email)
                .IsUnique();

            modelBuilder.Entity<Organization>()
                .HasIndex(o => o.Email)
                .IsUnique();

            // =========================
            // ORGANIZATION TOKENS
            // =========================

            modelBuilder.Entity<Tokens>()
                .HasOne(t => t.Organization)
                .WithMany(o => o.Tokens)
                .HasForeignKey(t => t.OrganizationId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Tokens>()
                .HasOne(t => t.Restaurant)
                .WithMany(o => o.Tokens)
                .HasForeignKey(t => t.RestaurantId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // OTP RELATIONSHIPS
            // =========================

            modelBuilder.Entity<CodeOTP>()
                .HasOne(c => c.Organization)
                .WithMany(o => o.CodeOTPs)
                .HasForeignKey(c => c.OrganizationId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CodeOTP>()
                .HasOne(c => c.Restaurant)
                .WithMany(r => r.CodeOTPs)
                .HasForeignKey(c => c.RestaurantId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            // Every OTP must belong to exactly one owner
            modelBuilder.Entity<CodeOTP>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_CodeOTP_Owner",
                    """
                    (
                        RestaurantId IS NOT NULL
                        AND OrganizationId IS NULL
                    )
                    OR
                    (
                        RestaurantId IS NULL
                        AND OrganizationId IS NOT NULL
                    )
                    """));

            // =========================
            // RESTAURANT ROLE
            // =========================

            modelBuilder.Entity<Restaurant>()
                .HasOne(r => r.Role)
                .WithMany(r => r.Restaurants)
                .HasForeignKey(r => r.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // ORGANIZATION ROLE
            // =========================

            modelBuilder.Entity<Organization>()
                .HasOne(o => o.Role)
                .WithMany(r => r.Organizations)
                .HasForeignKey(o => o.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // ORGANIZATION VERIFICATION
            // =========================

            modelBuilder.Entity<Organization>()
                .HasOne(o => o.VerificationDocument)
                .WithMany()
                .HasForeignKey(o => o.VerificationId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // RESTAURANT VERIFICATION
            // =========================

            modelBuilder.Entity<Restaurant>()
                .HasOne(r => r.VerificationDocument)
                .WithMany()
                .HasForeignKey(r => r.VerificationId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // VERIFICATION DOCUMENT OWNERS
            // =========================

            modelBuilder.Entity<VerificationDocument>()
                .HasOne(d => d.Organization)
                .WithMany()
                .HasForeignKey(d => d.OrgnizationId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VerificationDocument>()
                .HasOne(d => d.Restaurant)
                .WithMany()
                .HasForeignKey(d => d.RestaurantId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VerificationDocument>()
                .ToTable(t => t.HasCheckConstraint(
                    "CK_VerificationDocument_Owner",
                    """
                    (
                        OrgnizationId IS NOT NULL
                        AND RestaurantId IS NULL
                    )
                    OR
                    (
                        OrgnizationId IS NULL
                        AND RestaurantId IS NOT NULL
                    )
                    """));

            modelBuilder.Entity<VerificationDocument>()
                .HasIndex(x => x.OrgnizationId);

            modelBuilder.Entity<VerificationDocument>()
                .HasIndex(x => x.RestaurantId);
        }
    }
}
