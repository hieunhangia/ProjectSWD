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
    [Required(ErrorMessage = "Vui lòng nhập địa chỉ.")]
    [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự.")]
    public string Address { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; set; } = string.Empty;

    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Challenge();

        var customer = await _profileService.GetCustomerByIdAsync(userId);
        if (customer == null)
        {
            return NotFound("Không tìm thấy thông tin khách hàng.");
        }

        FullName = customer.FullName;
        Phone = customer.Phone;
        Address = customer.Address;
        Email = customer.Email;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Challenge();

        var success = await _profileService.UpdateCustomerProfileAsync(userId, FullName, Phone, Address);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Không thể cập nhật thông tin.");
            return Page();
        }

        SuccessMessage = "Cập nhật thông tin thành công!";
        return Page();
    }
}
