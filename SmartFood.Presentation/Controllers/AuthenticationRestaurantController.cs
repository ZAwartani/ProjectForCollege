using Microsoft.AspNetCore.Mvc;
using SmartFood.Application.DTOs;
using SmartFood.Application.Interfaces;

namespace SmartFood.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationRestaurantController : ControllerBase
    {
        private readonly IAuthenticationServiceRestaurant _restaurantService;

        public AuthenticationRestaurantController(
            IAuthenticationServiceRestaurant restaurantService)
        {
            _restaurantService = restaurantService;
        }

        // Register
        [HttpPost("restaurant/register")]
        public async Task<IActionResult> RegisterRestaurant(
            RestaurantRegisterDto dto)
        {
            await _restaurantService.RegisterNewRestaurantAsync(dto);

            return Ok("Restaurant registered successfully.");
        }

        // Login and send OTP
        [HttpPost("restaurant/login")]
        public async Task<IActionResult> LoginRestaurant(
            RestaurantLoginDto dto)
        {
            await _restaurantService.RestaurantLoginAsync(dto);

            return Ok("Verification code sent to your email.");
        }

        // Verify OTP and return tokens
        [HttpPost("restaurant/verify-otp")]
        public async Task<IActionResult> VerifyRestaurantOtp(
            RestaurantVerifyOTPDto dto)
        {
            var result =
                await _restaurantService.RestaurantVerifyOTPAsync(dto);

            return Ok(result);
        }

        // Refresh tokens
        [HttpPost("restaurant/refresh-token")]
        public async Task<IActionResult> RefreshRestaurantToken(
            RestaurantRefreshTokenDto dto)
        {
            var result =
                await _restaurantService.RestaurantRefreshTokenAsync(dto);

            return Ok(result);
        }

        // Send OTP
        [HttpPost("restaurant/send-otp")]
        public async Task<IActionResult> SendRestaurantOtp(
            RestaurantSendOTPDto dto)
        {
            await _restaurantService.RestaurantSendOTPAsync(dto);

            return Ok("Verification code sent to your email.");
        }

        // Reset password
        [HttpPost("restaurant/reset-password")]
        public async Task<IActionResult> ResetRestaurantPassword(
            RestaurantChangePasswordDto dto)
        {
            var result =
                await _restaurantService.RestaurantChangePasswordAsync(dto);

            return Ok(result);
        }

        // Logout
        [HttpPost("restaurant/logout")]
        public async Task<IActionResult> LogoutRestaurant(
            RestaurantLogoutDto dto)
        {
            await _restaurantService.RestaurantLogoutAsync(dto);

            return Ok("Logged out successfully.");
        }
    }
}