using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartFood.Application.DTOs;
using SmartFood.Application.Interfaces;

namespace SmartFood.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Organization")]
    [Authorize(Policy = "OrganizationApproved")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _CartService;

        public CartController(ICartService cartService)
        {
            _CartService = cartService;
        }
        [HttpGet]
        public async Task<IActionResult> GetMyCart()
        {
            var cart = await _CartService.GetMyCartAsync();

            return Ok(cart);
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItem(
            [FromBody] AddCartItemDto dto)
        {
            try
            {
                var result = await _CartService.AddItemAsync(dto);

                if (!result)
                    return NotFound("Product not found or unavailable.");

                return Ok("Product added to cart successfully.");
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
        }

        [HttpPut("items/{productId:int}")]
        public async Task<IActionResult> UpdateItemQuantity(
            [FromRoute] int productId,
            [FromBody] UpdateCartItemQuantityDto dto)
        {
            try
            {
                var result = await _CartService.UpdateItemQuantityAsync(
                    productId,
                    dto);

                if (!result)
                    return NotFound("Product not found in your cart.");

                return Ok("Cart item quantity updated successfully.");
            }
            catch (ArgumentException exception)
            {
                return BadRequest(exception.Message);
            }
        }

        [HttpDelete("items/{productId:int}")]
        public async Task<IActionResult> RemoveItem(
            [FromRoute] int productId)
        {
            var result = await _CartService.RemoveItemAsync(productId);

            if (!result)
                return NotFound("Product not found in your cart.");

            return Ok("Product removed from cart successfully.");
        }

        [HttpDelete]
        public async Task<IActionResult> ClearMyCart()
        {
            await _CartService.ClearMyCartAsync();

            return Ok("Cart cleared successfully.");
        }
    }
}