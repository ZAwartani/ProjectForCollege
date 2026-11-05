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

            [ForeignKey(nameof(VerificationDocument))]
            public int? VerificationId { get; set; }
            public VerificationDocument VerificationDocument { get; set; }
            public string Email { get; set; }
            public string PasswordHash { get; set; }
            public string RestaurantName { get; set; } = null!;
            public string OwnerName { get; set; } = null!;
            public string BusinessRegistrationNumber { get; set; } = null!;
            public string Address { get; set; } = null!;
            public RegisterStatus Status { get; set; }
            public DateTime CreatedAt { get; set; }
            public ICollection<Tokens> Tokens { get; set; }
            public ICollection<CodeOTP> CodeOTPs { get; set; }
            public Restaurant(
                int idVerification,
                string email, 
                string passwordHash, 
                string restaurantName, 
                string ownerName, 
                string businessRegistrationNumber, 
                string address)
            {
                VerificationId = idVerification;
                Email = email;
                PasswordHash = passwordHash;
                RestaurantName = restaurantName;
                OwnerName = ownerName;
                BusinessRegistrationNumber = businessRegistrationNumber;
                Address = address;
                Status = RegisterStatus.Pending;
                CreatedAt = DateTime.UtcNow;
            }
    }
}
