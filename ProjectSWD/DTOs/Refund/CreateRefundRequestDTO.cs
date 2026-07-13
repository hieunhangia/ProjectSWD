using System.ComponentModel.DataAnnotations;

namespace ProjectSWD.DTOs.Refund
{
    public class CreateRefundRequestDTO
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lý do hoàn tiền là bắt buộc.")]
        [MaxLength(1000, ErrorMessage = "Lý do hoàn tiền không được vượt quá 1000 ký tự.")]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public int ProductId { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        public decimal Quantity { get; set; }
    }
}
