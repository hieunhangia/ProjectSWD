using System.Text;
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

    /// <summary>
    /// BR-16: Only "Delivered" orders count. Refunded amounts are deducted automatically.
    /// </summary>
    public async Task<RevenueSummary> GetSummaryAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Orders
            .Include(o => o.Bill)
            .Include(o => o.Refunds)
            .Where(o => o.Status == OrderStatus.Delivered);

        if (startDate.HasValue)
            query = query.Where(o => o.Time >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(o => o.Time <= endDate.Value);

        var orders = await query.ToListAsync();

        // BR-16: Subtract refunded amounts to show net revenue
        var grossRevenue = orders.Sum(o => o.TotalPrice);
        var totalRefunded = orders.Sum(o =>
            o.Refunds?.Where(r => r.Status == RefundStatus.Refunded).Sum(r => r.Amount) ?? 0);
        var netRevenue = grossRevenue - totalRefunded;

        return new RevenueSummary
        {
            GrossRevenue = grossRevenue,
            TotalRefunded = totalRefunded,
            NetRevenue = netRevenue,
            TotalOrders = orders.Count,
            AverageOrderValue = orders.Any() ? Math.Round(grossRevenue / orders.Count, 0) : 0,
            StartDate = startDate,
            EndDate = endDate
        };
    }

    public async Task<List<DailyRevenue>> GetDailyRevenueAsync(DateTime startDate, DateTime endDate)
    {
        var data = await _context.Orders
            .Where(o => o.Status == OrderStatus.Delivered
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
            .Where(o => o.Status == OrderStatus.Delivered
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
            .Where(oi => oi.Order.Status == OrderStatus.Delivered)
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

    public async Task<List<CategoryRevenue>> GetRevenueByCategoryAsync(DateTime startDate, DateTime endDate)
    {
        var data = await _context.OrderItems
            .Where(oi => oi.Order.Status == OrderStatus.Delivered
                        && oi.Order.Time >= startDate
                        && oi.Order.Time <= endDate)
            .Include(oi => oi.Product)
            .ThenInclude(p => p.Category)
            .GroupBy(oi => oi.Product.Category.Name)
            .Select(g => new CategoryRevenue
            {
                CategoryName = g.Key,
                Revenue = g.Sum(oi => oi.Quantity * oi.Price),
                OrderCount = g.Select(oi => oi.OrderId).Distinct().Count()
            })
            .OrderByDescending(c => c.Revenue)
            .ToListAsync();

        return data;
    }

    /// <summary>
    /// Export daily revenue to CSV format.
    /// </summary>
    public async Task<string> ExportToCsvAsync(DateTime startDate, DateTime endDate)
    {
        var daily = await GetDailyRevenueAsync(startDate, endDate);
        var summary = await GetSummaryAsync(startDate, endDate);

        var sb = new StringBuilder();
        sb.AppendLine("Báo cáo doanh thu");
        sb.AppendLine($"Khoảng thời gian,{startDate:dd/MM/yyyy},{endDate:dd/MM/yyyy}");
        sb.AppendLine($"Doanh thu gộp,{summary.GrossRevenue:N0}₫");
        sb.AppendLine($"Hoàn tiền,{summary.TotalRefunded:N0}₫");
        sb.AppendLine($"Doanh thu ròng,{summary.NetRevenue:N0}₫");
        sb.AppendLine($"Tổng đơn hàng,{summary.TotalOrders}");
        sb.AppendLine();
        sb.AppendLine("Ngày,Doanh thu,Số đơn hàng");
        foreach (var d in daily)
        {
            sb.AppendLine($"{d.Date:dd/MM/yyyy},{d.Revenue},{d.OrderCount}");
        }

        return sb.ToString();
    }

    public async Task<List<string>> GetAvailableYearsAsync()
    {
        return await _context.Orders
            .Where(o => o.Status == OrderStatus.Delivered)
            .Select(o => o.Time.Year)
            .Distinct()
            .OrderByDescending(y => y)
            .Select(y => y.ToString())
            .ToListAsync();
    }
}

public class RevenueSummary
{
    public decimal GrossRevenue { get; set; }
    public decimal TotalRefunded { get; set; }
    public decimal NetRevenue { get; set; }
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

public class CategoryRevenue
{
    public string CategoryName { get; set; } = string.Empty;
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
