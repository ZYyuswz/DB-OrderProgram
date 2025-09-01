namespace ConsoleApp1.Models
{
    /// <summary>
    /// 订单详情表实体类
    /// </summary>
    public class OrderDetail
    {
        public int OrderDetailID { get; set; }
        public int OrderID { get; set; }
        public int DishID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        public string? SpecialRequests { get; set; }

        // 导航属性（用于显示关联信息）
        public string? DishName { get; set; }
    }
}