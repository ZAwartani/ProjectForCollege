using SmartFood.Core.Enums;
using SmartFood.Core.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartFood.Core.Entity
{
    public class Restaurant : IEntity<int>
    {
        [Key]
        public int Id { get; set; }

        public int RoleId { get; set; }

        public Role Role { get; set; }

        public int? VerificationId { get; set; }
        public VerificationDocument VerificationDocument { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string RestaurantName { get; set; } = null!;
        public string OwnerName { get; set; } = null!;
        public string BusinessRegistrationNumber { get; set; } = null!;
        public string Address { get; set; } = null!;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public RegisterStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<Tokens> Tokens { get; set; }
        public ICollection<CodeOTP> CodeOTPs { get; set; }

        public Restaurant(
                int? VerificationId,
                string email,
                string passwordHash,
                string restaurantName,
                string ownerName,
                string businessRegistrationNumber,
                string address,
                double? Longitude,
                double? Latitude
            )
        {
            this.VerificationId = VerificationId;
            Email = email;
            PasswordHash = passwordHash;
            RestaurantName = restaurantName;
            OwnerName = ownerName;
            BusinessRegistrationNumber = businessRegistrationNumber;
            Address = address;
            Status = RegisterStatus.Pending;
            CreatedAt = DateTime.UtcNow;
            RoleId = (int)UserType.Restaurant;
            this.Longitude = Longitude;
            this.Latitude = Latitude;
        }
    }
}
