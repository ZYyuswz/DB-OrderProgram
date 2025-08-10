using System;

namespace DB_Prog.Models
{
    /// <summary>
    /// 桌台预约请求DTO，仅用于接收前端提交的数据
    /// </summary>
    public class TableReservationRequest
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; }
        public string ContactPhone { get; set; }
        public int PartySize { get; set; }
        public DateTime ReservationTime { get; set; }
        public int ExpectedDuration { get; set; }
        public string? Notes { get; set; }
        public int MealTime { get; set; } // 0:中午, 1:晚上
    }
}