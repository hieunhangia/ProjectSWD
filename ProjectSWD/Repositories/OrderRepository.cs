using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectSWD.Data;
using ProjectSWD.Data.Entities;

namespace ProjectSWD.Repositories
{
    public class OrderRepository(ApplicationDbContext context) : IOrderRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task AddOrderAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
        }

        public async Task AddOrderItemAsync(OrderItem orderItem)
        {
            await _context.OrderItems.AddAsync(orderItem);
        }

        public Task UpdateOrderAsync(Order order)
        {
            _context.Orders.Update(order);
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
