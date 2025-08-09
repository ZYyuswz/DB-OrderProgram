using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B_DBManagement.Models
{
    /// <summary>
    /// 菜品促销关联实体
    /// </summary>
    [Table("DISHPROMOTION")]
    public class DishPromotion
    {
        [Key]
        [Column("DISHPROMOTIONID")]
        public int DishPromotionId { get; set; }

        [ForeignKey("Dish")]
        [Column("DISHID")]
        public int DishId { get; set; }

        [ForeignKey("Promotion")]
        [Column("PROMOTIONID")]
        public int PromotionId { get; set; }

        // 导航属性
        public virtual Dish Dish { get; set; }
        public virtual Promotion Promotion { get; set; }
    }
}