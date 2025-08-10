using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBManagement.Models
{
    /// <summary>
    /// 桌台信息实体
    /// </summary>
    [Table("TABLEINFO")]
    public class TableInfo
    {
        [Key]
        [Column("TABLEID")]
        public int TableId { get; set; }

        [Required]
        [Column("TABLENUMBER")]
        [StringLength(20)]
        public string TableNumber { get; set; }

        [Column("AREA")]
        [StringLength(100)]
        public string Area { get; set; }

        [Column("CAPACITY")]
        public int? Capacity { get; set; }

        [Column("STATUS")]
        [StringLength(20)]
        public string Status { get; set; } = "空闲";

        [ForeignKey("Store")]
        [Column("STOREID")]
        public int StoreId { get; set; }
    }
}