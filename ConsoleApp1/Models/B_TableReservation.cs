using DBManagement.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace DB_Prog.Models
{
    [Table("TABLERESERVATION")]
    public class TableReservation
    {
        [Key]
        [Column("RESERVATIONID")]
        public int ReservationID { get; set; }

        [ForeignKey("TableInfo")]
        [Column("TABLEID")]
        public int TableID { get; set; }

        [ForeignKey("Customer")]
        [Column("CUSTOMERID")]
        public int CustomerID { get; set; }

        /*
        [Column("CUSTOMERNAME")]
        [StringLength(100)]
        public string CustomerName { get; set; }

        [Column("CONTACTPHONE")]
        [StringLength(20)]
        public string ContactPhone { get; set; }
        */

        [Column("PARTYSIZE")]
        public int PartySize { get; set; }

        [Column("RESERVATIONTIME")]
        public DateTime ReservationTime { get; set; }

        [Column("EXPECTEDDURATION")]
        public int ExpectedDuration { get; set; }

        [Column("STATUS")]
        [StringLength(20)]
        public string Status { get; set; }

        [Column("NOTES")]
        [StringLength(500)]
        public string? Notes { get; set; }

        [Column("CREATETIME")]
        public DateTime CreateTime { get; set; }

        [Column("MEALTIME")]
        public int MealTime { get; set; } // 0:中午, 1:晚上

        // 导航属性
        public virtual TableInfo TableInfo { get; set; }
        public virtual Customer Customer { get; set; }
    }
}
