using Microsoft.EntityFrameworkCore;
using ProjectSWD.Data;
using ProjectSWD.Data.Entities;
using ProjectSWD.Data.Enums;
using ProjectSWD.DTOs.Refund;

namespace ProjectSWD.Services.Customer
{
    public class RefundService(ApplicationDbContext context)
    {
        public async Task<ValidateRefundEligibilityResponseDTO> ValidateRefundEligibilityAsync(string customerId, int orderId, int productId)
        {
            var order = await context.Orders
                .Include(o => o.OrderItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == customerId);

            if (order == null) return new ValidateRefundEligibilityResponseDTO { IsEligible = false, ErrorMessage = "Không tìm thấy đơn hàng hoặc quyền truy cập bị từ chối." };
            if (order.ApprovementStatus != OrderStatus.Delivered) return new ValidateRefundEligibilityResponseDTO { IsEligible = false, ErrorMessage = "Chỉ những đơn hàng đã giao mới đủ điều kiện được hoàn tiền." };
            
            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.ProductId == productId);
            if (orderItem == null) return new ValidateRefundEligibilityResponseDTO { IsEligible = false, ErrorMessage = "Sản phẩm không thuộc về đơn hàng này." };

            var existingRefund = await context.Refunds
                .Include(r => r.RefundItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.OrderId == orderId && r.RefundItems.Any(ri => ri.ProductId == productId));
            
            if (existingRefund != null) return new ValidateRefundEligibilityResponseDTO { IsEligible = false, ErrorMessage = "Bạn đã gửi yêu cầu hoàn tiền cho sản phẩm này rồi." };

            return new ValidateRefundEligibilityResponseDTO { IsEligible = true, ErrorMessage = null }; 
        }

        public async Task<ProductRefundInfoResponseDTO?> GetProductInfoForRefundAsync(string customerId, int orderId, int productId)
        {
            var orderItem = await context.OrderItems
                .Include(oi => oi.Product)
                .Include(oi => oi.Order)
                .AsNoTracking()
                .FirstOrDefaultAsync(oi => oi.OrderId == orderId && oi.ProductId == productId && oi.Order.CustomerId == customerId);

            if (orderItem?.Product == null) return null;

            return new ProductRefundInfoResponseDTO
            {
                Name = orderItem.Product.Name,
                ImageUrl = orderItem.Product.ImageUrl,
                MaxQuantity = orderItem.Quantity,
                Price = orderItem.Price
            };
        }

        public async Task CreateRefundRequestAsync(CreateRefundRequestDTO request)
        {
            if (request.Quantity <= 0)
                throw new ArgumentException("Số lượng phải lớn hơn 0.");

            var order = await context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.CustomerId == request.CustomerId);

            if (order == null)
                throw new InvalidOperationException("Không tìm thấy đơn hàng hoặc quyền truy cập bị từ chối.");
            
            if (order.ApprovementStatus != OrderStatus.Delivered)
                throw new InvalidOperationException("Chỉ những đơn hàng đã giao mới đủ điều kiện được hoàn tiền.");

            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.ProductId == request.ProductId);
            if (orderItem == null)
                throw new InvalidOperationException("Sản phẩm không thuộc về đơn hàng này.");

            if (request.Quantity > orderItem.Quantity)
                throw new InvalidOperationException($"Số lượng không thể vượt quá {orderItem.Quantity}.");

            var existingRefund = await context.Refunds
                .Include(r => r.RefundItems)
                .FirstOrDefaultAsync(r => r.OrderId == request.OrderId && r.RefundItems.Any(ri => ri.ProductId == request.ProductId));

            if (existingRefund != null)
                throw new InvalidOperationException("Bạn đã gửi yêu cầu hoàn tiền cho sản phẩm này rồi.");

            var refundItem = new RefundItem
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                Price = orderItem.Price 
            };

            var refund = new Refund
            {
                OrderId = request.OrderId,
                Amount = refundItem.Quantity * refundItem.Price,
                Reason = request.Reason,
                Status = RefundStatus.PendingReview,
                CreatedAt = DateTime.UtcNow,
                RefundItems = new List<RefundItem> { refundItem }
            };

            context.Refunds.Add(refund);
            await context.SaveChangesAsync();
        }
    }
}
