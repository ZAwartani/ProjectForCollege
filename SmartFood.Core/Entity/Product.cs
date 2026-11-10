using SmartFood.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SmartFood.Core.Entity
{
    public class Product : IEntity<int>
    {
        private int _quantity;

        [Key]
        public int Id { get; set; }
        [ForeignKey(nameof(Restaurant))]
        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Quantity can't be negative.");

                _quantity = value;
            }
        }
        public DateTime CreateDate{ get; set; }
        public string PhotoPath { get; set; }

        public Product(
            int restaurantId,
            string name,
            string description,
            int quantity,
            string photoPath)
        {
            RestaurantId = restaurantId;
            Name = name;
            Description = description;
            Quantity = quantity;
            CreateDate = DateTime.UtcNow;
            PhotoPath = photoPath;
        }
    }
}
