using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectSWD.Data.Entities
{
    public class CartItem
    {
        public string CustomerId { get; set; }
        public Customer Customer { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public decimal Quantity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
