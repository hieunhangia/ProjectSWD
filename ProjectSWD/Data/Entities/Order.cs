using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using ProjectSWD.Data.Enums;

namespace ProjectSWD.Data.Entities
{
    public class Order
    {
        public int Id { get; set; }
        
        [MaxLength(255)]
        public string FullName { get; set; }
        
        [MaxLength(20)]
        public string PhoneNumber { get; set; }
        
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;
        
        public DateTime Time { get; set; }
        public decimal TotalPrice { get; set; }
        
        public OrderStatus Status { get; set; }
 
        public string CustomerId { get; set; }
        public Customer Customer { get; set; }

        public string? StaffId { get; set; } 
        public Staff Staff { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
        public Shipment Shipment { get; set; }
        public Bill Bill { get; set; }
        public ICollection<Refund> Refunds { get; set; }
    }
}
