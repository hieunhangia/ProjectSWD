using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjectSWD.Data.Entities
{
    public class Promotion
    {
        public int Id { get; set; }
        
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        /// <summary>True = Percentage, False = FixedAmount</summary>
        public bool IsPercentage { get; set; }
        
        public decimal? FixedAmount { get; set; }
        public decimal? Percentage { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int? UsageLimit { get; set; }
        
        /// <summary>Giới hạn số lần sử dụng cho mỗi user (per-user frequency cap)</summary>
        public int? PerUserLimit { get; set; }
        
        public decimal? MinimumOrder { get; set; }

        /// <summary>Đã bị hủy khẩn cấp (emergency terminated)</summary>
        public bool IsTerminated { get; set; } = false;

        public ICollection<PromotionProduct> PromotionProducts { get; set; } = new List<PromotionProduct>();
    }
}
