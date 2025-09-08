namespace ConsoleApp1.Models
{
    /// <summary>
    /// 客户实体类
    /// </summary>
    public class Customer
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateTime? Birthday { get; set; }
        public char? Gender { get; set; }
        public DateTime RegisterTime { get; set; }
        public DateTime? LastVisitTime { get; set; }
        public decimal TotalConsumption { get; set; }
        public int? VIPLevel { get; set; }
        public int VIPPoints { get; set; }
        public string Status { get; set; } = "正常";
        public int? PreferredStoreID { get; set; }
    }
}