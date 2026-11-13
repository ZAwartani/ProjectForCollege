
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFood.Application.DTOs;
using SmartFood.Application.Interfaces;

namespace SmartFood.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // ================= AUTHENTICATION =================
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAdmin(
    [FromBody] RegisterAdminDto dto)
        {
            var result = await _adminService.RegisterAdminAsync(dto);

            if (!result)
                return Conflict("Admin email already exists.");

            return Ok("Admin registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginAdminDto dto)
        {
            var result = await _adminService.LoginAdmin(dto);

            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(
            [FromBody] AdminRefreshTokenDto dto)
        {
            var result = await _adminService.RefreshTokenAsync(dto);

            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Logout(
            [FromBody] AdminRefreshTokenDto dto)
        {
            await _adminService.LogoutAsync(dto);

            return Ok(new
            {
                Message = "Logged out successfully."
            });
        }

        // ================= ORGANIZATIONS =================

        [HttpGet("organizations/pending")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> GetPendingOrganizations()
        {
            var result = await _adminService.OrganizationDetail();

            return Ok(result);
        }

        [HttpGet("organizations")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> GetAllOrganizations()
        {
            var result = await _adminService.ListOrganizationAsync();

            return Ok(result);
        }

        [HttpPut("organizations/approve")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> ApproveOrganization(
            [FromBody] UpdateOrganizationNotApprovedDto dto)
        {
            var result = await _adminService.OrganizationUpdateData(dto);

            if (!result)
                return NotFound("Organization not found or is not pending.");

            return Ok("Organization approved successfully.");
        }

        [HttpDelete("organizations/reject")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> RejectOrganization(
            [FromBody] RemoveOrganizationDto dto)
        {
            var result = await _adminService.RemoveOrganization(dto);

            if (!result)
                return NotFound("Organization not found or is not pending.");

            return Ok("Organization rejected and deleted successfully.");
        }

        // ================= RESTAURANTS =================

        [HttpGet("restaurants/pending")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> GetPendingRestaurants()
        {
            var result = await _adminService.RestaurantDetail();

            return Ok(result);
        }

        [HttpGet("restaurants")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> GetAllRestaurants()
        {
            var result = await _adminService.ListRestaurantAsync();

            return Ok(result);
        }

        [HttpPut("restaurants/approve")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> ApproveRestaurant(
            [FromBody] UpdateRestaurantNotApprovedDto dto)
        {
            var result = await _adminService.RestaurantUpdateData(dto);

            if (!result)
                return NotFound("Restaurant not found or is not pending.");

            return Ok("Restaurant approved successfully.");
        }

        [HttpDelete("restaurants/reject")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> RejectRestaurant(
            [FromBody] RemoveRestaurantDto dto)
        {
            var result = await _adminService.RemoveRestaurant(dto);

            if (!result)
                return NotFound("Restaurant not found or is not pending.");

            return Ok("Restaurant rejected and deleted successfully.");
        }
    }
}
