using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectSWD.Data.Entities;

namespace ProjectSWD.Repositories
{
    public interface ICartRepository
    {
        Task<List<CartItem>> GetCartItemsByCustomerIdAsync(string customerId);
        Task<CartItem?> GetCartItemAsync(string customerId, int productId);
        Task AddCartItemAsync(CartItem cartItem);
        Task UpdateCartItemAsync(CartItem cartItem);
        Task DeleteCartItemAsync(CartItem cartItem);
        Task ClearCartAsync(string customerId);
        Task SaveChangesAsync();
    }
}
