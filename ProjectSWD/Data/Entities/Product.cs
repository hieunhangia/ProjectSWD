using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjectSWD.Data.Entities
{
    public class Product
    {
        public int Id { get; set; }
        
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string ImageUrl { get; set; } = string.Empty;
        
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public int UnitId { get; set; }
        public Unit Unit { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        public ICollection<PromotionProduct> PromotionProducts { get; set; }
        public ICollection<RefundItem> RefundItems { get; set; }

        public ICollection<CartItem> CartItems { get; set; }
    }
}
