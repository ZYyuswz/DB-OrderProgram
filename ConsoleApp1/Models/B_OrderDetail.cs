using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B_DBManagement.Models
{
    /// <summary>
    /// 订单详情实体
    /// </summary>
    [Table("ORDERDETAIL")]
    public class OrderDetail
    {
        [Key]
        [Column("ORDERDETAILID")]
        public int OrderDetailId { get; set; }

        [ForeignKey("Order")]
        [Column("ORDERID")]
        public int OrderId { get; set; }

        [ForeignKey("Dish")]
        [Column("DISHID")]
        public int DishId { get; set; }

        [Column("QUANTITY")]
        public int Quantity { get; set; }

        [Column("UNITPRICE", TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        [Column("SUBTOTAL", TypeName = "decimal(12,2)")]
        public decimal Subtotal { get; set; }

        [Column("SPECIALREQUESTS")]
        [StringLength(500)]
        public string? SpecialRequests { get; set; } // 可空

        // 导航属性
        public virtual Order? Order { get; set; }
        public virtual Dish? Dish { get; set; }
    }
}