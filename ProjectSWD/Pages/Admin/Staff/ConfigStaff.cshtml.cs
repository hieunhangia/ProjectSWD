using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectSWD.Data.Entities;
using ProjectSWD.Services.Admin;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectSWD.Pages.Admin.Staff
{
    [Authorize(Roles = "Admin")]
    public class ConfigStaffModel : PageModel
    {
        private readonly StaffService _staffService;

        public ConfigStaffModel(StaffService staffService)
        {
            _staffService = staffService;
        }

        [BindProperty]
        public string? StaffId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập họ và tên nhân viên.")]
        [StringLength(255, ErrorMessage = "Họ tên không được vượt quá 255 ký tự.")]
        public string FullName { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
        public string Phone { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không đúng định dạng.")]
        [StringLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự.")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải dài từ 6 đến 100 ký tự.")]
        public string? Password { get; set; }

        [BindProperty]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string? ConfirmPassword { get; set; }

        public bool IsEdit => !string.IsNullOrEmpty(StaffId);

        public async Task<IActionResult> OnGetAsync(string? id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                var staff = await _staffService.GetByIdAsync(id);
                if (staff == null)
                {
                    return NotFound();
                }

                StaffId = staff.Id;
                FullName = staff.FullName;
                Phone = staff.Phone;
                Email = staff.Email;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // If creating, Password is required
            if (!IsEdit && string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError(nameof(Password), "Mật khẩu là bắt buộc khi tạo tài khoản nhân viên mới.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                if (IsEdit)
                {
                    // Update existing
                    var result = await _staffService.UpdateAsync(StaffId!, FullName, Email, Phone, Password);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error);
                        }
                        return Page();
                    }

                    TempData["SuccessMessage"] = "Cập nhật thông tin nhân viên thành công!";
                }
                else
                {
                    // Create new
                    var staff = new Data.Entities.Staff
                    {
                        FullName = FullName,
                        Email = Email,
                        Phone = Phone
                    };

                    var result = await _staffService.CreateAsync(staff, Password!);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error);
                        }
                        return Page();
                    }

                    TempData["SuccessMessage"] = "Tạo tài khoản nhân viên mới thành công!";
                }

                return RedirectToPage("/Admin/Staff/Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi hệ thống: {ex.Message}");
                return Page();
            }
        }
    }
}
