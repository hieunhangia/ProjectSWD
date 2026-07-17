using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectSWD.Data;
using ProjectSWD.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ProjectSWD.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public string? Role { get; set; }

        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = string.Empty;

        [TempData]
        public string? StatusMessage { get; set; }

        [BindProperty]
        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(255, ErrorMessage = "Họ tên không được vượt quá 255 ký tự.")]
        public string FullName { get; set; } = string.Empty;

        [BindProperty]
        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
        public string? PhoneNumber { get; set; }

        [BindProperty]
        [Display(Name = "Email")]
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Display(Name = "Địa chỉ")]
        [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự.")]
        public string? Address { get; set; }

        private async Task LoadAsync(IdentityUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var email = await _userManager.GetEmailAsync(user);

            Username = userName ?? string.Empty;
            Email = email ?? string.Empty;
            PhoneNumber = phoneNumber;

            // Lấy thông tin mở rộng từ bảng Customer hoặc Staff
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == user.Id);
            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.Id == user.Id);

            if (customer != null)
            {
                Role = "Customer";
                FullName = customer.FullName;
                Address = customer.Address;
                if (string.IsNullOrEmpty(PhoneNumber) || PhoneNumber == customer.Phone)
                    PhoneNumber = customer.Phone;
            }
            else if (staff != null)
            {
                Role = "Staff";
                FullName = staff.FullName;
                if (string.IsNullOrEmpty(PhoneNumber) || PhoneNumber == staff.Phone)
                    PhoneNumber = staff.Phone;
            }
            else
            {
                Role = "Admin";
                FullName = userName ?? string.Empty;
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // Cập nhật email
            var email = await _userManager.GetEmailAsync(user);
            if (Email != email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, Email);
                if (!setEmailResult.Succeeded)
                {
                    foreach (var error in setEmailResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    await LoadAsync(user);
                    return Page();
                }
            }

            // Cập nhật số điện thoại
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    foreach (var error in setPhoneResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    await LoadAsync(user);
                    return Page();
                }
            }

            // Cập nhật FullName + Address vào bảng Customer/Staff
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == user.Id);
            if (customer != null)
            {
                customer.FullName = FullName;
                customer.Phone = PhoneNumber ?? string.Empty;
                customer.Address = Address ?? string.Empty;
                customer.Email = Email;
            }
            else
            {
                var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.Id == user.Id);
                if (staff != null)
                {
                    staff.FullName = FullName;
                    staff.Phone = PhoneNumber ?? string.Empty;
                    staff.Email = Email;
                }
            }

            await _context.SaveChangesAsync();
            await _signInManager.RefreshSignInAsync(user);

            StatusMessage = "Profile Information Updated Successfully";
            return RedirectToPage();
        }
    }
}
