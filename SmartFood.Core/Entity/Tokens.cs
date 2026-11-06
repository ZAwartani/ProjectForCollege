using SmartFood.Core.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartFood.Core.Entity
{
    public class Tokens : IEntity<int>
    {
        public Tokens(int? OrganizationId, int? RestaurantId, string refreshToken , DateTime refreshTokenExpir , DateTime? refreshTokenRevokedAt)
        {
            this.OrganizationId = OrganizationId;
            this.RestaurantId = RestaurantId;
            RefreshToken = refreshToken;
            CreatedDate = DateTime.UtcNow;
            RefreshTokenExpir = refreshTokenExpir;
            RefreshTokenRevokedAt = refreshTokenRevokedAt;
        }

        [Key]
        public int Id { get; set; }
        public int? OrganizationId { get; set; }
        public Organization Organization { get; set; }
        public int? RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }
        public string RefreshToken { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime RefreshTokenExpir { get; set; }
        public DateTime? RefreshTokenRevokedAt { get; set; }
    }
}
