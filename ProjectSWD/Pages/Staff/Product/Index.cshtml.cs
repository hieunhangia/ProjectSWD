using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectSWD.Data.Entities;
using ProjectSWD.Services.Staff;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectSWD.Pages.Staff.Product
{
    [Authorize(Roles = "Staff,Admin")]
    public class IndexModel : PageModel
    {
        private readonly ProductService _productService;

        public IndexModel(ProductService productService)
        {
            _productService = productService;
        }

        public List<Data.Entities.Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var allProducts = await _productService.GetAllAsync();
            Categories = await _productService.GetCategoriesAsync();

            // Filter products
            var query = allProducts.AsQueryable();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                var term = SearchTerm.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(term));
            }

            if (CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == CategoryId.Value);
            }

            Products = query.ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var result = await _productService.DeleteAsync(id);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Xóa sản phẩm thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Xóa sản phẩm không thành công.";
            }

            return RedirectToPage();
        }
    }
}
