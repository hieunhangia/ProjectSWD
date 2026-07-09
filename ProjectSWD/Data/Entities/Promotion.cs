using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjectSWD.Data.Entities
{
    public class Promotion
    {
        public int Id { get; set; }
        
        [MaxLength(255)]
        public string Name { get; set; }
        
        [MaxLength(500)]
        public string Description { get; set; }
        
        public decimal? FixedAmount { get; set; }
        public decimal? Percentage { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int? UsageLimit { get; set; }
        public decimal? MinimumOrder { get; set; }

        public ICollection<PromotionProduct> PromotionProducts { get; set; }
    }
}
