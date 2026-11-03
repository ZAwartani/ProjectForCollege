using SmartFood.Core.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartFood.Core.Entity
{
    public class VerificationDocument : IEntity<int>
    {
        [Key]
        public int Id { get; set; }
        public string FilePath { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public ICollection<Restaurant> Restaurants { get; set; }
        public ICollection<Organization> Organizations { get; set; }
        public VerificationDocument(string filePath, string fileName, DateTime? reviewedAt)
        {
            FilePath = filePath;
            FileName = fileName;
            UploadedAt = DateTime.UtcNow;
            ReviewedAt = reviewedAt;
        }
    }
}