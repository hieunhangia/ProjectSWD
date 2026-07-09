using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectSWD.Data.Entities
{
    public class Bill
    {
        public int Id { get; set; }
        public DateTime PaymentTime { get; set; }
        
        [MaxLength(255)]
        public string ShopEmail { get; set; }
        
        [MaxLength(255)]
        public string ShopName { get; set; }
        
        [MaxLength(20)]
        public string ShopPhone { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }
    }
}
