using System.ComponentModel.DataAnnotations;

namespace ProjectSWD.DTOs
{
    public class OrderCheckoutDTO
    {
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(255, ErrorMessage = "Full name cannot exceed 255 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Shipping address is required.")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Payment method is required.")]
        public string PaymentMethod { get; set; } = "COD"; // E.g., COD, CreditCard, BankTransfer

        public decimal ShippingFee { get; set; }
    }
}
