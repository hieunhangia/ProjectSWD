using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectSWD.Services.Customer;
using ProjectSWD.DTOs.Refund;

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
        [Required(ErrorMessage = "Vui lòng nhập lý do hoàn tiền.")]
        [MaxLength(1000)]
        public string Reason { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập số lượng.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
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
                ModelState.AddModelError("Quantity", $"Số lượng không thể vượt quá {MaxQuantity}.");
                return Page();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Challenge();
            }

            try
            {
                var requestDto = new CreateRefundRequestDTO
                {
                    OrderId = OrderId,
                    CustomerId = userId,
                    Reason = Reason,
                    ProductId = ProductId,
                    Quantity = Quantity
                };
                await _refundService.CreateRefundRequestAsync(requestDto);
                TempData["SuccessMessage"] = "Gửi yêu cầu hoàn tiền thành công.";
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
                var eligibilityResponse = await _refundService.ValidateRefundEligibilityAsync(userId, OrderId, ProductId);
                EligibilityError = eligibilityResponse.ErrorMessage;

                if (string.IsNullOrEmpty(EligibilityError))
                {
                    var productInfo = await _refundService.GetProductInfoForRefundAsync(userId, OrderId, ProductId);
                    if (productInfo != null)
                    {
                        ProductName = productInfo.Name;
                        ProductImage = productInfo.ImageUrl;
                        MaxQuantity = productInfo.MaxQuantity;
                        Price = productInfo.Price;
                    }
                }
            }
            else
            {
                EligibilityError = "Bạn cần đăng nhập để thực hiện hành động này.";
            }
        }
    }
}
