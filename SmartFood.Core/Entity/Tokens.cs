using SmartFood.Core.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartFood.Core.Entity
{
    public class Tokens : IEntity<int>
    {
        public Tokens(int? idOrganization, int? idRestaurant, string refreshToken , DateTime refreshTokenExpir , DateTime? refreshTokenRevokedAt)
        {
            IdOrganization = idOrganization;
            IdRestaurant = idRestaurant;
            RefreshToken = refreshToken;
            CreatedDate = DateTime.UtcNow;
            RefreshTokenExpir = refreshTokenExpir;
            RefreshTokenRevokedAt = refreshTokenRevokedAt;
        }

        [Key]
        public int Id { get; set; }
        [ForeignKey(nameof(Organization))]
        public int? IdOrganization { get; set; }
        public Organization Organization { get; set; }

        [ForeignKey(nameof(Restaurant))]
        public int? IdRestaurant{ get; set; }
        public Restaurant Restaurant { get; set; }
        public string RefreshToken { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime RefreshTokenExpir { get; set; }
        public DateTime? RefreshTokenRevokedAt { get; set; }
    }
}
