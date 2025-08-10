using Microsoft.AspNetCore.Mvc;
using ConsoleApp1.Services;

namespace ConsoleApp1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly DatabaseService _databaseService;
        private readonly OrderService _orderService;
        private readonly PointsService _pointsService;
        private readonly ILogger<TestController> _logger;

        public TestController(
            DatabaseService databaseService, 
            OrderService orderService, 
            PointsService pointsService,
            ILogger<TestController> logger)
        {
            _databaseService = databaseService;
            _orderService = orderService;
            _pointsService = pointsService;
            _logger = logger;
        }

        /// <summary>
        /// 初始化测试数据
        /// </summary>
        /// <returns>初始化结果</returns>
        [HttpPost("init-data")]
        public async Task<ActionResult> InitializeTestData()
        {
            try
            {
                _logger.LogInformation("开始初始化测试数据");
                
                // 1. 测试数据库连接
                bool connectionOk = await _databaseService.TestConnectionAsync();
                if (!connectionOk)
                {
                    return BadRequest(new { message = "数据库连接失败" });
                }

                // 2. 检查表是否存在
                bool tablesExist = await _databaseService.CheckTablesExistAsync();
                if (!tablesExist)
                {
                    return BadRequest(new { message = "核心表不存在，请先创建数据库表" });
                }

                // 3. 添加测试门店
                bool storeAdded = await _databaseService.AddTestStoreAsync();
                
                // 4. 添加测试客户
                bool customerAdded = await _databaseService.AddTestCustomerAsync();
                
                // 5. 创建测试订单
                bool orderCreated = await _orderService.CreateTestOrderWithDetailsAsync();
                
                // 6. 创建测试积分记录
                bool pointsCreated = await _pointsService.CreateTestPointsRecordAsync();
                
                // 7. 为现有订单添加积分记录
                bool existingOrdersPointsAdded = await _orderService.AddPointsForExistingOrdersAsync(1);
                
                _logger.LogInformation("测试数据初始化完成");
                
                return Ok(new { 
                    message = "测试数据初始化成功",
                    results = new {
                        databaseConnection = connectionOk,
                        tablesExist = tablesExist,
                        storeAdded = storeAdded,
                        customerAdded = customerAdded,
                        orderCreated = orderCreated,
                        pointsCreated = pointsCreated,
                        existingOrdersPointsAdded = existingOrdersPointsAdded
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化测试数据失败");
                return StatusCode(500, new { message = "初始化测试数据失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 获取系统状态
        /// </summary>
        /// <returns>系统状态信息</returns>
        [HttpGet("status")]
        public async Task<ActionResult> GetSystemStatus()
        {
            try
            {
                var status = new
                {
                    timestamp = DateTime.Now,
                    databaseConnection = await _databaseService.TestConnectionAsync(),
                    tablesExist = await _databaseService.CheckTablesExistAsync(),
                    message = "系统运行正常"
                };
                
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取系统状态失败");
                return StatusCode(500, new { message = "获取系统状态失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 为现有订单添加积分记录
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <returns>添加结果</returns>
        [HttpPost("add-points-for-existing-orders/{customerId}")]
        public async Task<ActionResult> AddPointsForExistingOrders(int customerId)
        {
            try
            {
                _logger.LogInformation($"开始为客户 {customerId} 的现有订单添加积分记录");
                
                bool success = await _orderService.AddPointsForExistingOrdersAsync(customerId);
                
                if (success)
                {
                    return Ok(new { message = $"成功为客户 {customerId} 的现有订单添加积分记录" });
                }
                else
                {
                    return BadRequest(new { message = $"为客户 {customerId} 的现有订单添加积分记录失败" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"为客户 {customerId} 的现有订单添加积分记录失败");
                return StatusCode(500, new { message = "添加积分记录失败", error = ex.Message });
            }
        }
    }
} 