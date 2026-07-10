using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectSWD.Data.Entities;
using ProjectSWD.Services.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectSWD.Pages.Admin.Refunds
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly RefundService _refundService;

        public IndexModel(RefundService refundService)
        {
            _refundService = refundService;
        }

        public List<Refund> Refunds { get; set; } = new List<Refund>();

        [BindProperty(SupportsGet = true)]
        public ProjectSWD.Data.Enums.RefundStatus? StatusFilter { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Refunds = await _refundService.GetRefundsAsync(StatusFilter);
            return Page();
        }
    }
}
