using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectSWD.Data;
using ProjectSWD.DTOs;
using ProjectSWD.Services.Customer;

namespace ProjectSWD.Controllers.Customer.MyOrders
{
    [Authorize]
    [Route("Checkout")]
    public class Checkout(IOrderService orderService, ICartService cartService, ApplicationDbContext context) : Controller
    {
        private readonly IOrderService _orderService = orderService;
        private readonly ICartService _cartService = cartService;
        private readonly ApplicationDbContext _context = context;

        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] string? productIds)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var cartItems = await _cartService.GetCartByCustomerIdAsync(userId);
            if (!string.IsNullOrEmpty(productIds))
            {
                try
                {
                    var idList = productIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                           .Select(int.Parse)
                                           .ToList();
                    cartItems = cartItems.Where(item => idList.Contains(item.ProductId)).ToList();
                }
                catch
                {
                }
            }

            if (cartItems == null || cartItems.Count == 0)
            {
                return Redirect("/Cart");
            }

            return View("~/Pages/Customer/MyOrders/Checkout.cshtml", cartItems);
        }

        [HttpGet("Success")]
        public async Task<IActionResult> Success(int orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Shipment)
                .Include(o => o.Bill)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == userId);

            if (order == null)
            {
                return NotFound("Không tìm thấy đơn hàng.");
            }

            return View("~/Pages/Customer/MyOrders/CheckoutSuccess.cshtml", order);
        }

        [HttpPost("CalculateShipping")]
        public async Task<IActionResult> CalculateShipping([FromBody] ShippingRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Address))
            {
                return Json(new { success = false, message = "Địa chỉ nhận hàng là bắt buộc để tính phí ship." });
            }

            try
            {
                decimal shippingFee = await _orderService.CalculateShippingFeeAsync(request.Address);
                Thread.Sleep(2000);
                return Json(new { success = true, shippingFee = shippingFee });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("SubmitOrder")]
        public async Task<IActionResult> SubmitOrder([FromBody] OrderCheckoutDTO checkoutInfo)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện đặt hàng." });
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Dữ liệu nhập vào không hợp lệ. Vui lòng kiểm tra lại các trường bắt buộc." });
            }

            try
            {
                var summary = await _orderService.ProcessOrderCheckoutAsync(userId, checkoutInfo);

                return Json(new { success = true, message = "Đặt hàng và thanh toán thành công!", data = summary });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
    public class ShippingRequest
    {
        public string Address { get; set; } = string.Empty;
    }
}
