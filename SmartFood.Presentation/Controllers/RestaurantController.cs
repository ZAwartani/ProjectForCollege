using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFood.Application.DTOs;
using SmartFood.Application.Interfaces;

namespace SmartFood.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Restaurant")]
    public class RestaurantController : ControllerBase
    {
        private readonly IRestaurantService _RestaurantService;
        public RestaurantController(IRestaurantService restaurantService)
        {
            this._RestaurantService = restaurantService;
        }

        [HttpPost("CreateProduct")]
        [Authorize(Policy = "Approved")]
        public async Task<IActionResult> CreateProduct(
    [FromForm] CreateProducrDto dto)
        {
            try
            {
                var result = await _RestaurantService.CreateProduct(dto);

                if (!result)
                    return NotFound("Restaurant not found.");

                return Ok("Product created successfully.");
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
        }

        [HttpGet("GetProduct")]
        [Authorize(Policy = "Approved")]

        public async Task<IActionResult> GetProduct()
        {
            var result = await _RestaurantService.ProductList();
            return Ok(result);
        }

        [HttpPut("UpdateProduct/{Id:int}")]
        [Authorize(Policy = "Approved")]
        public async Task<IActionResult> UpdateProduct(
    [FromForm] UpdateProductDto dto,
    [FromRoute] int Id)
        {
            try
            {
                var result = await _RestaurantService.UpdateProduct(dto, Id);

                if (!result)
                    return NotFound("Product not found.");

                return Ok("Product updated successfully.");
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
        }

        [HttpDelete("DeleteProduct/{Id:int}")]
        [Authorize(Policy = "Approved")]
        public async Task<IActionResult> DeleteProduct([FromRoute] int Id)
        {
            var result = await _RestaurantService.RemoveProduct(Id);

            if (!result)
                return NotFound("Product not found.");

            return Ok("Product and its photo deleted successfully.");
        }

        // 5. View profile and approval status
        [HttpGet("MyProfile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var result = await _RestaurantService.GetMyProfile();

            if (result == null)
                return NotFound("Restaurant not found.");

            return Ok(result);
        }


        [HttpPut("UpdateMyProfile")]
        public async Task<IActionResult> UpdateMyProfile(UpdateRestaurantProfileDto dto)
        {
            try
            {
                var result = await _RestaurantService.UpdateMyProfile(dto);

                if (!result)
                    return NotFound("Restaurant not found.");

                return Ok("Profile updated successfully.");
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
        }


        [HttpPut("UpdateMyLocation")]
        public async Task<IActionResult> UpdateMyLocation(
            UpdateRestaurantLocationDto dto)
        {
            var result = await _RestaurantService.UpdateMyLocation(dto);

            if (!result)
                return BadRequest("Invalid location or restaurant not found.");

            return Ok("Location updated successfully.");
        }
    }
}
