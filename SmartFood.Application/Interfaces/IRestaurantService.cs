using SmartFood.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFood.Application.Interfaces
{
    public interface IRestaurantService
    {
        Task<bool> CreateProduct(CreateProducrDto dto);
        Task<IReadOnlyList<GetProductDto>> ProductList();
        Task<bool> UpdateProduct(UpdateProductDto dto , int Id);
        Task<bool> RemoveProduct(int Id);
        Task<RestaurantProfileDto?> GetMyProfile();
        Task<bool> UpdateMyProfile(UpdateRestaurantProfileDto dto);
        Task<bool> UpdateMyLocation(UpdateRestaurantLocationDto dto);
    }
}


/*
 1.Add product + photo => {
            RestaurantId = restaurantId;
            Name = name;
            Description = description;
            Quantity = quantity;
            CreateDate = createDate;
            pathphoto = pathphoto
 } DONE


 2.List my products => With RestaurantId and the Id Product
 DONE
 
 3.Update product + optionally replace photo
   DONE
 
 4.Delete product + photo 
DONE
    
 5.View my profile and approval status
    
 6.Update my profile

 7.Set/update my location

*/