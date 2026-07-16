using System.ComponentModel.DataAnnotations;

namespace ProjectSWD.DTOs
{
    public class ProductDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(255, ErrorMessage = "Product name cannot exceed 255 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters.")]
        public string ImageUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be a non-negative number.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock quantity is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Stock quantity must be a non-negative number.")]
        public decimal Quantity { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Please select a unit.")]
        public int UnitId { get; set; }

        // Các thuộc tính bổ sung hỗ trợ hiển thị tên liên kết trên UI
        public string CategoryName { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
    }
}