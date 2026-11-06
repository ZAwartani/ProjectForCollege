using SmartFood.Core.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartFood.Core.Entity
{
    public class VerificationDocument : IEntity<int>
    {
        [Key]
        public int Id { get; set; }
        public int? OrgnizationId { get; set; }
        public Organization Organization { get; set; }
        public int? RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }
        public string FilePath { get; set; } = null!;
        public VerificationDocument(int? OrgnizationId, int? RestaurantId, string filePath)
        {
            FilePath = filePath;
            this.RestaurantId = RestaurantId;
            this.OrgnizationId = OrgnizationId;
        }
    }
}