using Microsoft.AspNetCore.Mvc;
using DBManagement.Service;
using DBManagement.Models;
using DBManagement.Utils;

namespace DBManagement.Controllers
{
    [ApiController]  // 标记为API控制器，自动处理模型绑定和验证
    [Route("api/[controller]")] // [controller] 是一个占位符，会被自动替换为控制器类名的前半部分，API路由前缀：api/order
    public class OrderController : ControllerBase
    {
        private readonly RestaurantDbContext _db;
        private readonly OrderService _orderService;

        // 构造函数注入数据库上下文
        public OrderController(RestaurantDbContext db)
        {
            _db = db;
            _orderService = new OrderService(db);  // 初始化订单服务层
        }

        // GET: /api/order
        // 获取所有订单列表（返回DTO对象，避免直接暴露实体）
        [HttpGet] // 标记为 HTTP GET 请求，对应路由：GET api/order
        public ActionResult<ApiResponse<List<Order>>> GetAllOrderes() // ActionResult规定了HTTP响应类型、控制响应的格式（JSON）
        {
            var orderes = _orderService.GetAllOrders();
            return new ApiResponse<List<Order>>(true, "获取成功", orderes);
        }

        // GET: /api/order/{id}
        // 根据ID获取单个订单（返回原始实体）
        [HttpGet("{id}")] // 带参数的 GET 请求
        public ActionResult<ApiResponse<Order>> GetorderById(int id)
        {
            var order = DbUtils.GetById<Order>(_db, id);  // 通过工具类查询
            if (order == null)
                return new ApiResponse<Order>(false, "未找到该订单");
            return new ApiResponse<Order>(true, "获取成功", order);
        }

        // POST: /api/order
        // 添加新订单
        [HttpPost]
        public ActionResult<ApiResponse<int>> AddOrder([FromBody] OrderRequest request)
        {
            var (success, message, id) = _orderService.CreateOrder(request.Order, request.OrderDetails);
            int order_id = id;
            return new ApiResponse<int>(success, message, order_id);
        }

        // PUT: /api/order/{customer_id}
        // 继续下单
        [HttpPut("{customer_id}")]
        public ActionResult<ApiResponse> Updateorder(int customer_id, [FromBody] List<OrderDetail> OrderDetails)
        {
            var (success, message) = _orderService.ContinueOrder(customer_id, OrderDetails);
            return new ApiResponse(success, message);
        }

        // 请求DTO，用于接收订单和订单详情
        public class OrderRequest
        {
            public Order Order { get; set; }
            public List<OrderDetail> OrderDetails { get; set; }
        }

        // DELETE: /api/order/{id}
        // 删除订单
        [HttpDelete("{id}")]
        public ActionResult<ApiResponse> Deleteorder(int id)
        {
            var (success, message) = _orderService.DeleteOrder(id);
            return new ApiResponse(success, message);
        }
    }
}
