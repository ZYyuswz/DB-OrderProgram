// DishService 是一个服务层类，主要负责处理与 “Dish” 相关的业务逻辑和数据处理
// 通俗讲就是DishController需要的方法
using DBManagement.Models;
using DBManagement.Utils;
namespace DBManagement.Service
{
    public class DishService
    {
        private readonly RestaurantDbContext _db;

        // 通过构造函数注入数据库上下文
        public DishService(RestaurantDbContext db)
        {
            _db = db;
        }

        // 获取所有可用菜品并转换为Dish列表
        public List<Dish> GetAllDishes()
        {
            // 从数据库查询菜品实体
            var dishes = _db.Dishes
                .Select(d => d) // 直接返回实体，无需显式映射
                .ToList(); // 执行查询并转换为列表
            return dishes;
        }

        // 创建菜品
        public (bool success, string message) CreateDish(Dish dish)
        {
            try
            {
                // 生成菜品ID
                dish.DishId = GenerateDishId();
                dish.IsAvailable = "Y";
                // 设置菜品基本信息
                dish.CreateTime = DateTime.Now;
                dish.UpdateTime = DateTime.Now;
                // 添加dish到数据库
                DbUtils.AddEntity(_db, dish); // 这里会自动处理菜品详情的添加，因为已设置导航属性

                return (true, "菜品创建成功");
            }
            catch (Exception ex)
            {
                return (false, $"菜品创建失败: {ex.Message}");
            }
        }
        private int GenerateDishId()
        {
            // 从数据库查询当前最大DishId
            int maxId = _db.Dishes.Max(d => (int?)d.DishId) ?? 0;
            return Interlocked.Increment(ref maxId);
        }
    } 
}