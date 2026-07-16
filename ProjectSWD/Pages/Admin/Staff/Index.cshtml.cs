using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectSWD.Data.Entities;
using ProjectSWD.Services.Admin;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectSWD.Pages.Admin.Staff
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly StaffService _staffService;

        public IndexModel(StaffService staffService)
        {
            _staffService = staffService;
        }

        public List<Data.Entities.Staff> Staffs { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var allStaffs = await _staffService.GetAllAsync();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                var term = SearchTerm.ToLower();
                Staffs = allStaffs.Where(s =>
                    (s.FullName != null && s.FullName.ToLower().Contains(term)) ||
                    (s.Email != null && s.Email.ToLower().Contains(term)) ||
                    (s.Phone != null && s.Phone.ToLower().Contains(term))
                ).ToList();
            }
            else
            {
                Staffs = allStaffs;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var result = await _staffService.DeleteAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Xóa nhân viên thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Xóa nhân viên không thành công hoặc không tìm thấy nhân viên.";
            }

            return RedirectToPage();
        }
    }
}
