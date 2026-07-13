using Microsoft.EntityFrameworkCore;
using ProjectSWD.Data;
using ProjectSWD.Data.Entities;
using ProjectSWD.Data.Enums;

namespace ProjectSWD.Services.Customer
{
    public class FeedbackService(ApplicationDbContext context)
    {
        public async Task<(string Name, string ImageUrl)?> GetProductInfoForRatingAsync(int productId)
        {
            var product = await context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return null;
            return (product.Name, product.ImageUrl);
        }

        public async Task<string?> ValidateFeedbackEligibilityAsync(string customerId, int orderId, int productId)
        {
            var order = await context.Orders
                .Include(o => o.OrderItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == customerId);

            if (order == null) return "Không tìm thấy đơn hàng hoặc quyền truy cập bị từ chối.";
            if (order.ApprovementStatus != OrderStatus.Delivered && order.Status != "Delivered Successfully")
                return "Chỉ có thể gửi đánh giá cho những sản phẩm đã được giao hàng thành công.";

            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.ProductId == productId);
            if (orderItem == null) return "Không tìm thấy sản phẩm trong đơn hàng này.";

            var existingReview = await context.Reviews
                .AsNoTracking()
                .FirstOrDefaultAsync(r =>
                    r.CustomerId == customerId && r.OrderId == orderId && r.ProductId == productId);

            if (existingReview != null) return "Bạn chỉ có thể gửi một đánh giá cho mỗi sản phẩm đã mua trên mỗi đơn hàng.";

            return null; // Valid
        }

        // Hardcoded readonly list of profanities
        private readonly IReadOnlyList<string> _profanities = new List<string>
        {
            "badword1", "badword2", "idiot", "stupid"
        }.AsReadOnly();

        public async Task SubmitFeedbackAsync(string customerId, int orderId, int productId, int rating, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Review content cannot be empty.");
            }

            if (rating < 1 || rating > 5)
            {
                throw new ArgumentException("Rating must be between 1 and 5.");
            }

            // Check for profanities
            var lowerContent = content.ToLowerInvariant();
            if (_profanities.Any(lowerContent.Contains))
            {
                throw new InvalidOperationException(
                    "Đánh giá của bạn chứa ngôn từ không phù hợp. Vui lòng chỉnh sửa lại và thử lại.");
            }

            // Validate order status and customer ownership
            var order = await context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == customerId);

            if (order == null)
            {
                throw new InvalidOperationException("Không tìm thấy đơn hàng hoặc quyền truy cập bị từ chối.");
            }

            if (order.ApprovementStatus != OrderStatus.Delivered && order.Status != "Delivered Successfully")
            {
                throw new InvalidOperationException(
                    "Chỉ có thể gửi đánh giá cho những sản phẩm đã được giao hàng thành công.");
            }

            // Check if product is in the order
            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.ProductId == productId);
            if (orderItem == null)
            {
                throw new InvalidOperationException("Không tìm thấy sản phẩm trong đơn hàng này.");
            }

            // Check if review already exists
            var existingReview = await context.Reviews
                .FirstOrDefaultAsync(r =>
                    r.CustomerId == customerId && r.OrderId == orderId && r.ProductId == productId);

            if (existingReview != null)
            {
                throw new InvalidOperationException("Bạn chỉ có thể gửi một đánh giá cho mỗi sản phẩm đã mua trên mỗi đơn hàng.");
            }

            var review = new Review
            {
                CustomerId = customerId,
                OrderId = orderId,
                ProductId = productId,
                Rating = rating,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            context.Reviews.Add(review);
            await context.SaveChangesAsync();
        }
    }
}