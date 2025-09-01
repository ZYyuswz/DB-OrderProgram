using System.ComponentModel.DataAnnotations;

namespace ConsoleApp1.Models
{
    /// <summary>
    /// 积分记录模型
    /// </summary>
    public class PointsRecord
    {
        [Key]
        public int RecordID { get; set; }
        
        public int? CustomerID { get; set; }
        
        public int? OrderID { get; set; }
        
        public int PointsChange { get; set; }
        
        public string RecordType { get; set; } = string.Empty;
        
        public DateTime RecordTime { get; set; }
        
        public string? Description { get; set; }
        
        // 扩展字段，用于前端显示
        public string? CustomerName { get; set; }
        
        public decimal? OrderAmount { get; set; }
        
        public string? OrderTime { get; set; }
        
        public string? StoreName { get; set; }
    }
} 