using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectSWD.Services.Customer;

namespace ProjectSWD.Pages.Customer.Profile;

[Authorize(Roles = "Customer")]
public class IndexModel : PageModel
{
    private readonly ProfileService _profileService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public IndexModel(ProfileService profileService, UserManager<IdentityUser> userManager,
                      SignInManager<IdentityUser> signInManager)
    {
        _profileService = profileService;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // --- Profile fields ---

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
    [Required(ErrorMessage = "Vui lòng nhập địa chỉ.")]
    [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự.")]
    public string Address { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [EmailAddress(ErrorMessage = "Vui lòng nhập email hợp lệ.")]
    public string Email { get; set; } = string.Empty;

    // --- Password change (A1) ---

    [BindProperty]
    public bool IsChangingPassword { get; set; }

    [BindProperty]
    [DataType(DataType.Password)]
    public string? OldPassword { get; set; }

    [BindProperty]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.")]
    public string? NewPassword { get; set; }

    [BindProperty]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    public string? ConfirmNewPassword { get; set; }

    // --- UI state ---

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Challenge();

        var customer = await _profileService.GetCustomerByIdAsync(userId);
        if (customer == null)
            return NotFound("Không tìm thấy thông tin khách hàng.");

        FullName = customer.FullName;
        Phone = customer.Phone;
        Address = customer.Address;
        Email = customer.Email;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Challenge();

        if (!ModelState.IsValid) return Page();

        // Update profile info
        var success = await _profileService.UpdateCustomerProfileAsync(userId, FullName, Phone, Address);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Không thể cập nhật thông tin.");
            return Page();
        }

        // A1: Password change
        if (IsChangingPassword)
        {
            if (string.IsNullOrEmpty(OldPassword) || string.IsNullOrEmpty(NewPassword))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập mật khẩu cũ và mật khẩu mới.");
                return Page();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Challenge();

            // E2: Verify old password
            var changeResult = await _userManager.ChangePasswordAsync(user, OldPassword, NewPassword);
            if (!changeResult.Succeeded)
            {
                foreach (var err in changeResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                }
                SuccessMessage = "Profile Information Updated Successfully";
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
        }

        SuccessMessage = "Profile Information Updated Successfully";
        return Page();
    }
}
