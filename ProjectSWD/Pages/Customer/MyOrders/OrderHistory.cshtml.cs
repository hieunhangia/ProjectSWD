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

        private static readonly List<string> StatusSteps = new()
        {
            "Awaiting Confirmation",
            "Confirmed",
            "In Transit",
            "Delivered Successfully"
        };

        private int GetStatusCode(string status)
        {
            return status switch
            {
                "Cancelled" => 0,
                "Awaiting Confirmation" => 1,
                "Confirmed" => 2,
                "In Transit" => 3,
                "Delivered Successfully" => 4,
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

            return View("~/Pages/Customer/MyOrders/Index.cshtml", orders);
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

            return View("~/Pages/Customer/MyOrders/OrderHistory.cshtml", order);
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

            string currentStatus = order.Status;
            if (string.IsNullOrWhiteSpace(currentStatus) || (!StatusSteps.Contains(currentStatus) && currentStatus != "Cancelled"))
            {
                currentStatus = "Awaiting Confirmation";
            }

            int currentIndex = StatusSteps.IndexOf(currentStatus);

            if (currentIndex >= 0 && currentIndex < StatusSteps.Count - 1)
            {
                order.Status = StatusSteps[currentIndex + 1];
                
                if (order.Status == "Confirmed")
                {
                    order.ApprovementStatus = ProjectSWD.Data.Enums.OrderStatus.Confirmed;
                }
                else if (order.Status == "In Transit")
                {
                    order.ApprovementStatus = ProjectSWD.Data.Enums.OrderStatus.Confirmed;
                }
                else if (order.Status == "Delivered Successfully")
                {
                    order.ApprovementStatus = ProjectSWD.Data.Enums.OrderStatus.Delivered;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, newStatus = order.Status, statusCode = GetStatusCode(order.Status) });
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

            string currentStatus = order.Status;
            if (string.IsNullOrWhiteSpace(currentStatus))
            {
                currentStatus = "Awaiting Confirmation";
            }

            if (currentStatus == "Awaiting Confirmation")
            {
                order.Status = "Cancelled";
                order.ApprovementStatus = ProjectSWD.Data.Enums.OrderStatus.Cancelled;
                await _context.SaveChangesAsync();
                return Json(new { success = true, newStatus = "Cancelled", statusCode = 0 });
            }

            return Json(new { success = false, message = "Chỉ đơn hàng đang chờ xác nhận mới có thể hủy." });
        }
    }
}
