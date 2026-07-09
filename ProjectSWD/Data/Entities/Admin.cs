using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectSWD.Data.Entities
{
    public class Admin
    {
        [Key, ForeignKey("User")]
        public string Id { get; set; }
        
        public IdentityUser User { get; set; }
    }
}
