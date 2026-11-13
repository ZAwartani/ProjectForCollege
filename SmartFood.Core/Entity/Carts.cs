using SmartFood.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SmartFood.Core.Entity
{
    public class Carts : IEntity<int>
    {
        public Carts(int organizationId)
        {
            OrganizationId = organizationId;
            CreateDate = DateTime.UtcNow;
        }

        [Key]
        public int Id { get; set; }
        [ForeignKey(nameof(Organization))]
        public int OrganizationId { get; set; }
        public Organization Organization { get; set; }
        public DateTime CreateDate { get; set; }
        public ICollection<CartItem> CartItems { get; set; }
    }
}
