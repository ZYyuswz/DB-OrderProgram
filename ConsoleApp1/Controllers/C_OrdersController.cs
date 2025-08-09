using Microsoft.AspNetCore.Mvc;
using ConsoleApp1.Services;
using ConsoleApp1.Models;

namespace ConsoleApp1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(OrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// 获取指定客户的订单列表
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页条数</param>
        /// <returns>订单列表</returns>
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<List<Orders>>> GetCustomerOrders(
            int customerId, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation($"获取客户 {customerId} 的订单列表，页码: {page}，每页: {pageSize}");
                
                var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId);
                
                // 分页处理
                var pagedOrders = orders
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Ok(pagedOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取客户 {customerId} 订单列表失败");
                return StatusCode(500, new { message = "获取订单列表失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 获取所有订单（分页）
        /// </summary>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页条数</param>
        /// <returns>订单列表</returns>
        [HttpGet]
        public async Task<ActionResult<List<Orders>>> GetAllOrders(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation($"获取所有订单，页码: {page}，每页: {pageSize}");
                
                var orders = await _orderService.GetAllOrdersAsync(pageSize, page);
                
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单列表失败");
                return StatusCode(500, new { message = "获取订单列表失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <returns>订单详情</returns>
        [HttpGet("{orderId}/details")]
        public async Task<ActionResult<List<OrderDetail>>> GetOrderDetails(int orderId)
        {
            try
            {
                _logger.LogInformation($"获取订单 {orderId} 的详情");
                
                var details = await _orderService.GetOrderDetailsAsync(orderId);
                
                return Ok(details);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取订单 {orderId} 详情失败");
                return StatusCode(500, new { message = "获取订单详情失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 创建新订单
        /// </summary>
        /// <param name="order">订单信息</param>
        /// <returns>创建的订单ID</returns>
        [HttpPost]
        public async Task<ActionResult<int>> CreateOrder([FromBody] Orders order)
        {
            try
            {
                _logger.LogInformation("创建新订单");
                
                var orderId = await _orderService.CreateOrderAsync(order);
                
                if (orderId.HasValue)
                {
                    return Ok(new { orderId = orderId.Value, message = "订单创建成功" });
                }
                else
                {
                    return BadRequest(new { message = "订单创建失败" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建订单失败");
                return StatusCode(500, new { message = "创建订单失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 更新订单状态
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="status">新状态</param>
        /// <returns>更新结果</returns>
        [HttpPut("{orderId}/status")]
        public async Task<ActionResult> UpdateOrderStatus(int orderId, [FromBody] string status)
        {
            try
            {
                _logger.LogInformation($"更新订单 {orderId} 状态为 {status}");
                
                var success = await _orderService.UpdateOrderStatusAsync(orderId, status);
                
                if (success)
                {
                    return Ok(new { message = "订单状态更新成功" });
                }
                else
                {
                    return BadRequest(new { message = "订单状态更新失败" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新订单 {orderId} 状态失败");
                return StatusCode(500, new { message = "更新订单状态失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 删除订单
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("{orderId}")]
        public async Task<ActionResult> DeleteOrder(int orderId)
        {
            try
            {
                _logger.LogInformation($"删除订单 {orderId}");
                
                var success = await _orderService.DeleteOrderAsync(orderId);
                
                if (success)
                {
                    return Ok(new { message = "订单删除成功" });
                }
                else
                {
                    return BadRequest(new { message = "订单删除失败" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除订单 {orderId} 失败");
                return StatusCode(500, new { message = "删除订单失败", error = ex.Message });
            }
        }
    }
}