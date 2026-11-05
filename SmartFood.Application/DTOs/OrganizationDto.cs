namespace SmartFood.Application.DTOs
{
    public record OrganizationRegisterDto(
            int VerificationDocumentId,
            string Email,
            string Password,
            string OrganizationName,
            string OwnerName,
            string RegistrationNumber,
            string Address
    );

    public record OrganizationLoginDto(
            string Email,
            string Password
    );

    public record OrganizationVerifyCodeOtp(
        string Email,
        string CodeOTP
    );

    public record OrganizationSendOTPDto(
            string Email
    );

    public record OrganizationChangePasswordDto(
        string Email,
        string Password,
        string OTP
    );

    public record OrganizationLogoutDto(
        string Email,
        string RefreshToken
    );

    public record OrganizationTokenRespoceDto(
        string AccessToken,
        string RefreshToken
    );

    public record OrganizationRefreshTokenDto(
        string Email,
        string RefreshToken
    );
}
