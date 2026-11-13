using Microsoft.AspNetCore.Http;
using SmartFood.Core.Entity;
using SmartFood.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFood.Application.DTOs
{
    public record CreateProducrDto(
        string Name,
        string Description,
        int Quantity,
        IFormFile Photo
    );

    public record GetProductDto(
        int Id,
        int RestaurantId,
        string Name,
        string Description,
        int Quantity,
        string PathPhoto
    );

    public record UpdateProductDto(
        string? Name,
        string? Description,
        int? Quantity,
        IFormFile? Photo
    );

    public record RestaurantProfileDto(
    int Id,
    string Name,
    string OwnerName,
    string Email,
    string Address,
    double? Latitude,
    double? Longitude,
    RegisterStatus Status
    );

    public record UpdateRestaurantProfileDto(
    string? Name,
    string? OwnerName,
    string? Address
    );

    public record UpdateRestaurantLocationDto(double Latitude, double Longitude);

}
