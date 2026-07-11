using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectSWD.Data.Entities;
using ProjectSWD.DTOs;

namespace ProjectSWD.Services.Customer
{
    public interface IOrderService
    {
        Task<decimal> CalculateShippingFeeAsync(string address);
        Task<OrderSummaryDTO> ProcessOrderCheckoutAsync(string userId, OrderCheckoutDTO checkoutInfo);
        Task<Product?> GetProductByIdAsync(int productId);
    }
}
