using Microsoft.AspNetCore.Mvc;
using ConsoleApp1.Services;
using ConsoleApp1.Models;

namespace ConsoleApp1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PointsController : ControllerBase
    {
        private readonly PointsService _pointsService;
        private readonly ILogger<PointsController> _logger;

        public PointsController(PointsService pointsService, ILogger<PointsController> logger)
        {
            _pointsService = pointsService;
            _logger = logger;
        }

        /// <summary>
        /// 获取客户的积分记录
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页条数</param>
        /// <returns>积分记录列表</returns>
        [HttpGet("customer/{customerId}/records")]
        public async Task<ActionResult<List<PointsRecord>>> GetCustomerPointsRecords(
            int customerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation($"获取客户 {customerId} 的积分记录，页码: {page}，每页: {pageSize}");

                var records = await _pointsService.GetCustomerPointsRecordsAsync(customerId, page, pageSize);

                return Ok(records);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取客户 {customerId} 积分记录失败");
                return StatusCode(500, new { message = "获取积分记录失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 获取客户当前积分余额
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <returns>积分余额</returns>
        [HttpGet("customer/{customerId}/balance")]
        public async Task<ActionResult<int>> GetCustomerPointsBalance(int customerId)
        {
            try
            {
                _logger.LogInformation($"获取客户 {customerId} 的积分余额");

                var balance = await _pointsService.GetCustomerPointsBalanceAsync(customerId);

                return Ok(new { customerId = customerId, pointsBalance = balance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取客户 {customerId} 积分余额失败");
                return StatusCode(500, new { message = "获取积分余额失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 添加积分记录
        /// </summary>
        /// <param name="record">积分记录</param>
        /// <returns>添加结果</returns>
        [HttpPost("records")]
        public async Task<ActionResult> AddPointsRecord([FromBody] PointsRecord record)
        {
            try
            {
                _logger.LogInformation("添加积分记录");

                var success = await _pointsService.AddPointsRecordAsync(record);

                if (success)
                {
                    return Ok(new { message = "积分记录添加成功" });
                }
                else
                {
                    return BadRequest(new { message = "积分记录添加失败" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加积分记录失败");
                return StatusCode(500, new { message = "添加积分记录失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 为订单添加积分
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <param name="orderId">订单ID</param>
        /// <param name="orderAmount">订单金额</param>
        /// <returns>添加结果</returns>
        [HttpPost("orders/{orderId}/points")]
        public async Task<ActionResult> AddPointsForOrder(
            int orderId,
            [FromBody] AddPointsRequest request)
        {
            try
            {
                _logger.LogInformation($"为订单 {orderId} 添加积分，客户ID: {request.CustomerId}，金额: {request.OrderAmount}");

                var success = await _pointsService.AddPointsForOrderAsync(request.CustomerId, orderId, request.OrderAmount);

                if (success)
                {
                    return Ok(new { message = "订单积分添加成功" });
                }
                else
                {
                    return BadRequest(new { message = "订单积分添加失败" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"为订单 {orderId} 添加积分失败");
                return StatusCode(500, new { message = "添加订单积分失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 创建测试积分记录
        /// </summary>
        /// <returns>创建结果</returns>
        [HttpPost("test-data")]
        public async Task<ActionResult> CreateTestPointsData()
        {
            try
            {
                _logger.LogInformation("创建测试积分数据");

                var success = await _pointsService.CreateTestPointsRecordAsync();

                if (success)
                {
                    return Ok(new { message = "测试积分数据创建成功" });
                }
                else
                {
                    return BadRequest(new { message = "测试积分数据创建失败" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建测试积分数据失败");
                return StatusCode(500, new { message = "创建测试积分数据失败", error = ex.Message });
            }
        }
    }

    /// <summary>
    /// 添加积分请求模型
    /// </summary>
    public class AddPointsRequest
    {
        public int CustomerId { get; set; }
        public decimal OrderAmount { get; set; }
    }
}