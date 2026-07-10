using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectSWD.Services.Customer;

namespace ProjectSWD.Pages.Staff.Profile;

[Authorize(Roles = "Staff")]
public class IndexModel : PageModel
{
    private readonly ProfileService _profileService;
    private readonly UserManager<IdentityUser> _userManager;

    public IndexModel(ProfileService profileService, UserManager<IdentityUser> userManager)
    {
        _profileService = profileService;
        _userManager = userManager;
    }

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
    [StringLength(255, ErrorMessage = "Họ tên không được vượt quá 255 ký tự.")]
    public string FullName { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
    [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    public string Phone { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; set; } = string.Empty;

    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Challenge();

        var staff = await _profileService.GetStaffByIdAsync(userId);
        if (staff == null)
        {
            return NotFound("Không tìm thấy thông tin nhân viên.");
        }

        FullName = staff.FullName;
        Phone = staff.Phone;
        Email = staff.Email;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Challenge();

        var success = await _profileService.UpdateStaffProfileAsync(userId, FullName, Phone);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Không thể cập nhật thông tin.");
            return Page();
        }

        SuccessMessage = "Cập nhật thông tin thành công!";
        return Page();
    }
}
