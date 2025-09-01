using ConsoleApp1.Services;
using DBManagement.Models;
using DBManagement.Service;
using DBManagement.Utils;
using Microsoft.AspNetCore.Mvc;

namespace DBManagement.Controllers
{
    [ApiController]  // 标记为API控制器，自动处理模型绑定和验证
    [Route("api/[controller]")] // [controller] 是一个占位符，会被自动替换为控制器类名的前半部分，API路由前缀：api/cache
    public class CacheController : ControllerBase
    {
        private readonly RestaurantDbContext _db;
        private readonly CacheService _cacheService;

        // 构造函数注入数据库上下文
        public CacheController(RestaurantDbContext db)
        {
            _db = db;
            _cacheService = new CacheService(db);  // 初始化订单服务层
        }

        // POST: /api/cache
        // 添加新缓存
        [HttpPost]
        public ActionResult<ApiResponse<int>> AddCache([FromBody] ShoppingCache request)
        {
            var (success, message, id) = _cacheService.CreateCache(request);
            int cache_id = id;
            return new ApiResponse<int>(success, message, cache_id);
        }

        // DELETE: /api/cache/{id}
        // 删除缓存
        [HttpDelete("{id}")]
        public ActionResult<ApiResponse> DeleteCache(int id)
        {
            var (success, message) = _cacheService.DeleteCache(id);
            return new ApiResponse(success, message);
        }

        // GET: /api/cache/{tableId}
        // 根据餐桌ID获取所有缓存
        [HttpGet("{tableId}")] // 注意参数名与方法参数一致
        public ActionResult<ApiResponse<List<ShoppingCache>>> GetCacheByTableId(int tableId)
        {
            var caches = _cacheService.ReturnCacheByTable(tableId);

            // 列表不会为null，只会为空列表，所以判断数量
            if (caches.Count == 0)
                return new ApiResponse<List<ShoppingCache>>(false, "未找到相关缓存");

            return new ApiResponse<List<ShoppingCache>>(true, "获取成功", caches);
        }

        // DELETE: /api/table/tableId
        // 根据桌号删除所有缓存
        [HttpDelete("table/{tableId}")]
        public ActionResult<ApiResponse> DeleteAllCacheByTableId(int tableId)
        {
            var (success, message) = _cacheService.DeleteAllCacheByTableId(tableId);
            return new ApiResponse(success, message);
        }
    }
}