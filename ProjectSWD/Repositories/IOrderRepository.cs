using System.Threading.Tasks;
using ProjectSWD.Data.Entities;

namespace ProjectSWD.Repositories
{
    public interface IOrderRepository
    {
        Task AddOrderAsync(Order order);
        Task AddOrderItemAsync(OrderItem orderItem);
        Task UpdateOrderAsync(Order order);
        Task ClearCartAsync(string customerId);
        Task SaveChangesAsync();
    }
}
