using Microsoft.AspNetCore.Mvc;
using ConsoleApp1.Services;
using Microsoft.Extensions.Logging;

namespace ConsoleApp1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly DatabaseService _databaseService;
        private readonly OrderService _orderService;
        private readonly PointsService _pointsService;
        private readonly ReviewService _reviewService;
        private readonly QRCodeService _qrCodeService;
        private readonly ILogger<TestController> _logger;

        public TestController(
            DatabaseService databaseService, 
            OrderService orderService, 
            PointsService pointsService,
            ReviewService reviewService,
            QRCodeService qrCodeService,
            ILogger<TestController> logger)
        {
            _databaseService = databaseService;
            _orderService = orderService;
            _pointsService = pointsService;
            _reviewService = reviewService;
            _qrCodeService = qrCodeService;
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

        /// <summary>
        /// 测试二维码生成功能
        /// </summary>
        /// <returns>二维码生成测试结果</returns>
        [HttpGet("qrcode")]
        public async Task<IActionResult> TestQRCode()
        {
            try
            {
                _logger.LogInformation("开始测试二维码生成功能...");

                // 测试生成单个桌台的二维码
                var tableNumber = "001";
                var qrCodeBytes = await _qrCodeService.GenerateQRCodeAsync(tableNumber);
                
                _logger.LogInformation($"成功生成桌台{tableNumber}的二维码，大小: {qrCodeBytes.Length} 字节");

                return Ok(new
                {
                    success = true,
                    message = "二维码生成测试成功",
                    tableNumber = tableNumber,
                    qrCodeSize = qrCodeBytes.Length,
                    downloadUrl = $"/api/qrcode/generate/{tableNumber}",
                    base64Url = $"/api/qrcode/generate-base64/{tableNumber}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "二维码生成测试失败");
                return BadRequest(new
                {
                    success = false,
                    error = ex.Message,
                    message = "请检查微信配置是否正确，包括AppId、AppSecret和AccessToken"
                });
            }
        }

        /// <summary>
        /// 测试批量生成二维码
        /// </summary>
        /// <returns>批量生成测试结果</returns>
        [HttpGet("qrcode-batch")]
        public async Task<IActionResult> TestBatchQRCode()
        {
            try
            {
                _logger.LogInformation("开始测试批量二维码生成功能...");

                // 测试批量生成桌台001-005的二维码
                var startTable = 1;
                var endTable = 5;
                var qrCodes = await _qrCodeService.GenerateRangeQRCodesAsync(startTable, endTable);
                
                _logger.LogInformation($"成功批量生成桌台{startTable}-{endTable}的二维码，共{qrCodes.Count}个");

                var results = new List<object>();
                foreach (var kvp in qrCodes)
                {
                    results.Add(new
                    {
                        tableNumber = kvp.Key,
                        qrCodeSize = kvp.Value.Length,
                        downloadUrl = $"/api/qrcode/generate/{kvp.Key}",
                        base64Url = $"/api/qrcode/generate-base64/{kvp.Key}"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "批量二维码生成测试成功",
                    startTable = startTable,
                    endTable = endTable,
                    totalCount = results.Count,
                    results = results,
                    downloadZipUrl = $"/api/qrcode/download-zip?startTableNumber={startTable}&endTableNumber={endTable}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量二维码生成测试失败");
                return BadRequest(new
                {
                    success = false,
                    error = ex.Message,
                    message = "请检查微信配置是否正确"
                });
            }
        }
    }
} 