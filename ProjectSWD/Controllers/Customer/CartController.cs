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
    public class CartController(ICartService cartService, UserManager<IdentityUser> userManager, ApplicationDbContext context) : Controller
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
            return View("~/Views/Cart/Index.cshtml", cartItems);
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
    }
}
