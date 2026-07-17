using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectSWD.DTOs.Review;
using ProjectSWD.Services.Customer;

namespace ProjectSWD.Pages.Customer.MyOrders
{
    [Authorize]
    public class RateProductModel : PageModel
    {
        private readonly ReviewService _reviewService;
        private readonly UserManager<IdentityUser> _userManager;

        public RateProductModel(ReviewService reviewService, UserManager<IdentityUser> userManager)
        {
            _reviewService = reviewService;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)] public int OrderId { get; set; }

        [BindProperty(SupportsGet = true)] public int ProductId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng chọn số sao đánh giá.")]
        [Range(1, 5, ErrorMessage = "Điểm đánh giá phải từ 1 đến 5.")]
        public int Rating { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá.")]
        [StringLength(2000, ErrorMessage = "Nội dung đánh giá không được vượt quá 2000 ký tự.")]
        public string Content { get; set; }

        public string ProductName { get; set; }
        public string ProductImage { get; set; }

        // This will hold the business validation error to disable the form
        public string? EligibilityError { get; set; }

        public async Task<IActionResult> OnGetAsync(int orderId, int productId)
        {
            OrderId = orderId;
            ProductId = productId;
            await InitPageDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await InitPageDataAsync();

            // If eligibility error exists, don't allow post
            if (!string.IsNullOrEmpty(EligibilityError))
            {
                ModelState.AddModelError(string.Empty, EligibilityError);
                return Page();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Challenge();
            }

            try
            {
                var request = new SubmitFeedbackRequestDto
                {
                    CustomerId = userId,
                    OrderId = OrderId,
                    ProductId = ProductId,
                    Rating = Rating,
                    Content = Content
                };
                await _reviewService.SubmitFeedbackAsync(request);
                TempData["SuccessMessage"] = "Cảm ơn bạn đã đánh giá!";
                // In a real app, redirecting to PRG pattern is better. Here we just return Page with TempData.
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
                EligibilityError = await _reviewService.ValidateFeedbackEligibilityAsync(userId, OrderId, ProductId);
            }
            else
            {
                EligibilityError = "Bạn cần đăng nhập để thực hiện hành động này.";
            }

            var productInfo = await _reviewService.GetProductInfoForRatingAsync(ProductId);
            if (productInfo != null)
            {
                ProductName = productInfo.Name;
                ProductImage = productInfo.ImageUrl;
            }
        }
    }
}