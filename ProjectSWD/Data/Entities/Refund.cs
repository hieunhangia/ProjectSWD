using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ProjectSWD.Data.Enums;

namespace ProjectSWD.Data.Entities
{
    public class Refund
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        
        [MaxLength(1000)]
        public string Reason { get; set; }

        public RefundStatus Status { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public ICollection<RefundItem> RefundItems { get; set; }
    }
}
