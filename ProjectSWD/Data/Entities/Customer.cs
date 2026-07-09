using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectSWD.Data.Entities
{
    public class Customer
    {
        [Key, ForeignKey("User")]
        public string Id { get; set; }
        
        [MaxLength(255)]
        public string FullName { get; set; }
        
        [MaxLength(255)]
        public string Email { get; set; }
        
        [MaxLength(20)]
        public string Phone { get; set; }
        
        [MaxLength(500)]
        public string Address { get; set; }

        public IdentityUser User { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}
