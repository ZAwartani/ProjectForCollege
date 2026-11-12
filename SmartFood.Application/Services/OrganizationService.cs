using SmartFood.Application.DTOs;
using SmartFood.Application.Interfaces;
using SmartFood.Application.Interfaces.Factory;
using SmartFood.Application.Interfaces.Specification;
using SmartFood.Core.Entity;
using SmartFood.Core.Enums;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

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
            var Specification = _Factory.CreateBuilder<Organization>()
                .AddCriteria(x => x.Id == IdOrganization);

            var ResultSpecification = await _RepoOrganization.GetAsync(Specification);
            if (ResultSpecification == null)
            {
                return null;
            }
            return new OrganizationProfileDto(
                    ResultSpecification.Id,
                    ResultSpecification.OrganizationName,
                    ResultSpecification.OwnerName,
                    ResultSpecification.Email,
                    ResultSpecification.RegistrationNumber,
                    ResultSpecification.Address,
                    ResultSpecification.Latitude,
                    ResultSpecification.Longitude,
                    ResultSpecification.Status
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

        public async Task<IReadOnlyList<OrganizationProductDto>> GetProductsAsync(OrganizationQueryProductDto query)
        {
            if (query.PageNumber < 1)
                throw new ArgumentException("Page number must be at least 1.");

            if (query.PageSize < 1  || query.PageSize > 100)
            {
                throw new ArgumentException(
                    "Page size must be between 1 and 100."
                );
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

        public async Task<OrganizationRestaurantDetailsDto?> GetRestaurantAsync(int id)
        {
            var SpeicificationRestaurant = _Factory.CreateBuilder<Restaurant>()
                .AddCriteria(x => x.Id == id && x.Status == RegisterStatus.Approved);
         
            var ResultRestaurant = await _RepoRestaurant.GetAsync(SpeicificationRestaurant);
            if(ResultRestaurant == null)
            {
                return null;
            }
            var CutOff = DateTime.UtcNow.AddDays(-1);
            var SpeificationProduct = _Factory.CreateBuilder<Product>()
                .AddCriteria(x => x.RestaurantId == ResultRestaurant.Id &&
                                  x.Quantity > 0 &&
                                  x.CreateDate > CutOff);

            var ResultProduct = await _RepoProduct.ListAsync(SpeificationProduct);

            if(ResultRestaurant == null)
            {
                return null;
            }
            var ReturnDataProduct = ResultProduct.Select(x => new OrganizationProductDto(
                    x.Id,
                    x.RestaurantId,
                    ResultRestaurant.RestaurantName,
                    x.Name,
                    x.Description,
                    x.Quantity,
                    x.PhotoPath,
                    x.CreateDate
                )).ToList();
                
            var ResultRequest = new OrganizationRestaurantDetailsDto(
                    ResultRestaurant.Id,
                    ResultRestaurant.RestaurantName,
                    ResultRestaurant.Address,
                    ResultRestaurant.Latitude,
                    ResultRestaurant.Longitude,
                    ReturnDataProduct
            );
            return ResultRequest;
        }

        public async Task<IReadOnlyList<OrganizationRestaurantDto>> GetRestaurantsAsync()
        {
            var Specification = _Factory.CreateBuilder<Restaurant>()
                .AddCriteria(x => x.Status == RegisterStatus.Approved);

            var result = await _RepoRestaurant.ListAsync(Specification);
            
            return result.Select(x => new OrganizationRestaurantDto
            (
                x.Id,
                x.RestaurantName,
                x.Address,
                x.Latitude,
                x.Longitude
            )).ToList();
        }

        public async Task<bool> UpdateMyLocationAsync(UpdateOrganizationLocationDto dto)
        {
            var Specification = _Factory.CreateBuilder<Organization>()
                .AddCriteria(x => x.Id == IdOrganization);

            var ResultSpecification = await _RepoOrganization.GetAsync(Specification);
            
            if (ResultSpecification == null)
            {
                return false;
            }

            ResultSpecification.Longitude = dto.Longitude;
            ResultSpecification.Latitude = dto.Latitude;

            await _RepoOrganization.UpdateAsync(ResultSpecification);
            return true;
        }

        public async Task<bool> UpdateMyProfileAsync(UpdateOrganizationProfileDto dto)
        {
            var Specification = _Factory.CreateBuilder<Organization>()
                .AddCriteria(x => x.Id == IdOrganization);

            var ResultSpecification = await _RepoOrganization.GetAsync(Specification);

            if(ResultSpecification == null)
            {
                return false;
            }
            if (dto.OwnerName != null)
            {
                if (string.IsNullOrWhiteSpace(dto.OwnerName))
                    throw new ArgumentException("Owner name cannot be empty.");

                ResultSpecification.OwnerName = dto.OwnerName.Trim();
            }

            if (dto.Address != null)
            {
                if (string.IsNullOrWhiteSpace(dto.Address))
                    throw new ArgumentException("Address cannot be empty.");

                ResultSpecification.Address = dto.Address.Trim();
            }

            await _RepoOrganization.UpdateAsync(ResultSpecification);
            return true;
        }
    }
}
