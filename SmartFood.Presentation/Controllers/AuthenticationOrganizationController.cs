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

        // ==========================================
        // REGISTER ORGANIZATION
        // ==========================================

        [HttpPost("register")]
        public async Task<IActionResult> RegisterOrganization(
            [FromBody] OrganizationRegisterDto dto)
        {
            await _organizationService.RegisterNewOrgnaizationAsync(dto);

            return Ok(new
            {
                Message = "Organization registered successfully."
            });
        }

        // ==========================================
        // LOGIN: EMAIL + PASSWORD -> SEND OTP
        // ==========================================

        [HttpPost("login")]
        public async Task<IActionResult> LoginOrganization(
            [FromBody] OrganizationLoginDto dto)
        {
            await _organizationService.OrganizationLoginAsync(dto);

            return Ok(new
            {
                Message = "Verification code sent to your email."
            });
        }

        // ==========================================
        // VERIFY LOGIN OTP -> GET TOKENS
        // ==========================================

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOrganizationOtp(
            [FromBody] OrganizationVerifyCodeOtp dto)
        {
            var result =
                await _organizationService.OrganizationVerifyOTPAsync(dto);

            return Ok(result);
        }

        // ==========================================
        // FORGOT PASSWORD: EMAIL -> SEND OTP
        // ==========================================

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotOrganizationPassword(
            [FromBody] OrganizationSendOTPDto dto)
        {
            await _organizationService.OrganizationForgotPasswordAsync(dto);

            return Ok(new
            {
                Message = "Password reset verification code sent to your email."
            });
        }

        // ==========================================
        // RESET PASSWORD: EMAIL + OTP + NEW PASSWORD
        // ==========================================

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetOrganizationPassword(
            [FromBody] OrganizationChangePasswordDto dto)
        {
            var result =
                await _organizationService.OrganizationResetPasswordAsync(dto);

            return Ok(new
            {
                Message = result
            });
        }

        // ==========================================
        // REFRESH TOKEN
        // ==========================================

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshOrganizationToken(
            [FromBody] OrganizationRefreshTokenDto dto)
        {
            var result =
                await _organizationService.OrganizationRefreshTokenAsync(dto);

            return Ok(result);
        }

        // ==========================================
        // LOGOUT
        // ==========================================

        [HttpPost("logout")]
        public async Task<IActionResult> LogoutOrganization(
            [FromBody] OrganizationLogoutDto dto)
        {
            await _organizationService.OragnizationLogoutAsync(dto);

            return Ok(new
            {
                Message = "Logged out successfully."
            });
        }
    }
}