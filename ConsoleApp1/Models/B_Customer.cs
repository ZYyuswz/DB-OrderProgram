using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBManagement.Models
{
    /// <summary>
    /// 客户实体
    /// </summary>
    [Table("CUSTOMER")]
    public class Customer
    {
        [Key]
        [Column("CUSTOMERID")]
        public long CustomerId { get; set; }

        [Required]
        [Column("CUSTOMERNAME")]
        [StringLength(100)]
        public string CustomerName { get; set; }

        [Column("PHONE")]
        [StringLength(20)]
        public string Phone { get; set; }

        [Column("EMAIL")]
        [StringLength(100)]
        public string Email { get; set; }

        [Column("BIRTHDAY")]
        public DateTime? Birthday { get; set; }

        [Column("GENDER")]
        [StringLength(1)]
        public string Gender { get; set; }

        [Column("REGISTERTIME")]
        public DateTime RegisterTime { get; set; } = DateTime.Now;

        [Column("TOTALCONSUMPTION", TypeName = "decimal(12,2)")]
        public decimal TotalConsumption { get; set; } = 0;

        [Column("VIPLEVEL")]
        public int VIPLevel { get; set; }

        [Column("VIPPOINTS")]
        public int VIPPoints { get; set; } = 0;

        [Column("STATUS")]
        [StringLength(20)]
        public string Status { get; set; }

        // 导航属性
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}