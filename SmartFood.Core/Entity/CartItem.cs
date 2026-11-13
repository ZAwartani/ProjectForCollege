using SmartFood.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SmartFood.Core.Entity
{
    public class CartItem : IEntity<int>
    {
        public CartItem(int cartId, int productId, int quantity)
        {
            CartId = cartId;
            ProductId = productId;
            Quantity = quantity;
        }
        public CartItem(int productId, int quantity)
        {
            ProductId = productId;
            Quantity = quantity;
        }
        public int Id { get; set; }
        
        [ForeignKey(nameof(Carts))]
        public int CartId { get; set; }
        public Carts Carts { get; set; }

        [ForeignKey(nameof(Product))]
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
    }
}
