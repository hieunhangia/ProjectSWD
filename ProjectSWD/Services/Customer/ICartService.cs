using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectSWD.DTOs;

namespace ProjectSWD.Services.Customer
{
    public interface ICartService
    {
        Task<List<CartItemDTO>> GetCartByCustomerIdAsync(string customerId);
        Task AddToCartAsync(string customerId, int productId, decimal quantity);
        Task UpdateQuantityAsync(string customerId, int productId, decimal quantity);
        Task RemoveFromCartAsync(string customerId, int productId);
        Task ClearCartAsync(string customerId);
    }
}
