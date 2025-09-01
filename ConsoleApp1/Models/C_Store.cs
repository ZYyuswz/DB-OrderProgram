namespace ConsoleApp1.Models
{
    /// <summary>
    /// 门店实体类
    /// </summary>
    public class Store
    {
        public int StoreID { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public int? ManagerID { get; set; }
        public string? OpeningHours { get; set; }
        public string Status { get; set; } = "营业中";
        public DateTime? OpenDate { get; set; }
        public int? RegionID { get; set; }
        public decimal? StoreSize { get; set; }
        public decimal? MonthlyRent { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}