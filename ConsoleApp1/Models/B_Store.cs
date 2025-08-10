using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBManagement.Models
{
    /// <summary>
    /// 门店实体
    /// </summary>
    [Table("STORE")]
    public class Store
    {
        [Key]
        [Column("STOREID")]
        public int StoreId { get; set; }

        [Required]
        [Column("STORENAME")]
        [StringLength(200)]
        public string StoreName { get; set; }

        [Column("ADDRESS")]
        [StringLength(500)]
        public string Address { get; set; }

        [Column("PHONE")]
        [StringLength(20)]
        public string Phone { get; set; }

        [ForeignKey("Manager")]
        [Column("MANAGERID")]
        public int? ManagerId { get; set; }

        [Column("OPENINGHOURS")]
        [StringLength(200)]
        public string OpeningHours { get; set; }

        [Column("STATUS")]
        [StringLength(20)]
        public string Status { get; set; }

        [Column("OPENDATE")]
        public DateTime? OpenDate { get; set; }

        [ForeignKey("Region")]
        [Column("REGIONID")]
        public int? RegionId { get; set; }

        [Column("STORESIZE", TypeName = "decimal(10,2)")]
        public decimal? StoreSize { get; set; }

        [Column("MONTHLYRENT", TypeName = "decimal(12,2)")]
        public decimal? MonthlyRent { get; set; }

        [Column("CREATETIME")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Column("UPDATETIME")]
        public DateTime UpdateTime { get; set; } = DateTime.Now;
    }
}