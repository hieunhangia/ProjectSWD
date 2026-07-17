using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectSWD.Data;
using ProjectSWD.Data.Entities;

namespace ProjectSWD.Controllers.Customer.MyOrders
{
    [Authorize]
    [Route("OrderHistory")]
    public class OrderHistory(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        private static readonly List<ProjectSWD.Data.Enums.OrderStatus> StatusSteps = new()
        {
            ProjectSWD.Data.Enums.OrderStatus.AwaitingConfirmation,
            ProjectSWD.Data.Enums.OrderStatus.Confirmed,
            ProjectSWD.Data.Enums.OrderStatus.InTransit,
            ProjectSWD.Data.Enums.OrderStatus.Delivered
        };

        private string GetStatusString(ProjectSWD.Data.Enums.OrderStatus status)
        {
            return status switch
            {
                ProjectSWD.Data.Enums.OrderStatus.Cancelled => "Cancelled",
                ProjectSWD.Data.Enums.OrderStatus.AwaitingConfirmation => "Awaiting Confirmation",
                ProjectSWD.Data.Enums.OrderStatus.Confirmed => "Confirmed",
                ProjectSWD.Data.Enums.OrderStatus.InTransit => "In Transit",
                ProjectSWD.Data.Enums.OrderStatus.Delivered => "Delivered Successfully",
                _ => "Awaiting Confirmation"
            };
        }

        private int GetStatusCode(ProjectSWD.Data.Enums.OrderStatus status)
        {
            return status switch
            {
                ProjectSWD.Data.Enums.OrderStatus.Cancelled => 0,
                ProjectSWD.Data.Enums.OrderStatus.AwaitingConfirmation => 1,
                ProjectSWD.Data.Enums.OrderStatus.Confirmed => 2,
                ProjectSWD.Data.Enums.OrderStatus.InTransit => 3,
                ProjectSWD.Data.Enums.OrderStatus.Delivered => 4,
                _ => 1
            };
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var orders = await _context.Orders
                .Where(o => o.CustomerId == userId)
                .OrderByDescending(o => o.Time)
                .ToListAsync();

            return View("~/Pages/Customer/MyOrders/OrderHistory.cshtml", orders);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == userId);

            if (order == null)
            {
                return NotFound("Không tìm thấy đơn hàng.");
            }

            return View("~/Pages/Customer/MyOrders/OrderHistoryDetail.cshtml", order);
        }

        [HttpPost("AdvanceTracking")]
        public async Task<IActionResult> AdvanceTracking(int orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện thao tác này." });
            }

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == userId);
            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
            }

            var currentStatus = order.Status;

            int currentIndex = StatusSteps.IndexOf(currentStatus);

            if (currentIndex >= 0 && currentIndex < StatusSteps.Count - 1)
            {
                order.Status = StatusSteps[currentIndex + 1];

                await _context.SaveChangesAsync();
                return Json(new { success = true, newStatus = GetStatusString(order.Status), statusCode = GetStatusCode(order.Status) });
            }

            return Json(new { success = false, message = "Không thể cập nhật thêm trạng thái của đơn hàng." });
        }

        [HttpPost("CancelOrder")]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện thao tác này." });
            }

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == userId);
            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
            }

            if (order.Status == ProjectSWD.Data.Enums.OrderStatus.AwaitingConfirmation)
            {
                order.Status = ProjectSWD.Data.Enums.OrderStatus.Cancelled;
                await _context.SaveChangesAsync();
                return Json(new { success = true, newStatus = "Cancelled", statusCode = 0 });
            }

            return Json(new { success = false, message = "Chỉ đơn hàng đang chờ xác nhận mới có thể hủy." });
        }
    }
}
