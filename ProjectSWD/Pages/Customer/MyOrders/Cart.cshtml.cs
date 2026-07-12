using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectSWD.Data;
using ProjectSWD.Data.Entities;
using ProjectSWD.Services.Customer;

namespace ProjectSWD.Controllers.Customer
{
    [Authorize]
    [Route("Cart")]
    public class Cart(ICartService cartService, UserManager<IdentityUser> userManager, ApplicationDbContext context) : Controller
    {
        private readonly ICartService _cartService = cartService;
        private readonly UserManager<IdentityUser> _userManager = userManager;
        private readonly ApplicationDbContext _context = context;

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var cartItems = await _cartService.GetCartByCustomerIdAsync(userId);
            return View("~/Pages/Customer/MyOrders/Cart.cshtml", cartItems);
        }

        [HttpPost("AddToCart")]
        public async Task<IActionResult> AddToCart(int productId, decimal quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            try
            {
                await _cartService.AddToCartAsync(userId, productId, quantity);
                TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            var referer = Request.Headers.Referer.ToString();
            return Redirect(string.IsNullOrEmpty(referer) ? "/" : referer);
        }

        [HttpPost("UpdateQuantity")]
        public async Task<IActionResult> UpdateQuantity(int productId, decimal quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            try
            {
                await _cartService.UpdateQuantityAsync(userId, productId, quantity);
                TempData["SuccessMessage"] = "Cập nhật số lượng thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("RemoveItem")]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            try
            {
                await _cartService.RemoveFromCartAsync(userId, productId);
                TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("AddDummyToCart")]
        public async Task<IActionResult> AddDummyToCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var product = await _context.Products.FirstOrDefaultAsync();
            if (product == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm nào trong hệ thống để tạo mẫu.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _cartService.AddToCartAsync(userId, product.Id, 1);
                TempData["SuccessMessage"] = $"Đã thêm sản phẩm mẫu '{product.Name}' vào giỏ hàng!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
