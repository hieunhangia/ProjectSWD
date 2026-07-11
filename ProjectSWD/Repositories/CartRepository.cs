using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectSWD.Data;
using ProjectSWD.Data.Entities;

namespace ProjectSWD.Repositories
{
    public class CartRepository(ApplicationDbContext context) : ICartRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<List<CartItem>> GetCartItemsByCustomerIdAsync(string customerId)
        {
            return await _context.CartItems
                .Where(c => c.CustomerId == customerId)
                .Include(c => c.Product)
                .ToListAsync();
        }

        public async Task<CartItem?> GetCartItemAsync(string customerId, int productId)
        {
            return await _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.ProductId == productId);
        }

        public async Task AddCartItemAsync(CartItem cartItem)
        {
            await _context.CartItems.AddAsync(cartItem);
        }

        public Task UpdateCartItemAsync(CartItem cartItem)
        {
            _context.CartItems.Update(cartItem);
            return Task.CompletedTask;
        }

        public Task DeleteCartItemAsync(CartItem cartItem)
        {
            _context.CartItems.Remove(cartItem);
            return Task.CompletedTask;
        }

        public async Task ClearCartAsync(string customerId)
        {
            var cartItems = await _context.CartItems
                .Where(c => c.CustomerId == customerId)
                .ToListAsync();
            
            if (cartItems.Count > 0)
            {
                _context.CartItems.RemoveRange(cartItems);
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
