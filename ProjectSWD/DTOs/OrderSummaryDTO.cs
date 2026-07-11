using System;
using System.Collections.Generic;

namespace ProjectSWD.DTOs
{
    public class OrderSummaryDTO
    {
        public int OrderId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public List<CartItemDTO> Items { get; set; } = new();
        public decimal ProductTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime OrderTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
