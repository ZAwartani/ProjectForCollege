using Microsoft.AspNetCore.Mvc;
using SmartFood.Application.DTOs;
using SmartFood.Application.Interfaces;

namespace SmartFood.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationOrganizationController : ControllerBase
    {
        private readonly IAuthenticationServiceOrganization _organizationService;

        public AuthenticationOrganizationController(
            IAuthenticationServiceOrganization organizationService)
        {
            _organizationService = organizationService;
        }

        // Register
        [HttpPost("organization/register")]
        public async Task<IActionResult> RegisterOrganization(
            OrganizationRegisterDto dto)
        {
            await _organizationService.RegisterNewOrgnaizationAsync(dto);

            return Ok("Organization registered successfully.");
        }

        // Login and send OTP
        [HttpPost("organization/login")]
        public async Task<IActionResult> LoginOrganization(
            OrganizationLoginDto dto)
        {
            await _organizationService.OrganizationLoginAsync(dto);

            return Ok("Verification code sent to your email.");
        }

        // Verify OTP and return tokens
        [HttpPost("organization/verify-otp")]
        public async Task<IActionResult> VerifyOrganizationOtp(
            OrganizationVerifyCodeOtp dto)
        {
            var result =
                await _organizationService.OrganizationVerifyOTPAsync(dto);

            return Ok(result);
        }

        // Refresh tokens
        [HttpPost("organization/refresh-token")]
        public async Task<IActionResult> RefreshOrganizationToken(
            OrganizationRefreshTokenDto dto)
        {
            var result =
                await _organizationService.OrganizationRefreshTokenAsync(dto);

            return Ok(result);
        }

        // Reset password
        [HttpPost("organization/reset-password")]
        public async Task<IActionResult> ResetOrganizationPassword(
            OrganizationChangePasswordDto dto)
        {
            var result =
                await _organizationService.OrganizationChangePasswordAsync(dto);

            return Ok(result);
        }

        // Logout
        [HttpPost("organization/logout")]
        public async Task<IActionResult> LogoutOrganization(
            OrganizationLogoutDto dto)
        {
            await _organizationService.OragnizationLogoutAsync(dto);

            return Ok("Logged out successfully.");
        }
    }
}