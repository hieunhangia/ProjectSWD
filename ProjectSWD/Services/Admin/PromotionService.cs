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

    /// <summary>
    /// A2: Emergency Campaign Revocation — terminate an active campaign immediately.
    /// </summary>
    public async Task TerminateAsync(int id)
    {
        var promotion = await _context.Promotions.FindAsync(id);
        if (promotion != null)
        {
            promotion.IsTerminated = true;
            promotion.EndTime = DateTime.Now; // force end immediately
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// E1: Check if any active promotion overlaps with the given product set in the same timeframe.
    /// Returns conflict descriptions.
    /// </summary>
    public async Task<List<string>> CheckOverlappingConflictsAsync(
        List<int> productIds, DateTime startTime, DateTime endTime, int? excludePromotionId = null)
    {
        var conflicts = new List<string>();

        var activePromotions = _context.Promotions
            .Include(p => p.PromotionProducts)
            .Where(p => !p.IsTerminated
                        && p.StartTime < endTime
                        && p.EndTime > startTime);

        if (excludePromotionId.HasValue)
            activePromotions = activePromotions.Where(p => p.Id != excludePromotionId.Value);

        var activeList = await activePromotions.ToListAsync();

        foreach (var promo in activeList)
        {
            // Global scope overlaps with everything
            if (promo.Scope == PromotionScope.Global)
            {
                conflicts.Add($"Khuyến mãi \"{promo.Name}\" (Toàn bộ sản phẩm) đang hoạt động trong cùng khung thời gian.");
                continue;
            }

            // Specific SKU overlap
            if (promo.Scope == PromotionScope.SpecificSKU && productIds.Any())
            {
                var overlapProductIds = promo.PromotionProducts
                    .Select(pp => pp.ProductId)
                    .Intersect(productIds)
                    .ToList();

                if (overlapProductIds.Any())
                {
                    var overlapNames = await _context.Products
                        .Where(p => overlapProductIds.Contains(p.Id))
                        .Select(p => p.Name)
                        .ToListAsync();

                    conflicts.Add($"Khuyến mãi \"{promo.Name}\" áp dụng cho sản phẩm: {string.Join(", ", overlapNames)} — bị trùng lịch.");
                }
            }
        }

        return conflicts;
    }

    /// <summary>
    /// Check if a voucher code is unique (for code-based promotions).
    /// </summary>
    public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
    {
        var query = _context.Promotions.Where(p => p.Code == code);
        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);
        return !await query.AnyAsync();
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        return await _context.Categories.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId)
    {
        return await _context.Products
            .Where(p => p.CategoryId == categoryId)
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
