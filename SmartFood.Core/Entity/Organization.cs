using SmartFood.Core.Enums;
using SmartFood.Core.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartFood.Core.Entity
{
    public class Organization : IEntity<int>
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(IdVerificationDoc))]
        public int IdVerificationDoc { get; set; }
        public VerificationDocument VerificationDocument { get; set; }
        public string OrganizationName { get; set; } = null!;
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string ResponsiblePerson { get; set; } = null!;
        public string RegistrationNumber { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string OrganizationType { get; set; } = null!;
        public RegisterStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<Tokens> Tokens { get; set; }
        public ICollection<CodeOTP> CodeOTPs { get; set; }
        public Organization(
            string organizationName,
            string responsiblePerson,
            string registrationNumber,
            string address,
            string organizationType)
        {
            OrganizationName = organizationName;
            ResponsiblePerson = responsiblePerson;
            RegistrationNumber = registrationNumber;
            Address = address;
            OrganizationType = organizationType;
            Status = RegisterStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }
    }
}