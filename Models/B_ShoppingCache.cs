using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace DBManagement.Models
{
    /// <summary>
    /// 购物缓存
    /// </summary>
    [Table("SHOPPINGCACHE")]
    public class ShoppingCache
    {
        [Key]
        [Column("CACHEID")]
        public int CacheId { get; set; }

        [Column("TABLEID")]
        public int TableId { get; set; }

        [Column("DISHID")]
        public int DishId { get; set; }

        [Column("QUANTITY")]
        public int Quantity { get; set; } = 1;

        [Column("STATUS")]
        [StringLength(20)]
        public string Status { get; set; } = "PENDING";
    }
}
