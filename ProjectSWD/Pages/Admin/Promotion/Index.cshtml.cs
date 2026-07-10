using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectSWD.Data.Entities;
using ProjectSWD.Services.Admin;

namespace ProjectSWD.Pages.Admin.Promotion;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly PromotionService _promotionService;

    public IndexModel(PromotionService promotionService)
    {
        _promotionService = promotionService;
    }

    public List<Data.Entities.Promotion> Promotions { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Promotions = await _promotionService.GetAllAsync();

        if (!string.IsNullOrEmpty(SearchTerm))
        {
            var term = SearchTerm.ToLower();
            Promotions = Promotions.Where(p =>
                p.Name.ToLower().Contains(term) ||
                (p.Description != null && p.Description.ToLower().Contains(term))
            ).ToList();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _promotionService.DeleteAsync(id);
        TempData["SuccessMessage"] = "Xóa khuyến mãi thành công!";
        return RedirectToPage();
    }
}
