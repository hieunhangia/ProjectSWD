using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectSWD.Data.Entities;
using ProjectSWD.Services.Staff;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectSWD.Pages.Staff.Product
{
    [Authorize(Roles = "Staff,Admin")]
    public class ConfigProductModel : PageModel
    {
        private readonly ProductService _productService;

        public ConfigProductModel(ProductService productService)
        {
            _productService = productService;
        }

        [BindProperty(SupportsGet = true)]
        public int? ProductId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        [StringLength(255, ErrorMessage = "Tên sản phẩm không được vượt quá 255 ký tự.")]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập đường dẫn ảnh sản phẩm.")]
        [StringLength(500, ErrorMessage = "Đường dẫn ảnh không được vượt quá 500 ký tự.")]
        [Url(ErrorMessage = "Đường dẫn ảnh phải là một URL hợp lệ.")]
        public string ImageUrl { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập giá bán.")]
        [Range(1000, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn hoặc bằng 1.000₫.")]
        public decimal Price { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập số lượng.")]
        [Range(0, double.MaxValue, ErrorMessage = "Số lượng tồn kho không được âm.")]
        public decimal Quantity { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
        public int CategoryId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng chọn đơn vị tính.")]
        public int UnitId { get; set; }

        public List<SelectListItem> CategoryOptions { get; set; } = new();
        public List<SelectListItem> UnitOptions { get; set; } = new();

        public bool IsEdit => ProductId.HasValue;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            await LoadDropdownsAsync();

            if (id.HasValue)
            {
                var prod = await _productService.GetByIdAsync(id.Value);
                if (prod == null)
                {
                    return NotFound();
                }

                ProductId = prod.Id;
                Name = prod.Name;
                ImageUrl = prod.ImageUrl;
                Price = prod.Price;
                Quantity = prod.Quantity;
                CategoryId = prod.CategoryId;
                UnitId = prod.UnitId;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadDropdownsAsync();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                if (ProductId.HasValue)
                {
                    // Update
                    var prod = await _productService.GetByIdAsync(ProductId.Value);
                    if (prod == null)
                    {
                        return NotFound();
                    }

                    prod.Name = Name;
                    prod.ImageUrl = ImageUrl;
                    prod.Price = Price;
                    prod.Quantity = Quantity;
                    prod.CategoryId = CategoryId;
                    prod.UnitId = UnitId;

                    await _productService.UpdateAsync(prod);
                    TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                }
                else
                {
                    // Create
                    var prod = new Data.Entities.Product
                    {
                        Name = Name,
                        ImageUrl = ImageUrl,
                        Price = Price,
                        Quantity = Quantity,
                        CategoryId = CategoryId,
                        UnitId = UnitId
                    };

                    await _productService.CreateAsync(prod);
                    TempData["SuccessMessage"] = "Thêm sản phẩm mới thành công!";
                }

                return RedirectToPage("/Staff/Product/Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi lưu dữ liệu: {ex.Message}");
                return Page();
            }
        }

        private async Task LoadDropdownsAsync()
        {
            var categories = await _productService.GetCategoriesAsync();
            CategoryOptions = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            var units = await _productService.GetUnitsAsync();
            UnitOptions = units.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = u.Name
            }).ToList();
        }
    }
}
