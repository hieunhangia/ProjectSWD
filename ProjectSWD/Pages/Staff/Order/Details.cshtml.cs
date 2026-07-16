using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectSWD.Data.Entities;
using ProjectSWD.Data.Enums;
using ProjectSWD.Services.Staff;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjectSWD.Pages.Staff.Order
{
    [Authorize(Roles = "Staff,Admin")]
    public class DetailsModel : PageModel
    {
        private readonly OrderService _orderService;

        public DetailsModel(OrderService orderService)
        {
            _orderService = orderService;
        }

        public Data.Entities.Order Order { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound("Không tìm thấy đơn hàng.");
            }

            Order = order;
            return Page();
        }

        public async Task<IActionResult> OnPostConfirmAsync(int id)
        {
            var staffId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(staffId))
            {
                TempData["ErrorMessage"] = "Bạn phải đăng nhập để thực hiện hành động này.";
                return RedirectToPage(new { id });
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

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var staffId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(staffId))
            {
                TempData["ErrorMessage"] = "Bạn phải đăng nhập để thực hiện hành động này.";
                return RedirectToPage(new { id });
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

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostShipAsync(int id)
        {
            var staffId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(staffId))
            {
                TempData["ErrorMessage"] = "Bạn phải đăng nhập để thực hiện hành động này.";
                return RedirectToPage(new { id });
            }

            var success = await _orderService.UpdateOrderStatusAsync(id, staffId, OrderStatus.InTransit);
            if (success)
            {
                TempData["SuccessMessage"] = $"Đơn hàng #{id} đã được chuyển sang trạng thái đang giao!";
            }
            else
            {
                TempData["ErrorMessage"] = $"Không thể cập nhật trạng thái đơn hàng #{id}.";
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            var staffId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(staffId))
            {
                TempData["ErrorMessage"] = "Bạn phải đăng nhập để thực hiện hành động này.";
                return RedirectToPage(new { id });
            }

            var success = await _orderService.UpdateOrderStatusAsync(id, staffId, OrderStatus.Delivered);
            if (success)
            {
                TempData["SuccessMessage"] = $"Đơn hàng #{id} đã được chuyển sang trạng thái hoàn thành giao hàng!";
            }
            else
            {
                TempData["ErrorMessage"] = $"Không thể hoàn thành đơn hàng #{id}.";
            }

            return RedirectToPage(new { id });
        }
    }
}
