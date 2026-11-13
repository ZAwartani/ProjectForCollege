using SmartFood.Core.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SmartFood.Core.Entity
{
    public class Role : IEntity<int>
    {
        [Key]
        public int Id { get; set; }

        public string RoleName { get; set; }

        public ICollection<Restaurant> Restaurants { get; set; }
        public ICollection<Organization> Organizations { get; set; }
        public ICollection<Admin> Admins { get; set; }

    }
}