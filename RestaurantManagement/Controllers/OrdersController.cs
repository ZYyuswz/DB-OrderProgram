using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Services;
using RestaurantManagement.Models;

namespace RestaurantManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly TableService _tableService;

        public OrdersController(OrderService orderService, TableService tableService)
        {
            _orderService = orderService;
            _tableService = tableService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                    return NotFound(new { message = "订单不存在" });
                return Ok(order);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取订单失败", error = ex.Message });
            }
        }

        [HttpGet("{id}/details")]
        public async Task<IActionResult> GetOrderDetails(int id)
        {
            try
            {
                Console.WriteLine($"[OrdersController] 获取订单详情请求: OrderID={id}");
                
                // 首先验证订单是否存在
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    Console.WriteLine($"[OrdersController] 订单不存在: OrderID={id}");
                    return NotFound(new { message = "订单不存在" });
                }

                var details = await _orderService.GetOrderDetailsAsync(id);
                var detailsList = details.ToList();
                
                if (!detailsList.Any())
                {
                    Console.WriteLine($"[OrdersController] 订单详情为空: OrderID={id}");
                    return Ok(new
                    {
                        orderId = id,
                        orderStatus = order.OrderStatus,
                        details = new List<object>(),
                        statistics = new
                        {
                            itemCount = 0,
                            totalItems = 0,
                            totalAmount = 0m,
                            averagePrice = 0m
                        }
                    });
                }
                
                // 安全地计算统计信息，使用强类型属性
                var totalItems = detailsList.Sum(d => d.Quantity);
                var totalAmount = detailsList.Sum(d => d.Subtotal);
                var itemCount = detailsList.Count;

                Console.WriteLine($"[OrdersController] 订单详情获取成功: OrderID={id}, 详情数量={itemCount}");
                
                return Ok(new
                {
                    orderId = id,
                    orderStatus = order.OrderStatus,
                    details = detailsList,
                    statistics = new
                    {
                        itemCount,
                        totalItems,
                        totalAmount,
                        averagePrice = itemCount > 0 ? totalAmount / itemCount : 0m
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrdersController] 获取订单详情失败: OrderID={id}, 错误={ex.Message}");
                return StatusCode(500, new { message = "获取订单详情失败", error = ex.Message });
            }
        }

        [HttpGet("{id}/enhanced-details")]
        public async Task<IActionResult> GetEnhancedOrderDetails(int id)
        {
            try
            {
                Console.WriteLine($"[OrdersController] 获取增强订单详情请求: OrderID={id}");
                
                var enhancedDetails = await _orderService.GetEnhancedOrderDetailsAsync(id);
                if (enhancedDetails == null)
                {
                    Console.WriteLine($"[OrdersController] 订单不存在: OrderID={id}");
                    return NotFound(new { message = "订单不存在" });
                }

                Console.WriteLine($"[OrdersController] 增强订单详情获取成功: OrderID={id}");
                return Ok(enhancedDetails);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrdersController] 获取增强订单详情失败: OrderID={id}, 错误={ex.Message}");
                Console.WriteLine($"[OrdersController] 错误堆栈: {ex.StackTrace}");
                return StatusCode(500, new { message = "获取增强订单详情失败", error = ex.Message });
            }
        }

        [HttpGet("{id}/details/summary")]
        public async Task<IActionResult> GetOrderDetailsSummary(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                    return NotFound(new { message = "订单不存在" });

                var details = await _orderService.GetOrderDetailsAsync(id);
                var staff = await _orderService.GetOrderStaffInfoAsync(id);

                // 按分类统计菜品
                var categoryStats = await _orderService.GetOrderCategoryStatsAsync(id);

                return Ok(new
                {
                    orderInfo = new
                    {
                        orderId = order.OrderID,
                        tableId = order.TableID,
                        orderTime = order.OrderTime,
                        orderStatus = order.OrderStatus,
                        totalPrice = order.TotalPrice ?? 0,
                        notes = order.Notes
                    },
                    staffInfo = staff,
                    itemsCount = details.Count(),
                    totalQuantity = details.Sum(d => d.Quantity),
                    categoryStats,
                    details = details.Select(d => new
                    {
                        detailId = d.OrderDetailID,
                        dishId = d.DishID,
                        dishName = d.DishName,
                        quantity = d.Quantity,
                        unitPrice = d.UnitPrice,
                        subtotal = d.Subtotal,
                        specialRequests = d.SpecialRequests,
                        // 计算单项占总订单的百分比
                        percentage = order.TotalPrice > 0 ? Math.Round((d.Subtotal / (order.TotalPrice ?? 1)) * 100, 2) : 0
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取订单详情摘要失败", error = ex.Message });
            }
        }

        [HttpGet("{id}/full-details")]
        public async Task<IActionResult> GetOrderFullDetails(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                    return NotFound(new { message = "订单不存在" });

                var details = await _orderService.GetOrderDetailsAsync(id);

                return Ok(new
                {
                    order,
                    details,
                    totalItems = details.Count(),
                    totalAmount = details.Sum(d => d.Subtotal)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取订单完整信息失败", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            try
            {
                if (order == null)
                    return BadRequest(new { message = "订单数据不能为空" });

                order.OrderTime = DateTime.Now;
                order.OrderStatus = "待处理";
                var orderId = await _orderService.CreateOrderAsync(order);
                
                return Ok(new { 
                    success = true, 
                    message = "订单创建成功", 
                    orderId = orderId 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "创建订单失败", error = ex.Message });
            }
        }

        [HttpPost("{id}/details")]
        public async Task<IActionResult> AddOrderDetail(int id, [FromBody] OrderDetail detail)
        {
            try
            {
                Console.WriteLine($"[OrdersController] 收到添加订单详情请求:");
                Console.WriteLine($"  - 订单ID: {id}");
                Console.WriteLine($"  - 请求数据: {System.Text.Json.JsonSerializer.Serialize(detail)}");
                
                if (detail == null)
                {
                    Console.WriteLine($"[OrdersController] 错误: 订单详情数据为空");
                    return BadRequest(new { message = "订单详情数据不能为空" });
                }

                // 验证订单是否存在
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    Console.WriteLine($"[OrdersController] 错误: 订单 {id} 不存在");
                    return NotFound(new { message = "订单不存在" });
                }

                Console.WriteLine($"[OrdersController] 找到订单: {order.OrderID}, 状态: {order.OrderStatus}");

                detail.OrderID = id;
                detail.Subtotal = detail.Quantity * detail.UnitPrice;
                
                Console.WriteLine($"[OrdersController] 计算小计: {detail.Quantity} × {detail.UnitPrice} = {detail.Subtotal}");
                
                var result = await _orderService.AddOrderDetailAsync(detail);
                if (result)
                {
                    Console.WriteLine($"[OrdersController] 订单详情添加成功，准备更新订单总金额");
                    // 更新订单总金额
                    await _orderService.UpdateOrderTotalAsync(id);
                    Console.WriteLine($"[OrdersController] 订单总金额更新完成");
                    return Ok(new { success = true, message = "添加菜品成功" });
                }
                else
                {
                    Console.WriteLine($"[OrdersController] 订单详情添加失败");
                    return BadRequest(new { message = "添加菜品失败" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrdersController] 添加订单详情异常: {ex.Message}");
                Console.WriteLine($"[OrdersController] 完整异常信息: {ex}");
                return StatusCode(500, new { message = "添加菜品失败", error = ex.Message });
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.Status))
                    return BadRequest(new { message = "状态参数无效" });

                var result = await _orderService.UpdateOrderStatusAsync(id, request.Status);
                if (result)
                {
                    return Ok(new { success = true, message = "订单状态更新成功" });
                }
                else
                {
                    return BadRequest(new { message = "订单状态更新失败" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "更新订单状态失败", error = ex.Message });
            }
        }

        [HttpPut("{id}/checkout")]
        public async Task<IActionResult> CheckoutOrder(int id)
        {
            try
            {
                Console.WriteLine($"[OrdersController] 收到结账请求: 订单ID={id}");
                
                // 获取订单信息
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    Console.WriteLine($"[OrdersController] 订单不存在: {id}");
                    return NotFound(new { message = "订单不存在" });
                }

                Console.WriteLine($"[OrdersController] 订单信息: 桌台ID={order.TableID}, 当前状态={order.OrderStatus}");

                // 执行结账操作
                var result = await _orderService.CheckoutOrderAsync(id);
                if (!result)
                {
                    Console.WriteLine($"[OrdersController] 结账失败: {id}");
                    return BadRequest(new { message = "结账失败" });
                }

                Console.WriteLine($"[OrdersController] 结账成功，开始更新桌台状态");

                // 如果有关联桌台，更新桌台状态为清洁中
                if (order.TableID.HasValue)
                {
                    try
                    {
                        var tableUpdateResult = await _tableService.UpdateTableStatusAsync(order.TableID.Value, "清洁中");
                        Console.WriteLine($"[OrdersController] 桌台状态更新结果: {(tableUpdateResult ? "成功" : "失败")}");
                    }
                    catch (Exception tableEx)
                    {
                        Console.WriteLine($"[OrdersController] 更新桌台状态异常: {tableEx.Message}");
                        // 桌台状态更新失败不影响结账流程
                    }
                }

                return Ok(new { success = true, message = "结账成功" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrdersController] 结账异常: {ex.Message}");
                return StatusCode(500, new { message = "结账失败", error = ex.Message });
            }
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                var result = await _orderService.CancelOrderAsync(id);
                if (result)
                {
                    return Ok(new { success = true, message = "订单取消成功" });
                }
                else
                {
                    return BadRequest(new { message = "订单取消失败" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "取消订单失败", error = ex.Message });
            }
        }

        [HttpGet("{id}/receipt")]
        public async Task<IActionResult> GetOrderReceipt(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                    return NotFound(new { message = "订单不存在" });

                var details = await _orderService.GetOrderDetailsAsync(id);
                var receipt = await _orderService.GenerateReceiptAsync(id);

                return Ok(receipt);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "生成小票失败", error = ex.Message });
            }
        }

        [HttpPost("{id}/print")]
        public async Task<IActionResult> PrintOrder(int id)
        {
            try
            {
                var result = await _orderService.PrintOrderAsync(id);
                if (result)
                {
                    return Ok(new { success = true, message = "打印成功" });
                }
                else
                {
                    return BadRequest(new { message = "打印失败" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "打印失败", error = ex.Message });
            }
        }
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }
}
