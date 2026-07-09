using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectSWD.Data.Entities
{
    public class Review
    {
        public int Id { get; set; }
        
        [MaxLength(2000)]
        public string Content { get; set; }
        
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }

        public string CustomerId { get; set; }
        public Customer Customer { get; set; }


        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public OrderItem OrderItem { get; set; }
    }
}
