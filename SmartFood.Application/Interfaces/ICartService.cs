using SmartFood.Application.DTOs;

namespace SmartFood.Application.Interfaces
{
    public interface ICartService
    {
        // GET /api/Cart
        Task<CartDto> GetMyCartAsync();

        // POST /api/Cart/items DONE!!
        Task<bool> AddItemAsync(AddCartItemDto dto);

        // PUT /api/Cart/items/{productId}
        Task<bool> UpdateItemQuantityAsync(int productId, UpdateCartItemQuantityDto dto);

        // DELETE /api/Cart/items/{productId}
        Task<bool> RemoveItemAsync(int productId);

        // DELETE /api/Cart
        Task ClearMyCartAsync();
    }
}
