namespace ConsoleApp1.Models
{
    /// <summary>
    /// 桌台信息表实体类
    /// </summary>
    public class TableInfo
    {
        public int TableID { get; set; }
        public string TableNumber { get; set; } = string.Empty;
        public string? Area { get; set; }
        public int? Capacity { get; set; }
        public string Status { get; set; } = "空闲";
        public int? StoreID { get; set; }
    }
}