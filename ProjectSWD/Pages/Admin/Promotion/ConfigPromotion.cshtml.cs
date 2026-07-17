using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectSWD.Data.Entities;
using ProjectSWD.Services.Admin;

namespace ProjectSWD.Pages.Admin.Promotion;

[Authorize(Roles = "Admin")]
public class ConfigPromotionModel : PageModel
{
    private readonly PromotionService _promotionService;

    public ConfigPromotionModel(PromotionService promotionService)
    {
        _promotionService = promotionService;
    }

    [BindProperty(SupportsGet = true)]
    public int? PromotionId { get; set; }

    // ——— Core fields ———

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng nhập tên khuyến mãi.")]
    [StringLength(255, ErrorMessage = "Tên không được vượt quá 255 ký tự.")]
    public string Name { get; set; } = string.Empty;

    [BindProperty]
    [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
    public string? Description { get; set; }

    // ——— Discount type ———

    [BindProperty]
    [Range(0, 100, ErrorMessage = "Phần trăm giảm phải từ 0 đến 100.")]
    public decimal? Percentage { get; set; }

    [BindProperty]
    [Range(0, double.MaxValue, ErrorMessage = "Số tiền giảm phải là số không âm.")]
    public decimal? FixedAmount { get; set; }

    // ——— Voucher Code (A1) ———

    [BindProperty]
    [StringLength(100, ErrorMessage = "Mã code không được vượt quá 100 ký tự.")]
    public string? Code { get; set; }

    [BindProperty]
    public bool IsCodeBased { get; set; }

    [BindProperty]
    public List<int> SelectedProductIds { get; set; } = new();

    // ——— Time ———

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng chọn thời gian bắt đầu.")]
    [DataType(DataType.DateTime)]
    public DateTime StartTime { get; set; } = DateTime.Now;

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng chọn thời gian kết thúc.")]
    [DataType(DataType.DateTime)]
    public DateTime EndTime { get; set; } = DateTime.Now.AddDays(7);

    // ——— Caps & limits ———

    [BindProperty]
    [Range(0, int.MaxValue, ErrorMessage = "Giới hạn sử dụng phải là số không âm.")]
    public int? UsageLimit { get; set; }

    [BindProperty]
    [Range(0, int.MaxValue, ErrorMessage = "Giới hạn mỗi người dùng phải là số không âm.")]
    public int? PerUserLimit { get; set; }

    [BindProperty]
    [Range(0, double.MaxValue, ErrorMessage = "Đơn hàng tối thiểu phải là số không âm.")]
    public decimal? MinimumOrder { get; set; }

    // ——— Presentation helpers ———

    public List<SelectListItem> ProductOptions { get; set; } = new();
    public List<string> OverlapWarnings { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public bool IsEdit => PromotionId.HasValue;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        await LoadDropdownsAsync();

        if (id.HasValue)
        {
            var promo = await _promotionService.GetByIdAsync(id.Value);
            if (promo == null) return NotFound();

            PromotionId = promo.Id;
            Name = promo.Name;
            Description = promo.Description;
            Code = promo.Code;
            IsCodeBased = !string.IsNullOrEmpty(promo.Code);
            Percentage = promo.Percentage;
            FixedAmount = promo.FixedAmount;
            StartTime = promo.StartTime;
            EndTime = promo.EndTime;
            UsageLimit = promo.UsageLimit;
            PerUserLimit = promo.PerUserLimit;
            MinimumOrder = promo.MinimumOrder;
            SelectedProductIds = promo.PromotionProducts?.Select(pp => pp.ProductId).ToList() ?? new();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadDropdownsAsync();

        // === E2: Chronological Sequence Inversion ===
        if (EndTime <= StartTime)
        {
            ModelState.AddModelError(string.Empty, "Thời gian kết thúc phải sau thời gian bắt đầu.");
        }

        // === At least one discount type ===
        if (!FixedAmount.HasValue && !Percentage.HasValue)
        {
            ModelState.AddModelError(string.Empty, "Vui lòng nhập số tiền giảm hoặc phần trăm giảm.");
        }

        // === Code uniqueness (A1) ===
        if (IsCodeBased && !string.IsNullOrEmpty(Code))
        {
            var codeUnique = await _promotionService.IsCodeUniqueAsync(Code, PromotionId);
            if (!codeUnique)
                ModelState.AddModelError(nameof(Code), "Mã code này đã được sử dụng cho khuyến mãi khác.");
        }

        // === E1: Overlapping campaign check ===
        OverlapWarnings = await _promotionService.CheckOverlappingConflictsAsync(
            StartTime, EndTime, PromotionId);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            if (PromotionId.HasValue)
            {
                // Update existing
                var promo = await _promotionService.GetByIdAsync(PromotionId.Value);
                if (promo == null) return NotFound();

                promo.Name = Name;
                promo.Description = Description;
                promo.Code = IsCodeBased ? Code : null;
                promo.FixedAmount = FixedAmount;
                promo.Percentage = Percentage;
                promo.StartTime = StartTime;
                promo.EndTime = EndTime;
                promo.UsageLimit = UsageLimit;
                promo.PerUserLimit = PerUserLimit;
                promo.MinimumOrder = MinimumOrder;
                promo.IsPercentage = Percentage.HasValue;

                await _promotionService.UpdateAsync(promo);
                await _promotionService.UpdatePromotionProductsAsync(promo.Id, SelectedProductIds);

                TempData["SuccessMessage"] = "Promotion Strategy Successfully Scheduled/Activated";
            }
            else
            {
                // Create new
                var promo = new Data.Entities.Promotion
                {
                    Name = Name,
                    Description = Description,
                    Code = IsCodeBased ? Code : null,
                    FixedAmount = FixedAmount,
                    Percentage = Percentage,
                    IsPercentage = Percentage.HasValue,
                    StartTime = StartTime,
                    EndTime = EndTime,
                    UsageLimit = UsageLimit,
                    PerUserLimit = PerUserLimit,
                    MinimumOrder = MinimumOrder
                };

                await _promotionService.CreateAsync(promo);
                await _promotionService.UpdatePromotionProductsAsync(promo.Id, SelectedProductIds);

                TempData["SuccessMessage"] = "Promotion Strategy Successfully Scheduled/Activated";
            }

            return RedirectToPage("/Admin/Promotion/Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Lỗi: {ex.Message}");
            return Page();
        }
    }

    private async Task LoadDropdownsAsync()
    {
        ProductOptions = (await _promotionService.GetAllProductsAsync())
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.Name} - {p.Price:N0}₫"
            }).ToList();
    }
}
