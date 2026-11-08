using SmartFood.Core.Enums;

namespace SmartFood.Application.DTOs
{
    public record RestaurantRegisterDto(
            string Email,
            string Password,
            string RestaurantName,
            string OwnerName,
            string BusinessRegistrationNumber,
            string Address
    );

    public record ReturnRestaurantDto(
            int Id,
            int VerificationId,
            string RestaurantName,
            string OwnerName,
            string RegisterNumber,
            string Address,
            RegisterStatus Status
    );

    public record RestaurantLoginDto(
            string Email,
            string Password
    );
    public record RestaurantVerifyOTPDto(
        string Email,
        string OTP
    );
    public record RestaurantSendOTPDto(
            string Email
    );

    public record RestaurantChangePasswordDto(
        string Email,
        string Password,
        string OTP
    );

    public record RestaurantLogoutDto(
        string Email,
        string RefreshToken
    );

    public record RestaurantTokenResponceDto(
        string AccessToken,
        string RefreshToken
    );

    public record RestaurantRefreshTokenDto(
        string Email,
        string RefreshToken
    );
}

