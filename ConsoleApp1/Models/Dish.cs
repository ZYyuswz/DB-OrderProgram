namespace ConsoleApp1.Models
{
    /// <summary>
    /// 菜品实体类
    /// </summary>
    public class Dish
    {
        public int DishID { get; set; }
        public string DishName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int? CategoryID { get; set; }
        public char IsAvailable { get; set; } = 'Y';
        public string? Description { get; set; }
        public string? ImageURL { get; set; }
        public int? EstimatedTime { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        
        // 导航属性（用于显示关联信息）
        public string? CategoryName { get; set; }
    }
}