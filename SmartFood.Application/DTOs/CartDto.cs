using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SmartFood.Application.DTOs
{
    public record CartDto(
        IReadOnlyList<CartItemDto> Items
    );

    public record CartItemDto(
        int ProductId,
        string ProductName,
        int RestaurantId,
        string RestaurantName,
        string PhotoPath,
        int Quantity
    );

    public record AddCartItemDto(
        [Range(1, int.MaxValue)] 
        int ProductId,
        [Range(1, int.MaxValue)] 
        int Quantity
    );

    public record UpdateCartItemQuantityDto(
        [Range(1, int.MaxValue)]
        int Quantity
    );
}
