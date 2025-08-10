using Microsoft.AspNetCore.Mvc;
using DBManagement.Service;
using DBManagement.Models;
using DBManagement.Utils;

namespace DBManagement.Controller
{
    [ApiController]  // 标记为API控制器，自动处理模型绑定和验证
    [Route("api/[controller]")] // [controller] 是一个占位符，会被自动替换为控制器类名的前半部分，API路由前缀：api/dish
    public class DishController : ControllerBase
    {
        private readonly RestaurantDbContext _db;
        private readonly DishService _dishService;

        // 构造函数注入数据库上下文
        public DishController(RestaurantDbContext db)
        {
            _db = db;
            _dishService = new DishService(db);  // 初始化菜品服务层
        }

        // GET: /api/dish
        // 获取所有菜品列表（返回DTO对象，避免直接暴露实体）
        [HttpGet] // 标记为 HTTP GET 请求，对应路由：GET api/dish
        public ActionResult<ApiResponse<List<Dish>>> GetAllDishes() // ActionResult规定了HTTP响应类型、控制响应的格式（JSON）
        {
            var dishes = _dishService.GetAllDishes();
            return new ApiResponse<List<Dish>>(true, "获取成功", dishes);
        }

        // GET: /api/dish/{id}
        // 根据ID获取单个菜品（返回原始实体）
        [HttpGet("{id}")] // 带参数的 GET 请求
        public ActionResult<ApiResponse<Dish>> GetDishById(int id)
        {
            var dish = DbUtils.GetById<Dish>(_db, id);  // 通过工具类查询
            if (dish == null)
                return new ApiResponse<Dish>(false, "未找到该菜品");
            return new ApiResponse<Dish>(true, "获取成功", dish);
        }

        // POST: /api/dish
        // 添加新菜品，这个可能是商家端的方法
        [HttpPost]
        public ActionResult<ApiResponse> AddDish([FromBody] Dish dish) // [FromBody] 表示从请求体（JSON）绑定 Dish 对象
        {
            var (success, message) = _dishService.CreateDish(dish);
            return new ApiResponse(success, message);
        }

        // PUT: /api/dish/{id}
        // 更新菜品（指定ID并提供更新的字段）
        [HttpPut("{id}")]
        public ActionResult<ApiResponse> UpdateDish(int id, [FromBody] Dish dish)
        {
            // 使用工具类的通用更新方法，指定要更新的字段
            var (success, message) = DbUtils.UpdateEntity<Dish>(_db, id, entity =>
            {
                entity.DishName = dish.DishName; // dish是从请求体中绑定的对象（FromBody）
                entity.Price = dish.Price;
                entity.CategoryId = dish.CategoryId;
                entity.IsAvailable = dish.IsAvailable;
                entity.Description = dish.Description;
                entity.ImageUrl = dish.ImageUrl;
                entity.EstimatedTime = dish.EstimatedTime;
                entity.StoreId = dish.StoreId;
                entity.UpdateTime = DateTime.Now;
            });
            return new ApiResponse(success, message);
        }

        // DELETE: /api/dish/{id}
        // 删除菜品
        [HttpDelete("{id}")]
        public ActionResult<ApiResponse> DeleteDish(int id)
        {
            var (success, message) = DbUtils.DeleteById<Dish>(_db, id);
            return new ApiResponse(success, message);
        }
    }
}