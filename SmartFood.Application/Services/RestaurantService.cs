using SmartFood.Application.DTOs;
using SmartFood.Application.Interfaces;
using SmartFood.Application.Interfaces.Factory;
using SmartFood.Application.Interfaces.Specification;
using SmartFood.Core.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using static SmartFood.Application.Interfaces.IRestaurantService;

namespace SmartFood.Application.Services
{
    public class RestaurantService : IRestaurantService
    {
        private readonly IGenericRepository<Product, int> _RepoProduct;
        private readonly IGenericRepository<Restaurant, int> _RepoRestaurant;
        private readonly ISpecificationBuilderFactory _Factory;
        private readonly IFileStorageService _SaveFile;
        private readonly ICurrentUser _CurrentUser;
        public RestaurantService(
            IGenericRepository<Product, int> repoProduct, 
            IFileStorageService saveFile,
            IGenericRepository<Restaurant, int> _RepoRestaurant,
            ISpecificationBuilderFactory _Factory,
            ICurrentUser _CurrentUser)
        {
            this._RepoRestaurant = _RepoRestaurant;
            _RepoProduct = repoProduct;
            _SaveFile = saveFile;
            this._Factory = _Factory;
            this._CurrentUser = _CurrentUser;
        }

        private int RestaurantId => _CurrentUser.Id;

        public async Task<bool> CreateProduct(CreateProducrDto dto)
        {
            var restaurant = await _RepoRestaurant.GetByIdAsync(RestaurantId);

            if (restaurant == null)
            {
                return false;
            }

            var relativeFolder = Path.Combine(
                "Restaurants",
                restaurant.RestaurantName,
                "Products"
            );

            var PathPhoto = await _SaveFile.SaveFileAsync(
                dto.Photo,
                relativeFolder
            );

            var Result = new Product(
                RestaurantId,
                dto.Name,
                dto.Description,
                dto.Quantity,
                PathPhoto
            );

            await _RepoProduct.AddAsync(Result);

            return true;
        }

        public async Task<IReadOnlyList<GetProductDto>> ProductList()
        {
            var specification = _Factory.CreateBuilder<Product>()
                .AddCriteria(p => p.RestaurantId == RestaurantId)
                .AddCriteria(x => x.CreateDate >= DateTime.UtcNow.AddDays(-1));

            var products = await _RepoProduct.ListAsync(specification);
            return products
                .Select(p => new GetProductDto(
                p.Id,
                p.RestaurantId,
                p.Name,
                p.Description,
                p.Quantity,
                p.PhotoPath
            ))
            .ToList();
        }

        public async Task<bool> UpdateProduct(UpdateProductDto dto, int Id)
        {
            var specification = _Factory.CreateBuilder<Product>()
                .AddCriteria(x => x.RestaurantId == RestaurantId)
                .AddCriteria(x => x.Id == Id)
                .AddCriteria(x => x.CreateDate >= DateTime.UtcNow.AddDays(-1));

            var product = await _RepoProduct.GetAsync(specification);
            
            if(product == null)
            {
                return false;
            }

            if (dto.Name != null)
                product.Name = dto.Name;

            if (dto.Description != null)
                product.Description = dto.Description;

            if (dto.Quantity.HasValue)
                product.Quantity = dto.Quantity.Value;

            if (dto.Photo != null && dto.Photo.Length > 0)
            {
                var newPhotoPath = await _SaveFile.SaveFileAsync(
                    dto.Photo,
                    dto.Name
                );

                if (!string.IsNullOrEmpty(newPhotoPath))
                {
                    product.PhotoPath = newPhotoPath;
                }
            }

            await _RepoProduct.UpdateAsync(product);
            return true;
        }


        public async Task<bool> RemoveProduct(int Id)
        {
            var Specification = _Factory.CreateBuilder<Product>()
                .AddCriteria(x => x.RestaurantId == RestaurantId)
                .AddCriteria(x => x.Id == Id)
                .AddCriteria(x => x.CreateDate >= DateTime.UtcNow.AddDays(-1));

            var Result = await _RepoProduct.GetAsync(Specification);
            if(Result == null)
            {
                throw new Exception("We Don't Have This Product");
            }
            await _SaveFile.DeleteFileAsync(Result.PhotoPath);
            await _RepoProduct.DeleteAsync(Result.Id);
            return true;
        }

        public async Task<RestaurantProfileDto?> GetMyProfile()
        {
            var specification = _Factory.CreateBuilder<Restaurant>()
                .AddCriteria(x => x.Id == RestaurantId);

            var restaurant = await _RepoRestaurant.GetAsync(specification);

            if (restaurant == null)
                return null;

            return new RestaurantProfileDto(
                restaurant.Id,
                restaurant.RestaurantName,
                restaurant.OwnerName,
                restaurant.Email,
                restaurant.Address,
                restaurant.Latitude,
                restaurant.Longitude,
                restaurant.Status
            );
        }

        public async Task<bool> UpdateMyProfile(UpdateRestaurantProfileDto dto)
        {
            var specification = _Factory.CreateBuilder<Restaurant>()
                .AddCriteria(x => x.Id == RestaurantId);

            var restaurant = await _RepoRestaurant.GetAsync(specification);

            if (restaurant == null)
                return false;

            if (!string.IsNullOrWhiteSpace(dto.Name))
                restaurant.RestaurantName = dto.Name;

            if (!string.IsNullOrWhiteSpace(dto.OwnerName))
                restaurant.OwnerName = dto.OwnerName;

            if (!string.IsNullOrWhiteSpace(dto.Address))
                restaurant.Address = dto.Address;

            await _RepoRestaurant.UpdateAsync(restaurant);

            return true;
        }

        public async Task<bool> UpdateMyLocation(UpdateRestaurantLocationDto dto)
        {
            if (!double.IsFinite(dto.Latitude) ||
                !double.IsFinite(dto.Longitude))
                return false;

            if (dto.Latitude < -90 || dto.Latitude > 90)
                return false;

            if (dto.Longitude < -180 || dto.Longitude > 180)
                return false;

            var specification = _Factory.CreateBuilder<Restaurant>()
                .AddCriteria(x => x.Id == RestaurantId);

            var restaurant = await _RepoRestaurant.GetAsync(specification);

            if (restaurant == null)
                return false;

            restaurant.Latitude = dto.Latitude;
            restaurant.Longitude = dto.Longitude;

            await _RepoRestaurant.UpdateAsync(restaurant);

            return true;
        }

    }
}
