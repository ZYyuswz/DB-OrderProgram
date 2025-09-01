namespace ConsoleApp1.Models
{
    /// <summary>
    /// 订单主表实体类
    /// </summary>
    public class Orders
    {
        public int OrderID { get; set; }
        public DateTime OrderTime { get; set; }
        public int? TableID { get; set; }
        public int? CustomerID { get; set; }
        public decimal TotalPrice { get; set; }
        public string OrderStatus { get; set; } = "待处理";
        public int? StaffID { get; set; }
        public int? StoreID { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }

        // 导航属性（用于显示关联信息）
        public string? CustomerName { get; set; }
        public string? StoreName { get; set; }
        public string? StaffName { get; set; }
        public string? TableNumber { get; set; }
    }
}