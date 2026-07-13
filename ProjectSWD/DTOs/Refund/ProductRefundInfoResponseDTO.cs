namespace ProjectSWD.DTOs.Refund
{
    public class ProductRefundInfoResponseDTO
    {
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public decimal MaxQuantity { get; set; }
        public decimal Price { get; set; }
    }
}
