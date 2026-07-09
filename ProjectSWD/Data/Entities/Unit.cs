using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjectSWD.Data.Entities
{
    public class Unit
    {
        public int Id { get; set; }
        
        [MaxLength(255)]
        public string Name { get; set; }
        
        public bool AllowsDecimal { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}
