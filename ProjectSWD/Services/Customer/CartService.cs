using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectSWD.Data;
using ProjectSWD.Data.Entities;
using ProjectSWD.DTOs;
using ProjectSWD.Repositories;

namespace ProjectSWD.Services.Customer
{
    public class CartService(ICartRepository cartRepository, ApplicationDbContext context) : ICartService
    {
        private readonly ICartRepository _cartRepository = cartRepository;
        private readonly ApplicationDbContext _context = context;

        public async Task<List<CartItemDTO>> GetCartByCustomerIdAsync(string customerId)
        {
            var items = await _cartRepository.GetCartItemsByCustomerIdAsync(customerId);
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

            var existingItem = await _cartRepository.GetCartItemAsync(customerId, productId);
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
                await _cartRepository.UpdateCartItemAsync(existingItem);
            }
            else
            {
                var newItem = new CartItem
                {
                    CustomerId = customerId,
                    ProductId = productId,
                    Quantity = quantity
                };
                await _cartRepository.AddCartItemAsync(newItem);
            }

            await _cartRepository.SaveChangesAsync();
        }

        public async Task UpdateQuantityAsync(string customerId, int productId, decimal quantity)
        {
            if (quantity < 0)
            {
                throw new ArgumentException("Số lượng không được âm.");
            }

            var existingItem = await _cartRepository.GetCartItemAsync(customerId, productId);
            if (existingItem == null)
            {
                throw new KeyNotFoundException("Sản phẩm chưa có trong giỏ hàng.");
            }

            if (quantity == 0)
            {
                await _cartRepository.DeleteCartItemAsync(existingItem);
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
                await _cartRepository.UpdateCartItemAsync(existingItem);
            }

            await _cartRepository.SaveChangesAsync();
        }

        public async Task RemoveFromCartAsync(string customerId, int productId)
        {
            var existingItem = await _cartRepository.GetCartItemAsync(customerId, productId);
            if (existingItem != null)
            {
                await _cartRepository.DeleteCartItemAsync(existingItem);
                await _cartRepository.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(string customerId)
        {
            await _cartRepository.ClearCartAsync(customerId);
            await _cartRepository.SaveChangesAsync();
        }
    }
}
