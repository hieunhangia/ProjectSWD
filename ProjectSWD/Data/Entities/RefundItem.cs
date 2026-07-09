namespace ProjectSWD.Data.Entities
{
    public class RefundItem
    {
        public int RefundId { get; set; }
        public Refund Refund { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
