using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectSWD.Data.Entities;
using ProjectSWD.Data.Enums;
using ProjectSWD.Services.Staff;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjectSWD.Pages.Staff.Order
{
    [Authorize(Roles = "Staff,Admin")]
    public class IndexModel : PageModel
    {
        private readonly OrderService _orderService;

        public IndexModel(OrderService orderService)
        {
            _orderService = orderService;
        }

        public List<Data.Entities.Order> Orders { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var allOrders = await _orderService.GetAllOrdersAsync();
            var query = allOrders.AsQueryable();

            // Filter by Status
            if (!string.IsNullOrEmpty(StatusFilter) && StatusFilter != "All")
            {
                query = query.Where(o => o.Status.ToString() == StatusFilter);
            }

            // Filter by Search Term
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                var term = SearchTerm.Trim().ToLower();
                query = query.Where(o =>
                    o.Id.ToString().Contains(term) ||
                    (!string.IsNullOrEmpty(o.FullName) && o.FullName.ToLower().Contains(term)) ||
                    (!string.IsNullOrEmpty(o.PhoneNumber) && o.PhoneNumber.Contains(term))
                );
            }

            Orders = query.ToList();
            return Page();
        }

        public async Task<IActionResult> OnPostConfirmAsync(int id)
        {
            var staffId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(staffId))
            {
                TempData["ErrorMessage"] = "Bạn phải đăng nhập để thực hiện hành động này.";
                return RedirectToPage();
            }

            var success = await _orderService.ConfirmOrderAsync(id, staffId);
            if (success)
            {
                TempData["SuccessMessage"] = $"Đã xác nhận đơn hàng #{id} thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = $"Không thể xác nhận đơn hàng #{id}.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var staffId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(staffId))
            {
                TempData["ErrorMessage"] = "Bạn phải đăng nhập để thực hiện hành động này.";
                return RedirectToPage();
            }

            var success = await _orderService.CancelOrderAsync(id, staffId);
            if (success)
            {
                TempData["SuccessMessage"] = $"Đã hủy đơn hàng #{id} thành công và hoàn trả tồn kho!";
            }
            else
            {
                TempData["ErrorMessage"] = $"Không thể hủy đơn hàng #{id}.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostShipAsync(int id)
        {
            var staffId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(staffId))
            {
                TempData["ErrorMessage"] = "Bạn phải đăng nhập để thực hiện hành động này.";
                return RedirectToPage();
            }

            var success = await _orderService.UpdateOrderStatusAsync(id, staffId, OrderStatus.InTransit);
            if (success)
            {
                TempData["SuccessMessage"] = $"Đơn hàng #{id} đã được chuyển sang giao hàng!";
            }
            else
            {
                TempData["ErrorMessage"] = $"Không thể giao đơn hàng #{id}.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            var staffId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(staffId))
            {
                TempData["ErrorMessage"] = "Bạn phải đăng nhập để thực hiện hành động này.";
                return RedirectToPage();
            }

            var success = await _orderService.UpdateOrderStatusAsync(id, staffId, OrderStatus.Delivered);
            if (success)
            {
                TempData["SuccessMessage"] = $"Đơn hàng #{id} đã hoàn thành giao hàng!";
            }
            else
            {
                TempData["ErrorMessage"] = $"Không thể hoàn thành đơn hàng #{id}.";
            }

            return RedirectToPage();
        }
    }
}
