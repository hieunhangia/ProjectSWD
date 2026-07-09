using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectSWD.Services.Customer;

namespace ProjectSWD.Pages.Customer.MyOrders
{
    [Authorize]
    public class RequestRefundModel : PageModel
    {
        private readonly RefundService _refundService;

        public RequestRefundModel(RefundService refundService)
        {
            _refundService = refundService;
        }

        [BindProperty(SupportsGet = true)] public int OrderId { get; set; }
        [BindProperty(SupportsGet = true)] public int ProductId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please provide a reason for the refund.")]
        [MaxLength(1000)]
        public string Reason { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please enter the quantity.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public decimal Quantity { get; set; }

        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal MaxQuantity { get; set; }
        public decimal Price { get; set; }

        public string? EligibilityError { get; set; }

        public async Task<IActionResult> OnGetAsync(int orderId, int productId)
        {
            OrderId = orderId;
            ProductId = productId;
            Quantity = 1; // Default
            await InitPageDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await InitPageDataAsync();

            if (!string.IsNullOrEmpty(EligibilityError))
            {
                ModelState.AddModelError(string.Empty, EligibilityError);
                return Page();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (Quantity > MaxQuantity)
            {
                ModelState.AddModelError("Quantity", $"Quantity cannot exceed {MaxQuantity}.");
                return Page();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Challenge();
            }

            try
            {
                await _refundService.CreateRefundRequestAsync(OrderId, userId, Reason, ProductId, Quantity);
                TempData["SuccessMessage"] = "Refund request submitted successfully.";
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
        }

        private async Task InitPageDataAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                EligibilityError = await _refundService.ValidateRefundEligibilityAsync(userId, OrderId, ProductId);

                if (string.IsNullOrEmpty(EligibilityError))
                {
                    var productInfo = await _refundService.GetProductInfoForRefundAsync(userId, OrderId, ProductId);
                    if (productInfo.HasValue)
                    {
                        ProductName = productInfo.Value.Name;
                        ProductImage = productInfo.Value.ImageUrl;
                        MaxQuantity = productInfo.Value.MaxQuantity;
                        Price = productInfo.Value.Price;
                    }
                }
            }
            else
            {
                EligibilityError = "You need to log in to perform this action.";
            }
        }
    }
}
