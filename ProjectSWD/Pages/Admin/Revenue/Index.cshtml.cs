using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectSWD.Services.Admin;

namespace ProjectSWD.Pages.Admin.Revenue;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly RevenueService _revenueService;

    public IndexModel(RevenueService revenueService)
    {
        _revenueService = revenueService;
    }

    public RevenueSummary Summary { get; set; } = new();
    public List<DailyRevenue> DailyRevenues { get; set; } = new();
    public List<MonthlyRevenue> MonthlyRevenues { get; set; } = new();
    public List<TopProduct> TopProducts { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public DateTime StartDate { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

    [BindProperty(SupportsGet = true)]
    public DateTime EndDate { get; set; } = DateTime.Now;

    [BindProperty(SupportsGet = true)]
    public int SelectedYear { get; set; } = DateTime.Now.Year;

    public async Task<IActionResult> OnGetAsync()
    {
        Summary = await _revenueService.GetSummaryAsync(StartDate, EndDate);
        DailyRevenues = await _revenueService.GetDailyRevenueAsync(StartDate, EndDate);
        MonthlyRevenues = await _revenueService.GetMonthlyRevenueAsync(SelectedYear);
        TopProducts = await _revenueService.GetTopSellingProductsAsync(10);

        return Page();
    }
}
