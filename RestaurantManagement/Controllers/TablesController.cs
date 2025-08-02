using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Services;
using RestaurantManagement.Models;

namespace RestaurantManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TablesController : ControllerBase
    {
        private readonly TableService _tableService;
        private readonly OrderService _orderService;
        private readonly ILogger<TablesController> _logger;

        public TablesController(TableService tableService, OrderService orderService, ILogger<TablesController> logger)
        {
            _tableService = tableService;
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// 获取所有桌台信息
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllTables()
        {
            try
            {
                _logger.LogInformation("获取所有桌台信息");
                var tables = await _tableService.GetAllTablesAsync();
                
                _logger.LogInformation($"成功获取 {tables.Count()} 个桌台信息");
                return Ok(tables);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取桌台信息失败");
                return StatusCode(500, new { message = "获取桌台信息失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 根据ID获取单个桌台信息
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTable(int id)
        {
            try
            {
                _logger.LogInformation($"获取桌台信息: TableID={id}");
                
                var table = await _tableService.GetTableByIdAsync(id);
                if (table == null)
                {
                    _logger.LogWarning($"桌台不存在: TableID={id}");
                    return NotFound(new { message = "桌台不存在" });
                }
                
                _logger.LogInformation($"成功获取桌台信息: TableID={id}, TableNumber={table.TableNumber}");
                return Ok(table);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取桌台信息失败: TableID={id}");
                return StatusCode(500, new { message = "获取桌台信息失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 更新桌台状态
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateTableStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                _logger.LogInformation($"更新桌台状态请求: TableID={id}, Status={request?.Status}");
                
                if (request == null || string.IsNullOrWhiteSpace(request.Status))
                {
                    _logger.LogWarning($"无效的状态更新请求: TableID={id}");
                    return BadRequest(new { message = "状态参数不能为空" });
                }

                // 验证状态值是否有效
                var validStatuses = new[] { "空闲", "占用", "预订", "清洁中" };
                if (!validStatuses.Contains(request.Status))
                {
                    _logger.LogWarning($"无效的状态值: {request.Status}");
                    return BadRequest(new { message = "无效的状态值", validStatuses });
                }

                // 检查桌台是否存在
                var table = await _tableService.GetTableByIdAsync(id);
                if (table == null)
                {
                    _logger.LogWarning($"桌台不存在: TableID={id}");
                    return NotFound(new { message = "桌台不存在" });
                }

                // 更新状态
                var success = await _tableService.UpdateTableStatusAsync(id, request.Status);
                if (!success)
                {
                    _logger.LogError($"更新桌台状态失败: TableID={id}, Status={request.Status}");
                    return StatusCode(500, new { message = "更新桌台状态失败" });
                }

                _logger.LogInformation($"成功更新桌台状态: TableID={id}, Status={request.Status}");
                return Ok(new { 
                    message = "桌台状态更新成功", 
                    tableId = id, 
                    newStatus = request.Status 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新桌台状态异常: TableID={id}");
                return StatusCode(500, new { message = "更新桌台状态失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 获取桌台当前订单
        /// </summary>
        [HttpGet("{id}/current-order")]
        public async Task<IActionResult> GetCurrentOrder(int id)
        {
            try
            {
                _logger.LogInformation($"获取桌台当前订单: TableID={id}");
                
                // 检查桌台是否存在
                var table = await _tableService.GetTableByIdAsync(id);
                if (table == null)
                {
                    _logger.LogWarning($"桌台不存在: TableID={id}");
                    return NotFound(new { message = "桌台不存在" });
                }

                // 获取当前订单
                var order = await _orderService.GetCurrentOrderByTableAsync(id);
                if (order == null)
                {
                    _logger.LogInformation($"桌台无当前订单: TableID={id}");
                    return NotFound(new { message = "该桌台暂无订单" });
                }

                _logger.LogInformation($"成功获取桌台当前订单: TableID={id}, OrderID={order.OrderID}");
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取桌台当前订单失败: TableID={id}");
                return StatusCode(500, new { message = "获取桌台当前订单失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 开台（将桌台状态设为占用）
        /// </summary>
        [HttpPost("{id}/open")]
        public async Task<IActionResult> OpenTable(int id, [FromBody] OpenTableRequest? request = null)
        {
            try
            {
                _logger.LogInformation($"开台请求: TableID={id}");
                
                // 检查桌台是否存在
                var table = await _tableService.GetTableByIdAsync(id);
                if (table == null)
                {
                    _logger.LogWarning($"桌台不存在: TableID={id}");
                    return NotFound(new { message = "桌台不存在" });
                }

                // 检查桌台当前状态
                if (table.Status != "空闲")
                {
                    _logger.LogWarning($"桌台状态不允许开台: TableID={id}, CurrentStatus={table.Status}");
                    return BadRequest(new { message = $"桌台当前状态为'{table.Status}'，无法开台" });
                }

                // 开台
                var success = await _tableService.OpenTableAsync(id);
                if (!success)
                {
                    _logger.LogError($"开台失败: TableID={id}");
                    return StatusCode(500, new { message = "开台失败" });
                }

                _logger.LogInformation($"开台成功: TableID={id}");
                return Ok(new { 
                    message = "开台成功", 
                    tableId = id,
                    tableNumber = table.TableNumber,
                    notes = request?.Notes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"开台异常: TableID={id}");
                return StatusCode(500, new { message = "开台失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 清洁完成（将桌台状态设为空闲）
        /// </summary>
        [HttpPost("{id}/clean-complete")]
        public async Task<IActionResult> CompleteTableCleaning(int id)
        {
            try
            {
                _logger.LogInformation($"完成桌台清洁: TableID={id}");
                
                // 检查桌台是否存在
                var table = await _tableService.GetTableByIdAsync(id);
                if (table == null)
                {
                    _logger.LogWarning($"桌台不存在: TableID={id}");
                    return NotFound(new { message = "桌台不存在" });
                }

                // 检查桌台当前状态
                if (table.Status != "清洁中")
                {
                    _logger.LogWarning($"桌台状态不是清洁中: TableID={id}, CurrentStatus={table.Status}");
                    return BadRequest(new { message = $"桌台当前状态为'{table.Status}'，无法完成清洁操作" });
                }

                // 完成清洁
                var success = await _tableService.CompleteCleaningAsync(id);
                if (!success)
                {
                    _logger.LogError($"完成清洁失败: TableID={id}");
                    return StatusCode(500, new { message = "完成清洁失败" });
                }

                _logger.LogInformation($"完成清洁成功: TableID={id}");
                return Ok(new { 
                    message = "清洁完成，桌台已设为空闲", 
                    tableId = id,
                    tableNumber = table.TableNumber
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"完成清洁异常: TableID={id}");
                return StatusCode(500, new { message = "完成清洁失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 获取桌台统计信息
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetTableStatistics()
        {
            try
            {
                _logger.LogInformation("获取桌台统计信息");
                
                var tables = await _tableService.GetAllTablesAsync();
                var statistics = new
                {
                    total = tables.Count(),
                    available = tables.Count(t => t.Status == "空闲"),
                    occupied = tables.Count(t => t.Status == "占用"),
                    reserved = tables.Count(t => t.Status == "预订"),
                    cleaning = tables.Count(t => t.Status == "清洁中")
                };

                _logger.LogInformation($"桌台统计: 总计={statistics.total}, 空闲={statistics.available}, 占用={statistics.occupied}, 预订={statistics.reserved}, 清洁中={statistics.cleaning}");
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取桌台统计信息失败");
                return StatusCode(500, new { message = "获取桌台统计信息失败", error = ex.Message });
            }
        }
    }
}
