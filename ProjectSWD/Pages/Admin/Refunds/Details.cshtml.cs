using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectSWD.Data.Entities;
using ProjectSWD.Services.Admin;
using System.Threading.Tasks;

namespace ProjectSWD.Pages.Admin.Refunds
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel : PageModel
    {
        private readonly RefundService _refundService;

        public DetailsModel(RefundService refundService)
        {
            _refundService = refundService;
        }

        public Refund RefundData { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var refund = await _refundService.GetRefundDetailsAsync(id);
            if (refund == null)
            {
                TempData["ErrorMessage"] = "Refund request not found.";
                return RedirectToPage("./Index");
            }
            
            RefundData = refund;
            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var (success, message) = await _refundService.ApproveRefundAsync(id);
            
            if (success)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToPage("./Index");
            }
            else
            {
                TempData["ErrorMessage"] = message;
                return RedirectToPage(new { id = id });
            }
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            var (success, message) = await _refundService.RejectRefundAsync(id);

            if (success)
            {
                TempData["SuccessMessage"] = message;
                return RedirectToPage("./Index");
            }
            else
            {
                TempData["ErrorMessage"] = message;
                return RedirectToPage(new { id = id });
            }
        }
    }
}
