namespace ProjectSWD.Data.Entities
{
    public class CartItem
    {
        public string CustomerId { get; set; } = string.Empty;
        public Customer Customer { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public decimal Quantity { get; set; }
    }
}
