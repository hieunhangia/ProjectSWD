using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectSWD.Data;
using ProjectSWD.Data.Entities;
using ProjectSWD.DTOs.Order;

namespace ProjectSWD.Services.Customer
{
    public class CartService(ApplicationDbContext context)
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<List<CartItemDTO>> GetCartByCustomerIdAsync(string customerId)
        {
            var items = await _context.CartItems
                .Where(c => c.CustomerId == customerId)
                .Include(c => c.Product)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return items.Select(item => new CartItemDTO
            {
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? "Unknown Product",
                ProductImage = string.IsNullOrEmpty(item.Product?.ImageUrl) ? "/images/products/placeholder.jpg" : item.Product.ImageUrl,
                Price = item.Product?.Price ?? 0,
                Quantity = item.Quantity,
                AvailableQuantity = item.Product?.Quantity ?? 0
            }).ToList();
        }

        public async Task AddToCartAsync(string customerId, int productId, decimal quantity)
        {
            if (quantity <= 0)
            {
                throw new ArgumentException("Số lượng phải lớn hơn 0.");
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                throw new KeyNotFoundException("Sản phẩm không tồn tại.");
            }

            var existingItem = await _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.ProductId == productId);
            decimal newQuantity = quantity;

            if (existingItem != null)
            {
                newQuantity += existingItem.Quantity;
            }

            if (newQuantity > product.Quantity)
            {
                throw new InvalidOperationException($"Không thể thêm sản phẩm. Số lượng yêu cầu ({newQuantity:N0}) vượt quá tồn kho hiện có ({product.Quantity:N0}).");
            }

            if (existingItem != null)
            {
                existingItem.Quantity = newQuantity;
                existingItem.CreatedAt = DateTime.Now;
                _context.CartItems.Update(existingItem);
            }
            else
            {
                var newItem = new CartItem
                {
                    CustomerId = customerId,
                    ProductId = productId,
                    Quantity = quantity
                };
                await _context.CartItems.AddAsync(newItem);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateQuantityAsync(string customerId, int productId, decimal quantity)
        {
            if (quantity < 0)
            {
                throw new ArgumentException("Số lượng không được âm.");
            }

            var existingItem = await _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.ProductId == productId);
            if (existingItem == null)
            {
                throw new KeyNotFoundException("Sản phẩm chưa có trong giỏ hàng.");
            }

            if (quantity == 0)
            {
                _context.CartItems.Remove(existingItem);
            }
            else
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    throw new KeyNotFoundException("Sản phẩm không tồn tại.");
                }

                if (quantity > product.Quantity)
                {
                    throw new InvalidOperationException($"Số lượng cập nhật ({quantity:N0}) vượt quá tồn kho hiện có ({product.Quantity:N0}).");
                }

                existingItem.Quantity = quantity;
                _context.CartItems.Update(existingItem);
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveFromCartAsync(string customerId, int productId)
        {
            var existingItem = await _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.ProductId == productId);
            if (existingItem != null)
            {
                _context.CartItems.Remove(existingItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(string customerId)
        {
            var cartItems = await _context.CartItems
                .Where(c => c.CustomerId == customerId)
                .ToListAsync();
            if (cartItems.Count > 0)
            {
                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();
            }
        }
    }
}
