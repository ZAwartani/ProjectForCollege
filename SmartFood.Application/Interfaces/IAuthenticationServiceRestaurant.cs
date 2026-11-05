using SmartFood.Application.DTOs;

namespace SmartFood.Application.Interfaces
{
    public interface IAuthenticationServiceRestaurant
    {

        Task RegisterNewRestaurantAsync(RestaurantRegisterDto dto);
        Task RestaurantLoginAsync(RestaurantLoginDto dto);
        Task<RestaurantTokenResponceDto> RestaurantVerifyOTPAsync(
            RestaurantVerifyOTPDto dto);
        Task RestaurantLogoutAsync(RestaurantLogoutDto LogoutDto);
        Task<string?> RestaurantChangePasswordAsync(RestaurantChangePasswordDto ChangePasswordDto);
        Task<RestaurantTokenResponceDto> RestaurantRefreshTokenAsync(RestaurantRefreshTokenDto RefreshToken);
        Task RestaurantSendOTPAsync(RestaurantSendOTPDto SendOTPDto);

    }
}
