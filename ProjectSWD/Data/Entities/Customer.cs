using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectSWD.Data.Entities
{
    public class Customer
    {
        [Key, ForeignKey("User")]
        public string Id { get; set; } = string.Empty;
        
        [MaxLength(255)]
        public string FullName { get; set; } = string.Empty;
        
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        public IdentityUser User { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
