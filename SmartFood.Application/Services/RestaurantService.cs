using SmartFood.Application.DTOs;
using SmartFood.Application.Interfaces;
using SmartFood.Application.Interfaces.Factory;
using SmartFood.Application.Interfaces.Specification;
using SmartFood.Core.Entity;

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
                return false;

            var product = new Product(
                RestaurantId,
                dto.Name,
                dto.Description,
                dto.Quantity,
                string.Empty
            );

            var relativeFolder = Path.Combine(
                "Restaurants",
                restaurant.RestaurantName,
                "Products"
            );

            var photoPath = await _SaveFile.SaveFileAsync(
                dto.Photo,
                relativeFolder
            );

            product.PhotoPath = photoPath;

            try
            {
                await _RepoProduct.AddAsync(product);
            }
            catch
            {
                await _SaveFile.DeleteFileAsync(photoPath);
                throw;
            }

            return true;
        }

        public async Task<IReadOnlyList<GetProductDto>> ProductList()
        {
            var cutoff = DateTime.UtcNow.AddDays(-1);

            var specification = _Factory.CreateBuilder<Product>()
                .AddCriteria(p =>
                    p.RestaurantId == RestaurantId &&
                    p.CreateDate >= cutoff);

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
            var cutoff = DateTime.UtcNow.AddDays(-1);

            var specification = _Factory.CreateBuilder<Product>()
                .AddCriteria(p =>
                    p.RestaurantId == RestaurantId &&
                    p.Id == Id &&
                    p.CreateDate >= cutoff);

            var product = await _RepoProduct.GetAsync(specification);

            if (product == null)
                return false;

            var oldPhotoPath = product.PhotoPath;
            string? newPhotoPath = null;

            if (dto.Name != null)
                product.Name = dto.Name;

            if (dto.Description != null)
                product.Description = dto.Description;

            if (dto.Quantity.HasValue)
                product.Quantity = dto.Quantity.Value;

            if (dto.Photo != null)
            {
                var restaurant = await _RepoRestaurant.GetByIdAsync(RestaurantId);

                if (restaurant == null)
                    return false;

                var relativeFolder = Path.Combine(
                    "Restaurants",
                    restaurant.RestaurantName,
                    "Products"
                );

                newPhotoPath = await _SaveFile.SaveFileAsync(
                    dto.Photo,
                    relativeFolder
                );

                product.PhotoPath = newPhotoPath;
            }

            try
            {
                await _RepoProduct.UpdateAsync(product);
            }
            catch
            {
                if (newPhotoPath != null)
                    await _SaveFile.DeleteFileAsync(newPhotoPath);

                throw;
            }

            if (newPhotoPath != null)
                await _SaveFile.DeleteFileAsync(oldPhotoPath);

            return true;
        }

        public async Task<bool> RemoveProduct(int Id)
        {
            var cutoff = DateTime.UtcNow.AddDays(-1);

            var Specification = _Factory.CreateBuilder<Product>()
                .AddCriteria(p =>
                    p.RestaurantId == RestaurantId &&
                    p.Id == Id &&
                    p.CreateDate >= cutoff);

            var Result = await _RepoProduct.GetAsync(Specification);
            if (Result == null)
            {
                return false;
            }
            await _RepoProduct.DeleteAsync(Result.Id);
            await _SaveFile.DeleteFileAsync(Result.PhotoPath);

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
                .AddCriteria(r => r.Id == RestaurantId);

            var restaurant = await _RepoRestaurant.GetAsync(specification);

            if (restaurant == null)
                return false;

            if (!string.IsNullOrWhiteSpace(dto.Name) &&
                dto.Name.Trim() != restaurant.RestaurantName)
            {
                throw new ArgumentException(
                    "Restaurant name changes are temporarily unavailable " +
                    "because uploaded files are stored under that name."
                );
            }

            if (!string.IsNullOrWhiteSpace(dto.OwnerName))
                restaurant.OwnerName = dto.OwnerName.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Address))
                restaurant.Address = dto.Address.Trim();

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
