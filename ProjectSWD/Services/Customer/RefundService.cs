using Microsoft.EntityFrameworkCore;
using ProjectSWD.Data;
using ProjectSWD.Data.Entities;
using ProjectSWD.Data.Enums;

namespace ProjectSWD.Services.Customer
{
    public class RefundService(ApplicationDbContext context)
    {
        public async Task<string?> ValidateRefundEligibilityAsync(string customerId, int orderId, int productId)
        {
            var order = await context.Orders
                .Include(o => o.OrderItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == customerId);

            if (order == null) return "Order not found or access denied.";
            if (order.ApprovementStatus != OrderStatus.Delivered) return "Only delivered orders are eligible for a refund.";
            
            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.ProductId == productId);
            if (orderItem == null) return "Product does not belong to this order.";

            var existingRefund = await context.Refunds
                .Include(r => r.RefundItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.OrderId == orderId && r.RefundItems.Any(ri => ri.ProductId == productId));
            
            if (existingRefund != null) return "You have already submitted a refund request for this product.";

            return null; 
        }

        public async Task<(string Name, string ImageUrl, decimal MaxQuantity, decimal Price)?> GetProductInfoForRefundAsync(string customerId, int orderId, int productId)
        {
            var orderItem = await context.OrderItems
                .Include(oi => oi.Product)
                .Include(oi => oi.Order)
                .AsNoTracking()
                .FirstOrDefaultAsync(oi => oi.OrderId == orderId && oi.ProductId == productId && oi.Order.CustomerId == customerId);

            if (orderItem?.Product == null) return null;

            return (orderItem.Product.Name, orderItem.Product.ImageUrl, orderItem.Quantity, orderItem.Price);
        }

        public async Task CreateRefundRequestAsync(int orderId, string customerId, string reason, int productId, decimal quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0.");

            var order = await context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == customerId);

            if (order == null)
                throw new InvalidOperationException("Order not found or access denied.");
            
            if (order.ApprovementStatus != OrderStatus.Delivered)
                throw new InvalidOperationException("Only delivered orders are eligible for a refund.");

            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.ProductId == productId);
            if (orderItem == null)
                throw new InvalidOperationException("Product does not belong to this order.");

            if (quantity > orderItem.Quantity)
                throw new InvalidOperationException($"Quantity cannot exceed {orderItem.Quantity}.");

            var existingRefund = await context.Refunds
                .Include(r => r.RefundItems)
                .FirstOrDefaultAsync(r => r.OrderId == orderId && r.RefundItems.Any(ri => ri.ProductId == productId));

            if (existingRefund != null)
                throw new InvalidOperationException("You have already submitted a refund request for this product.");

            var refundItem = new RefundItem
            {
                ProductId = productId,
                Quantity = quantity,
                Price = orderItem.Price 
            };

            var refund = new Refund
            {
                OrderId = orderId,
                Amount = refundItem.Quantity * refundItem.Price,
                Reason = reason,
                Status = RefundStatus.PendingReview,
                CreatedAt = DateTime.UtcNow,
                RefundItems = new List<RefundItem> { refundItem }
            };

            context.Refunds.Add(refund);
            await context.SaveChangesAsync();
        }
    }
}
