using Microsoft.EntityFrameworkCore;
using ProjectSWD.Data;
using ProjectSWD.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectSWD.Services.Staff
{
    public class ProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Unit)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Unit)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task CreateAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task<(bool Succeeded, string? ErrorMessage)> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return (false, "Không tìm thấy sản phẩm.");
            }

            // Check if product exists in OrderItems (historical orders)
            var hasOrders = await _context.OrderItems.AnyAsync(oi => oi.ProductId == id);
            if (hasOrders)
            {
                return (false, "Không thể xóa sản phẩm này vì đã tồn tại trong lịch sử đơn hàng.");
            }

            // Check if product exists in RefundItems
            var hasRefunds = await _context.RefundItems.AnyAsync(ri => ri.ProductId == id);
            if (hasRefunds)
            {
                return (false, "Không thể xóa sản phẩm này vì sản phẩm đang liên quan đến yêu cầu hoàn tiền.");
            }

            // Clean up related CartItems and PromotionProducts first to avoid foreign key violations
            var relatedCarts = await _context.CartItems.Where(ci => ci.ProductId == id).ToListAsync();
            if (relatedCarts.Any())
            {
                _context.CartItems.RemoveRange(relatedCarts);
            }

            var relatedPromotions = await _context.PromotionProducts.Where(pp => pp.ProductId == id).ToListAsync();
            if (relatedPromotions.Any())
            {
                _context.PromotionProducts.RemoveRange(relatedPromotions);
            }

            try
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi hệ thống khi xóa sản phẩm: {ex.Message}");
            }
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<List<Unit>> GetUnitsAsync()
        {
            return await _context.Units.OrderBy(u => u.Name).ToListAsync();
        }
    }
}
