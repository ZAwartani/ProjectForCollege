
using SmartFood.Application.DTOs;

namespace SmartFood.Application.Interfaces
{
    public interface IAuthenticationServiceOrganization
    {
        Task RegisterNewOrgnaizationAsync(
            OrganizationRegisterDto dto);

        Task OrganizationLoginAsync(
            OrganizationLoginDto LoginDto);

        Task<RestaurantTokenResponceDto> OrganizationVerifyOTPAsync(
            OrganizationVerifyCodeOtp VerifyCodedto);

        Task OragnizationLogoutAsync(
            OrganizationLogoutDto LogoutDto);

        Task OrganizationForgotPasswordAsync(
            OrganizationSendOTPDto dto);

        Task<string?> OrganizationResetPasswordAsync(
            OrganizationChangePasswordDto dto);

        Task<RestaurantTokenResponceDto> OrganizationRefreshTokenAsync(
            OrganizationRefreshTokenDto RefreshToken);
    }
}
