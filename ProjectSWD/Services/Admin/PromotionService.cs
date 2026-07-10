using Microsoft.EntityFrameworkCore;
using ProjectSWD.Data;
using ProjectSWD.Data.Entities;

namespace ProjectSWD.Services.Admin;

public class PromotionService
{
    private readonly ApplicationDbContext _context;

    public PromotionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Promotion>> GetAllAsync()
    {
        return await _context.Promotions
            .Include(p => p.PromotionProducts)
            .ThenInclude(pp => pp.Product)
            .OrderByDescending(p => p.StartTime)
            .ToListAsync();
    }

    public async Task<Promotion?> GetByIdAsync(int id)
    {
        return await _context.Promotions
            .Include(p => p.PromotionProducts)
            .ThenInclude(pp => pp.Product)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task CreateAsync(Promotion promotion)
    {
        _context.Promotions.Add(promotion);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Promotion promotion)
    {
        _context.Promotions.Update(promotion);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var promotion = await _context.Promotions.FindAsync(id);
        if (promotion != null)
        {
            _context.Promotions.Remove(promotion);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task UpdatePromotionProductsAsync(int promotionId, List<int> productIds)
    {
        var existing = await _context.PromotionProducts
            .Where(pp => pp.PromotionId == promotionId)
            .ToListAsync();

        _context.PromotionProducts.RemoveRange(existing);

        foreach (var productId in productIds)
        {
            _context.PromotionProducts.Add(new PromotionProduct
            {
                PromotionId = promotionId,
                ProductId = productId
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Promotions.AnyAsync(p => p.Id == id);
    }
}
