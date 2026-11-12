using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SmartFood.Application.DTOs;
using SmartFood.Application.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SmartFood.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Organization")]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService OrganizationService;
        public OrganizationController(IOrganizationService organizationService)
        {
            this.OrganizationService = organizationService;
        }


        [HttpGet("GetMyProfile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var result = await OrganizationService.GetMyProfileAsync();
            if(result == null)
            {
                return NotFound("Organization Not Found");
            }
            return Ok(result);
        }

        [HttpPut("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateOrganizationProfileDto dto)
        {
            try
            {
                var result = await OrganizationService.UpdateMyProfileAsync(dto);

                if (!result)
                    return NotFound("Organization not found.");

                return Ok("Profile updated successfully.");
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
        }

        [HttpPut("UpdateLocation")]
        public async Task<IActionResult> UpdateLocation([FromBody] UpdateOrganizationLocationDto dto)
        {
            try
            {
                var result = await OrganizationService.UpdateMyLocationAsync(dto);

                if (!result)
                    return NotFound("Organization not found.");

                return Ok("Profile updated successfully.");
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
        }

        [HttpGet("GetProduct")]
        [Authorize(Policy = "OrganizationApproved")]
        public async Task<IActionResult> GetProduct([FromQuery] OrganizationQueryProductDto query)
        {
            try
            {
                var result = await OrganizationService.GetProductsAsync(query);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetProductAsync/{Id:int}")]
        [Authorize(Policy = "OrganizationApproved")]

        public async Task<IActionResult> GetProduct(int Id)
        {
            var result = await OrganizationService.GetProductAsync(Id);
                
            if (result == null)
                return NotFound("Product Not Found Or Unavailable");

            return Ok(result);
        }

        [HttpGet("GetRestaurant")]
        [Authorize(Policy = "OrganizationApproved")]
        public async Task<IActionResult> GetRestaurant()
        {
            var result = await OrganizationService.GetRestaurantsAsync();
            return Ok(result);
        }

        [HttpGet("GetRestaurantDetail/{Id:int}")]
        [Authorize(Policy = "OrganizationApproved")]
        public async Task<IActionResult> GetRestaurantDetail([FromRoute] int Id)
        {
            var result = await OrganizationService.GetRestaurantAsync(Id);

            if (result == null)
                return NotFound("Restaurant not found or unavailable.");

            return Ok(result);
        }
    }
}
