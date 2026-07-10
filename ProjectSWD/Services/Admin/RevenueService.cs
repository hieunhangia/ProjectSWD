using Microsoft.EntityFrameworkCore;
using ProjectSWD.Data;
using ProjectSWD.Data.Entities;
using ProjectSWD.Data.Enums;

namespace ProjectSWD.Services.Admin;

public class RevenueService
{
    private readonly ApplicationDbContext _context;

    public RevenueService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RevenueSummary> GetSummaryAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Orders
            .Include(o => o.Bill)
            .Where(o => o.ApprovementStatus == OrderStatus.Delivered);

        if (startDate.HasValue)
            query = query.Where(o => o.Time >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(o => o.Time <= endDate.Value);

        var orders = await query.ToListAsync();

        return new RevenueSummary
        {
            TotalRevenue = orders.Sum(o => o.TotalPrice),
            TotalOrders = orders.Count,
            AverageOrderValue = orders.Any() ? Math.Round(orders.Average(o => o.TotalPrice), 0) : 0,
            StartDate = startDate,
            EndDate = endDate
        };
    }

    public async Task<List<DailyRevenue>> GetDailyRevenueAsync(DateTime startDate, DateTime endDate)
    {
        var data = await _context.Orders
            .Where(o => o.ApprovementStatus == OrderStatus.Delivered
                        && o.Time >= startDate && o.Time <= endDate)
            .GroupBy(o => o.Time.Date)
            .Select(g => new DailyRevenue
            {
                Date = g.Key,
                Revenue = g.Sum(o => o.TotalPrice),
                OrderCount = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToListAsync();

        return data;
    }

    public async Task<List<MonthlyRevenue>> GetMonthlyRevenueAsync(int year)
    {
        var data = await _context.Orders
            .Where(o => o.ApprovementStatus == OrderStatus.Delivered
                        && o.Time.Year == year)
            .GroupBy(o => o.Time.Month)
            .Select(g => new MonthlyRevenue
            {
                Month = g.Key,
                Revenue = g.Sum(o => o.TotalPrice),
                OrderCount = g.Count()
            })
            .OrderBy(m => m.Month)
            .ToListAsync();

        return data;
    }

    public async Task<List<TopProduct>> GetTopSellingProductsAsync(int top = 10)
    {
        var data = await _context.OrderItems
            .Where(oi => oi.Order.ApprovementStatus == OrderStatus.Delivered)
            .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
            .Select(g => new TopProduct
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                TotalQuantity = g.Sum(oi => oi.Quantity),
                TotalRevenue = g.Sum(oi => oi.Quantity * oi.Price)
            })
            .OrderByDescending(p => p.TotalRevenue)
            .Take(top)
            .ToListAsync();

        return data;
    }
}

public class RevenueSummary
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class DailyRevenue
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class MonthlyRevenue
{
    public int Month { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class TopProduct
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal TotalQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
}
