using SmartFood.Core.Enums;
using SmartFood.Core.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartFood.Core.Entity
{
    public class Organization : IEntity<int>
    {
        public Organization(
            int? verificationId,
            string organizationName, string email, string passwordHash,
            string ownerName, string registrationNumber, string address,
            double? latitude, double? longitude , int? cartId
           )
        {
            VerificationId = verificationId;
            OrganizationName = organizationName;
            Email = email;
            PasswordHash = passwordHash;
            OwnerName = ownerName;
            RegistrationNumber = registrationNumber;
            Address = address;
            Latitude = latitude;
            Longitude = longitude;
            Status = RegisterStatus.Pending;
            CreatedAt = DateTime.UtcNow;
            RoleId = (int)UserType.Organization;
            CartId = cartId;
        }

        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Role))]
        public int RoleId { get; set; }
        public Role Role { get; set; }

        [ForeignKey(nameof(VerificationDocument))]
        public int? VerificationId { get; set; }
        public VerificationDocument VerificationDocument { get; set; }
        [ForeignKey(nameof(Carts))]
        public int? CartId { get; set; }
        public Carts Carts { get; set; }
        public string OrganizationName { get; set; } = null!;
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string PasswordHash { get; set; }
        public string OwnerName { get; set; } = null!;
        public string RegistrationNumber { get; set; } = null!;
        public string Address { get; set; } = null!;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public RegisterStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<Tokens> Tokens { get; set; }
        public ICollection<CodeOTP> CodeOTPs { get; set; }
    }
}