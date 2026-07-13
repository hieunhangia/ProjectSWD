using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectSWD.Data;
using ProjectSWD.Data.Entities;
using ProjectSWD.Data.Enums;

namespace ProjectSWD.Pages
{
    public class IndexModel : PageModel
    {
        private const int PageSize = 12;

        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();

        // productId -> phần trăm giảm giá cao nhất đang áp dụng
        public Dictionary<int, decimal> ActiveDiscounts { get; set; } = new();

        // productId -> (điểm trung bình, số lượt đánh giá)
        public Dictionary<int, (double Avg, int Count)> Ratings { get; set; } = new();

        // Gợi ý khi không có kết quả: top sản phẩm bán chạy
        public List<Product> TrendingProducts { get; set; } = new();

        public string? LoadError { get; set; }

        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int ShowingFrom { get; set; }
        public int ShowingTo { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Sort { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? MinRating { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public async Task OnGetAsync()
        {
            try
            {
                Categories = await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                // Điểm đánh giá trung bình theo sản phẩm
                var ratingRows = await _context.Reviews
                    .GroupBy(r => r.ProductId)
                    .Select(g => new { ProductId = g.Key, Avg = g.Average(r => (double)r.Rating), Count = g.Count() })
                    .ToListAsync();
                Ratings = ratingRows.ToDictionary(x => x.ProductId, x => (x.Avg, x.Count));

                // Tổng số lượng đã bán (đơn đã giao) theo sản phẩm
                var soldQuantities = await _context.OrderItems
                    .Where(oi => oi.Order.Status == OrderStatus.Delivered)
                    .GroupBy(oi => oi.ProductId)
                    .Select(g => new { ProductId = g.Key, Total = g.Sum(oi => oi.Quantity) })
                    .ToDictionaryAsync(x => x.ProductId, x => x.Total);

                var now = DateTime.Now;
                ActiveDiscounts = await _context.PromotionProducts
                    .Where(pp => pp.Promotion.Percentage != null
                                 && pp.Promotion.StartTime <= now
                                 && pp.Promotion.EndTime >= now)
                    .GroupBy(pp => pp.ProductId)
                    .Select(g => new { ProductId = g.Key, Percentage = g.Max(pp => pp.Promotion.Percentage!.Value) })
                    .ToDictionaryAsync(x => x.ProductId, x => x.Percentage);

                var query = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Unit)
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    var term = SearchTerm.Trim();
                    query = query.Where(p => p.Name.Contains(term) || p.Category.Name.Contains(term));
                }

                if (CategoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == CategoryId.Value);
                }

                if (MinPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= MinPrice.Value);
                }

                if (MaxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= MaxPrice.Value);
                }

                var filtered = await query.ToListAsync();

                if (MinRating.HasValue)
                {
                    filtered = filtered
                        .Where(p => Ratings.TryGetValue(p.Id, out var r) && r.Avg >= MinRating.Value)
                        .ToList();
                }

                filtered = Sort switch
                {
                    "price_asc" => filtered.OrderBy(p => p.Price).ToList(),
                    "price_desc" => filtered.OrderByDescending(p => p.Price).ToList(),
                    "name_desc" => filtered.OrderByDescending(p => p.Name).ToList(),
                    "best_seller" => filtered
                        .OrderByDescending(p => soldQuantities.TryGetValue(p.Id, out var sold) ? sold : 0m)
                        .ThenBy(p => p.Name)
                        .ToList(),
                    _ => filtered.OrderBy(p => p.Name).ToList()
                };

                TotalCount = filtered.Count;
                TotalPages = Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
                PageNumber = Math.Clamp(PageNumber, 1, TotalPages);

                Products = filtered
                    .Skip((PageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .ToList();

                ShowingFrom = TotalCount == 0 ? 0 : (PageNumber - 1) * PageSize + 1;
                ShowingTo = (PageNumber - 1) * PageSize + Products.Count;

                // Gợi ý sản phẩm bán chạy khi không có kết quả
                if (TotalCount == 0)
                {
                    var allProducts = await _context.Products
                        .Include(p => p.Category)
                        .Include(p => p.Unit)
                        .AsNoTracking()
                        .ToListAsync();

                    TrendingProducts = soldQuantities.Count > 0
                        ? allProducts
                            .OrderByDescending(p => soldQuantities.TryGetValue(p.Id, out var sold) ? sold : 0m)
                            .ThenBy(p => p.Name)
                            .Take(4)
                            .ToList()
                        : allProducts.OrderBy(p => p.Name).Take(4).ToList();
                }
            }
            catch (Exception)
            {
                LoadError = "Hiện không thể tải dữ liệu sản phẩm. Vui lòng tải lại trang hoặc thử lại sau.";
            }
        }
    }
}
