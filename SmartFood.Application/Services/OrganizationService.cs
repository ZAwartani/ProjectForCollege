using SmartFood.Application.DTOs;
using SmartFood.Application.Interfaces;
using SmartFood.Application.Interfaces.Factory;
using SmartFood.Application.Interfaces.Specification;
using SmartFood.Core.Entity;
using SmartFood.Core.Enums;

namespace SmartFood.Application.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IGenericRepository<Organization, int> _RepoOrganization;
        private readonly IGenericRepository<Restaurant, int> _RepoRestaurant;
        private readonly ISpecificationBuilderFactory _Factory;
        private readonly ICurrentUser _CurrentUser;
        private readonly IFileStorageService _FileUploader;
        private readonly IGenericRepository<Product, int> _RepoProduct;

        public OrganizationService(
            IGenericRepository<Organization, int> repoOrganization,
            IGenericRepository<Restaurant, int> repoRestaurant,
            ISpecificationBuilderFactory factory,
            ICurrentUser currentUser,
            IFileStorageService fileUploader,
            IGenericRepository<Product, int> repoProduct)
        {
            _RepoOrganization = repoOrganization;
            _RepoRestaurant = repoRestaurant;
            _Factory = factory;
            _CurrentUser = currentUser;
            _FileUploader = fileUploader;
            _RepoProduct = repoProduct;
        }

        private int IdOrganization => _CurrentUser.Id;

        public async Task<OrganizationProfileDto?> GetMyProfileAsync()
        {
            var specification = _Factory.CreateBuilder<Organization>()
                .AddCriteria(x => x.Id == IdOrganization);

            var organization = await _RepoOrganization.GetAsync(specification);

            if (organization == null)
                return null;

            return new OrganizationProfileDto(
                organization.Id,
                organization.OrganizationName,
                organization.OwnerName,
                organization.Email,
                organization.RegistrationNumber,
                organization.Address,
                organization.Latitude,
                organization.Longitude,
                organization.Status,
                organization.PhoneNumber
            );
        }

        public async Task<OrganizationProductDto?> GetProductAsync(int id)
        {
            var cutoff = DateTime.UtcNow.AddDays(-1);

            var Specification = _Factory.CreateBuilder<Product>()
                .AddCriteria(p =>
                    p.Id == id &&
                    p.Quantity > 0 &&
                    p.CreateDate >= cutoff &&
                    p.Restaurant.Status == RegisterStatus.Approved)
                .AddInclude(p => p.Restaurant);

            var ResultSpecification =
                await _RepoProduct.GetAsync(Specification);

            if (ResultSpecification == null)
                return null;

            return new OrganizationProductDto(
                ResultSpecification.Id,
                ResultSpecification.RestaurantId,
                ResultSpecification.Restaurant.RestaurantName,
                ResultSpecification.Name,
                ResultSpecification.Description,
                ResultSpecification.Quantity,
                ResultSpecification.PhotoPath,
                ResultSpecification.CreateDate
            );
        }

        public async Task<IReadOnlyList<OrganizationProductDto>> GetProductsAsync(
      OrganizationQueryProductDto query)
        {
            if (query.PageNumber < 1)
            {
                throw new ArgumentException(
                    "Page number must be at least 1.");
            }

            if (query.PageSize < 1 || query.PageSize > 100)
            {
                throw new ArgumentException(
                    "Page size must be between 1 and 100.");
            }

            if ((long)(query.PageNumber - 1) * query.PageSize > int.MaxValue)
            {
                throw new ArgumentException(
                    "Page number is too large.");
            }

            var search = query.Search?.Trim();
            var cutoff = DateTime.UtcNow.AddDays(-1);

            var specification = _Factory.CreateBuilder<Product>()
                .AddCriteria(p =>
                    p.Quantity > 0 &&
                    p.CreateDate >= cutoff &&
                    p.Restaurant.Status == RegisterStatus.Approved &&
                    (string.IsNullOrEmpty(search) ||
                     p.Name.Contains(search)))
                .AddInclude(p => p.Restaurant)
                .ApplyOrderBy(p => p.Id)
                .ApplyPaging(query.PageNumber, query.PageSize);

            var products = await _RepoProduct.ListAsync(specification);

            return products.Select(p => new OrganizationProductDto(
                p.Id,
                p.RestaurantId,
                p.Restaurant.RestaurantName,
                p.Name,
                p.Description,
                p.Quantity,
                p.PhotoPath,
                p.CreateDate
            )).ToList();
        }

        public async Task<OrganizationRestaurantDetailsDto?> GetRestaurantAsync(
            int id)
        {
            var specification = _Factory.CreateBuilder<Restaurant>()
                .AddCriteria(r =>
                    r.Id == id &&
                    r.Status == RegisterStatus.Approved &&
                    r.Latitude.HasValue &&
                    r.Longitude.HasValue);

            var restaurant = await _RepoRestaurant.GetAsync(specification);

            if (restaurant == null)
                return null;

            var cutoff = DateTime.UtcNow.AddDays(-1);

            var productSpecification = _Factory.CreateBuilder<Product>()
                .AddCriteria(p =>
                    p.RestaurantId == restaurant.Id &&
                    p.Quantity > 0 &&
                    p.CreateDate >= cutoff)
                .ApplyOrderBy(p => p.Id);

            var products = await _RepoProduct.ListAsync(productSpecification);

            var productDtos = products.Select(p => new OrganizationProductDto(
                p.Id,
                p.RestaurantId,
                restaurant.RestaurantName,
                p.Name,
                p.Description,
                p.Quantity,
                p.PhotoPath,
                p.CreateDate
            )).ToList();

            return new OrganizationRestaurantDetailsDto(
                restaurant.Id,
                restaurant.RestaurantName,
                restaurant.Address,
                restaurant.Latitude.Value,
                restaurant.Longitude.Value,
                productDtos
            );
        }

        public async Task<IReadOnlyList<OrganizationRestaurantDto>> GetRestaurantsAsync()
        {
            var specification = _Factory.CreateBuilder<Restaurant>()
                .AddCriteria(r =>
                    r.Status == RegisterStatus.Approved &&
                    r.Latitude.HasValue &&
                    r.Longitude.HasValue)
                .ApplyOrderBy(r => r.Id);

            var restaurants = await _RepoRestaurant.ListAsync(specification);

            return restaurants.Select(r => new OrganizationRestaurantDto(
                r.Id,
                r.RestaurantName,
                r.Address,
                r.Latitude.Value,
                r.Longitude.Value
            )).ToList();
        }

        public async Task<bool> UpdateMyLocationAsync(
            UpdateOrganizationLocationDto dto)
        {
            if (!double.IsFinite(dto.Latitude) ||
                dto.Latitude < -90 ||
                dto.Latitude > 90)
            {
                throw new ArgumentException(
                    "Latitude must be between -90 and 90.");
            }

            if (!double.IsFinite(dto.Longitude) ||
                dto.Longitude < -180 ||
                dto.Longitude > 180)
            {
                throw new ArgumentException(
                    "Longitude must be between -180 and 180.");
            }

            var specification = _Factory.CreateBuilder<Organization>()
                .AddCriteria(x => x.Id == IdOrganization);

            var organization = await _RepoOrganization.GetAsync(specification);

            if (organization == null)
                return false;

            organization.Latitude = dto.Latitude;
            organization.Longitude = dto.Longitude;

            await _RepoOrganization.UpdateAsync(organization);

            return true;
        }

        public async Task<bool> UpdateMyProfileAsync(
    UpdateOrganizationProfileDto dto)
        {
            if (dto.OwnerName != null &&
                string.IsNullOrWhiteSpace(dto.OwnerName))
            {
                throw new ArgumentException("Owner name cannot be empty.");
            }

            if (dto.Address != null &&
                string.IsNullOrWhiteSpace(dto.Address))
            {
                throw new ArgumentException("Address cannot be empty.");
            }

            string? phoneNumber = null;

            if (dto.PhoneNumber != null)
            {
                phoneNumber = dto.PhoneNumber.Trim();

                var digits = phoneNumber.StartsWith("+")
                    ? phoneNumber.Substring(1)
                    : phoneNumber;

                if (digits.Length < 7 ||
                    digits.Length > 15 ||
                    digits.Any(c => c < '0' || c > '9'))
                {
                    throw new ArgumentException(
                        "Phone number must contain 7 to 15 digits, " +
                        "with an optional leading +.");
                }
            }

            var specification = _Factory.CreateBuilder<Organization>()
                .AddCriteria(x => x.Id == IdOrganization);

            var organization = await _RepoOrganization.GetAsync(specification);

            if (organization == null)
                return false;

            if (dto.OwnerName != null)
                organization.OwnerName = dto.OwnerName.Trim();

            if (dto.Address != null)
                organization.Address = dto.Address.Trim();

            if (phoneNumber != null)
                organization.PhoneNumber = phoneNumber;

            await _RepoOrganization.UpdateAsync(organization);

            return true;
        }
    }
}