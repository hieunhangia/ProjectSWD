using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProjectSWD.Data;
using ProjectSWD.Data.Entities;

namespace ProjectSWD.Pages
{
    public class ProductDetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ProductDetailModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Product? Product { get; set; }
        public List<Review> Reviews { get; set; } = new();
        public double AvgRating { get; set; }
        public int ReviewCount { get; set; }

        // Phần trăm giảm giá cao nhất đang áp dụng (null nếu không có)
        public decimal? DiscountPercent { get; set; }

        public string? LoadError { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                Product = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Unit)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (Product == null)
                {
                    return NotFound();
                }

                Reviews = await _context.Reviews
                    .Include(r => r.Customer)
                    .AsNoTracking()
                    .Where(r => r.ProductId == id)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                ReviewCount = Reviews.Count;
                AvgRating = ReviewCount > 0 ? Reviews.Average(r => r.Rating) : 0;

                var now = DateTime.Now;
                DiscountPercent = await _context.PromotionProducts
                    .Where(pp => pp.ProductId == id
                                 && pp.Promotion.Percentage != null
                                 && pp.Promotion.StartTime <= now
                                 && pp.Promotion.EndTime >= now)
                    .MaxAsync(pp => (decimal?)pp.Promotion.Percentage);
            }
            catch (Exception)
            {
                LoadError = "Không thể tải thông tin sản phẩm.";
            }

            return Page();
        }
    }
}
