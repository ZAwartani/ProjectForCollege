using SmartFood.Application.DTOs;

namespace SmartFood.Application.Interfaces
{
    public interface IAuthenticationServiceRestaurant
    {
        Task RegisterNewRestaurantAsync(RestaurantRegisterDto dto);

        Task RestaurantLoginAsync(RestaurantLoginDto dto);

        Task<RestaurantTokenResponceDto> RestaurantVerifyOTPAsync(
            RestaurantVerifyOTPDto dto);

        Task RestaurantForgotPasswordAsync(RestaurantSendOTPDto dto);

        Task<string?> RestaurantChangePasswordAsync(
            RestaurantChangePasswordDto dto);

        Task<RestaurantTokenResponceDto> RestaurantRefreshTokenAsync(
            RestaurantRefreshTokenDto dto);

        Task RestaurantLogoutAsync(RestaurantLogoutDto dto);

    }
}
