using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B_DBManagement.Models
{
    /// <summary>
    /// 促销活动实体
    /// </summary>
    [Table("PROMOTION")]
    public class Promotion
    {
        [Key]
        [Column("PROMOTIONID")]
        public int PromotionId { get; set; }

        [Required]
        [Column("PROMOTIONNAME")]
        [StringLength(200)]
        public string PromotionName { get; set; }

        [Column("DISCOUNTTYPE")]
        [StringLength(20)]
        public string DiscountType { get; set; }

        [Column("DISCOUNTVALUE", TypeName = "decimal(10,2)")]
        public decimal? DiscountValue { get; set; }

        [Column("BEGINTIME")]
        public DateTime? BeginTime { get; set; }

        [Column("ENDTIME")]
        public DateTime? EndTime { get; set; }

        [Column("ISACTIVE")]
        [StringLength(1)]
        public string IsActive { get; set; }

        [Column("APPLICABLECATEGORIES")]
        [StringLength(500)]
        public string ApplicableCategories { get; set; }

        [Column("MINORDERAMOUNT", TypeName = "decimal(10,2)")]
        public decimal MinOrderAmount { get; set; } = 0;

        // 导航属性
        public virtual ICollection<DishPromotion> DishPromotions { get; set; } = new List<DishPromotion>();
    }
}