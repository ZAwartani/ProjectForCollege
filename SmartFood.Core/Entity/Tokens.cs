using SmartFood.Core.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartFood.Core.Entity
{
    public class Tokens : IEntity<int>
    {
        public Tokens(int? OrganizationId, 
            int? RestaurantId, 
            string refreshToken , 
            DateTime refreshTokenExpir , 
            DateTime? refreshTokenRevokedAt,
            int? AdminId = null)
        {
            this.OrganizationId = OrganizationId;
            this.RestaurantId = RestaurantId;
            this.AdminId = AdminId;
            RefreshToken = refreshToken;
            CreatedDate = DateTime.UtcNow;
            RefreshTokenExpir = refreshTokenExpir;
            RefreshTokenRevokedAt = refreshTokenRevokedAt;
        }

        [Key]
        public int Id { get; set; }
        [ForeignKey(nameof(Organization))]
        public int? OrganizationId { get; set; }
        public Organization Organization { get; set; }
        [ForeignKey(nameof(Restaurant))]
        public int? RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }
        [ForeignKey(nameof(AdminId))]
        public int? AdminId { get; set; }
        public Admin Admin { get; set; }
        public string RefreshToken { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime RefreshTokenExpir { get; set; }
        public DateTime? RefreshTokenRevokedAt { get; set; }
    }
}
