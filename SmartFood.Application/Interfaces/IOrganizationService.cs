using SmartFood.Application.DTOs;
namespace SmartFood.Application.Interfaces
{
    public interface IOrganizationService
    {
        // GET /api/customer/profile DONE !!
        Task<OrganizationProfileDto?> GetMyProfileAsync();

        // PUT /api/customer/profile DONE !!
        Task<bool> UpdateMyProfileAsync(UpdateOrganizationProfileDto dto);

        // PUT /api/customer/location DONE!!
        Task<bool> UpdateMyLocationAsync(UpdateOrganizationLocationDto dto);

        // GET /api/customer/products?search=rice&pageNumber=1&pageSize=10 DONE!!
        Task<IReadOnlyList<OrganizationProductDto>> GetProductsAsync(OrganizationQueryProductDto query);

        // GET /api/customer/products/{id} DONE!!
        Task<OrganizationProductDto?> GetProductAsync(int id);

        // GET /api/customer/restaurants DONE!!
        Task<IReadOnlyList<OrganizationRestaurantDto>> GetRestaurantsAsync();

        // GET /api/customer/restaurants/{id}
        Task<OrganizationRestaurantDetailsDto?> GetRestaurantAsync(int id);
    }
}
