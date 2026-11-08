using SmartFood.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace SmartFood.Application.Interfaces
{
    public interface IAdminService
    {
        Task<IReadOnlyList<ListOrganizationDto>> ListOrganizationAsync();
        Task<IReadOnlyList<ListRestaurantDto>> ListRestaurantAsync();
        Task<IReadOnlyList<ReturnOrganizationDto>> OrganizationDetail();
        Task<IReadOnlyList<ReturnRestaurantDto>> RestaurantDetail();
        Task<AdminResponceTokenDto> LoginAdmin(LoginAdminDto loginData);
        Task<bool> RemoveOrganization(RemoveOrganizationDto removeDataOrg);
        Task<bool> RemoveRestaurant(RemoveRestaurantDto removeDataRes);
        Task<bool> OrganizationUpdateData(UpdateOrganizationNotApprovedDto UpdateOrganizaiton);
        Task<bool> RestaurantUpdateData(UpdateRestaurantNotApprovedDto UpdateRestaurant);
        Task<AdminResponceTokenDto> RefreshTokenAsync(AdminRefreshTokenDto dto);
        Task LogoutAsync(AdminRefreshTokenDto dto);
        Task<bool> RegisterAdminAsync(RegisterAdminDto dto);
    }
}
