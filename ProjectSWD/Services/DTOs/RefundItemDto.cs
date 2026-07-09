using System.ComponentModel.DataAnnotations;

namespace ProjectSWD.Services.DTOs
{
    public class RefundItemDto
    {
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Quantity { get; set; }
    }
}
