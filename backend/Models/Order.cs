using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBManagement.Models
{
    /// <summary>
    /// 订单主表实体
    /// </summary>
    [Table("ORDERS")]
    public class Order
    {
        [Key]
        [Column("ORDERID")]
        public int OrderId { get; set; }

        [Column("ORDERTIME")]
        public DateTime OrderTime { get; set; } = DateTime.Now;

        [Column("TABLEID")]
        public int TableId { get; set; }

        [ForeignKey("Customer")]
        [Column("CUSTOMERID")]
        public int? CustomerId { get; set; }

        [Column("TOTALPRICE", TypeName = "decimal(12,2)")]
        public decimal TotalPrice { get; set; } = 0;

        [Column("ORDERSTATUS")]
        [StringLength(20)]
        public string OrderStatus { get; set; } = "待处理"; // 默认状态

        [ForeignKey("Store")]
        [Column("STOREID")]
        public int StoreId { get; set; }

        [Column("CREATETIME")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column("UPDATETIME")]
        public DateTime UpdateTime { get; set; } = DateTime.Now;

        // 导航属性
        public virtual Store? Store { get; set; }
        public virtual ICollection<OrderDetail>? OrderDetails { get; set; } = new List<OrderDetail>();
    }
}