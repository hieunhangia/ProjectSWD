namespace ProjectSWD.Data.Entities
{
    public class OrderItem
    {
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public decimal Quantity { get; set; }
        public decimal Price { get; set; } 

        public Review Review { get; set; }
    }
}
