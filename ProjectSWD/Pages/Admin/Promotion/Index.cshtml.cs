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
        TempData["SuccessMessage"] = "Đã xóa khuyến mãi.";
        return RedirectToPage();
    }

    /// <summary>
    /// A2: Emergency Campaign Revocation — terminate an active campaign immediately.
    /// </summary>
    public async Task<IActionResult> OnPostTerminateAsync(int id)
    {
        await _promotionService.TerminateAsync(id);
        TempData["SuccessMessage"] = "Khuyến mãi đã bị hủy khẩn cấp (Emergency Terminated).";
        return RedirectToPage();
    }
}
