using SmartFood.Core.Enums;

namespace SmartFood.Application.DTOs
{
    public record OrganizationRegisterDto(
            string Email,
            string Password,
            string OrganizationName,
            string OwnerName,
            string RegistrationNumber,
            string Address
    );

    public record ReturnOrganizationDto(
        int Id,
        string OrganizationName,
        string OwnerName,
        string RegisterNumber,
        string Address,
        RegisterStatus Status,
        VerificationDocumentDto? VerificationDocument
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
    public record OrganizationProfileDto(
    int Id,
    string OrganizationName,
    string OwnerName,
    string Email,
    string RegistrationNumber,
    string Address,
    double? Latitude,
    double? Longitude,
    RegisterStatus Status,
    string? PhoneNumber
);

    public record UpdateOrganizationProfileDto(
    string? OwnerName,
    string? Address,
    string? PhoneNumber
);

    public record UpdateOrganizationLocationDto(
        double Latitude,
        double Longitude
     );

    public class OrganizationQueryProductDto
    {
        public string? Search { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public record OrganizationProductDto(
       int Id,
       int RestaurantId,
       string RestaurantName,
       string Name,
       string Description,
       int Quantity,
       string PhotoPath,
       DateTime CreateDate
    );

    public record OrganizationRestaurantDto(
    int Id,
    string RestaurantName,
    string Address,
    double Latitude,
    double Longitude
);

    public record OrganizationRestaurantDetailsDto(
      int Id,
      string RestaurantName,
      string Address,
      double Latitude,
      double Longitude,
      IReadOnlyList<OrganizationProductDto> Products
  );
}
