namespace SmartFood.Application.DTOs
{
    public record RestaurantRegisterDto(
            int VerificationDocumentId,
            string Email,
            string Password,
            string RestaurantName,
            string OwnerName,
            string BusinessRegistrationNumber,
            string Address
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

