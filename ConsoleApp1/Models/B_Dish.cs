using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBManagement.Models
{
    /// <summary>
    /// 菜品实体
    /// </summary>
    [Table("DISH")]
    public class Dish
    {
        [Key]
        [Column("DISHID")]
        public int DishId { get; set; }

        [Required]
        [Column("DISHNAME")]
        [StringLength(200)]
        public string DishName { get; set; }

        [Required]
        [Column("PRICE", TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [ForeignKey("Category")]
        [Column("CATEGORYID")]
        public int CategoryId { get; set; }

        [Required]
        [Column("ISAVAILABLE")]
        [StringLength(1)]
        public string IsAvailable { get; set; } = "Y";

        [Column("DESCRIPTION")]
        [StringLength(1000)]
        public string Description { get; set; }

        [Column("IMAGEURL")]
        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [Column("ESTIMATEDTIME")]
        public int? EstimatedTime { get; set; }

        [Required]
        [Column("STOREID")]
        public int StoreId { get; set; }

        [Column("CREATETIME")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column("UPDATETIME")]
        public DateTime UpdateTime { get; set; } = DateTime.Now;

        // 导航属性
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public virtual ICollection<DishPromotion> DishPromotions { get; set; } = new List<DishPromotion>();
    }
}