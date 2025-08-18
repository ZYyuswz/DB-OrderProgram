using Dapper;
using RestaurantManagement.Models;

namespace RestaurantManagement.Services
{
    public class OrderService
    {
        private readonly DatabaseService _dbService;

        public OrderService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        // 获取所有订单
        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            using var connection = _dbService.CreateConnection();
            var sql = "SELECT * FROM PUB.Orders ORDER BY OrderTime DESC";
            return await connection.QueryAsync<Order>(sql);
        }

        // 根据ID获取订单详情
        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            using var connection = _dbService.CreateConnection();
            var sql = "SELECT * FROM PUB.Orders WHERE OrderID = :OrderId";
            
            Console.WriteLine($"[OrderService] 查询订单基本信息: OrderID={orderId}");
            Console.WriteLine($"[OrderService] 执行SQL: {sql}");
            
            try
            {
                var result = await connection.QueryFirstOrDefaultAsync<Order>(sql, new { OrderId = orderId });
                Console.WriteLine($"[OrderService] 订单基本信息查询结果: {(result != null ? "找到订单" : "订单不存在")}");
                
                if (result != null)
                {
                    Console.WriteLine($"[OrderService] 订单详情: ID={result.OrderID}, 状态={result.OrderStatus}, 总价={result.TotalPrice}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderService] 查询订单时发生错误: {ex.Message}");
                throw;
            }
        }

        // 获取订单的菜品详情（增强版）
        public async Task<IEnumerable<OrderDetailDto>> GetOrderDetailsAsync(int orderId)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                SELECT 
                    od.ORDERDETAILID as OrderDetailID,
                    od.ORDERID as OrderID,
                    od.DISHID as DishID,
                    od.QUANTITY as Quantity,
                    od.UNITPRICE as UnitPrice,
                    od.SUBTOTAL as Subtotal,
                    od.SPECIALREQUESTS as SpecialRequests,
                    d.DISHNAME as DishName,
                    d.DESCRIPTION as DishDescription,
                    d.IMAGEURL as DishImageURL,
                    c.CATEGORYNAME as CategoryName,
                    c.CATEGORYID as CategoryID
                FROM PUB.ORDERDETAIL od
                INNER JOIN PUB.DISH d ON od.DISHID = d.DISHID
                LEFT JOIN PUB.CATEGORY c ON d.CATEGORYID = c.CATEGORYID
                WHERE od.ORDERID = :OrderId
                ORDER BY od.ORDERDETAILID";
            
            Console.WriteLine($"[OrderService] 获取订单详情: 订单ID={orderId}");
            Console.WriteLine($"[OrderService] 执行SQL: {sql}");
            
            try
            {
                var result = await connection.QueryAsync<OrderDetailDto>(sql, new { OrderId = orderId });
                var resultList = result.ToList();
                
                Console.WriteLine($"[OrderService] 订单详情查询结果: 找到{resultList.Count}条菜品记录");
                
                // 输出每条记录的详细信息用于调试
                for (int i = 0; i < Math.Min(3, resultList.Count); i++) // 只输出前3条避免日志过长
                {
                    var detail = resultList[i];
                    Console.WriteLine($"[OrderService] 详情[{i}]: DishName='{detail.DishName}', Quantity={detail.Quantity}, UnitPrice={detail.UnitPrice}, Subtotal={detail.Subtotal}");
                }
                
                return resultList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderService] 获取订单详情时发生错误: {ex.Message}");
                Console.WriteLine($"[OrderService] 错误堆栈: {ex.StackTrace}");
                throw;
            }
        }

        // 根据桌台ID获取当前订单
        public async Task<Order?> GetCurrentOrderByTableAsync(int tableId)
        {
            using var connection = _dbService.CreateConnection();
            // 查找状态不是"已结账"和"已取消"的订单
            var sql = @"SELECT * FROM PUB.Orders 
                       WHERE TableID = :TableId 
                       AND OrderStatus NOT IN ('已结账', '已取消')
                       ORDER BY OrderTime DESC";
            return await connection.QueryFirstOrDefaultAsync<Order>(sql, new { TableId = tableId });
        }

        // 获取单个订单明细
public async Task<OrderDetailDto?> GetOrderDetailByIdAsync(int detailId)
{
    using var connection = _dbService.CreateConnection();
    var sql = @"
        SELECT 
            od.ORDERDETAILID as OrderDetailID,
            od.ORDERID as OrderID,
            od.DISHID as DishID,
            od.QUANTITY as Quantity,
            od.UNITPRICE as UnitPrice,
            od.SUBTOTAL as Subtotal,
            od.SPECIALREQUESTS as SpecialRequests,
            d.DISHNAME as DishName,
            d.DESCRIPTION as DishDescription,
            d.IMAGEURL as DishImageURL,
            c.CATEGORYNAME as CategoryName,
            c.CATEGORYID as CategoryID
        FROM PUB.ORDERDETAIL od
        INNER JOIN PUB.DISH d ON od.DISHID = d.DISHID
        LEFT JOIN PUB.CATEGORY c ON d.CATEGORYID = c.CATEGORYID
        WHERE od.ORDERDETAILID = :DetailId";
    return await connection.QueryFirstOrDefaultAsync<OrderDetailDto>(sql, new { DetailId = detailId });
}

// 删除订单明细
public async Task<bool> DeleteOrderDetailAsync(int detailId)
{
    using var connection = _dbService.CreateConnection();
    var sql = "DELETE FROM PUB.OrderDetail WHERE ORDERDETAILID = :DetailId";
    var result = await connection.ExecuteAsync(sql, new { DetailId = detailId });
    return result > 0;
}

        // 获取桌台的所有订单（用于调试）
        public async Task<IEnumerable<Order>> GetAllOrdersByTableAsync(int tableId)
        {
            using var connection = _dbService.CreateConnection();
            var sql = "SELECT * FROM PUB.Orders WHERE TableID = :TableId ORDER BY OrderTime DESC";
            return await connection.QueryAsync<Order>(sql, new { TableId = tableId });
        }

        // 根据桌台获取订单历史
        public async Task<IEnumerable<Order>> GetOrdersByTableAsync(int tableId, int limit = 10)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"SELECT TOP (:Limit) * FROM PUB.Orders 
                       WHERE TableID = :TableId 
                       ORDER BY OrderTime DESC";
            return await connection.QueryAsync<Order>(sql, new { TableId = tableId, Limit = limit });
        }

        // 创建新订单
        public async Task<int> CreateOrderAsync(Order order)
        {
            using var connection = _dbService.CreateConnection();
            
            // Oracle数据库使用不同的语法
            var sql = @"
                INSERT INTO PUB.Orders (TableID, CustomerID, StaffID, OrderTime, TotalPrice, OrderStatus, StoreID, CreateTime, UpdateTime)
                VALUES (:TableID, :CustomerID, :StaffID, :OrderTime, :TotalPrice, :OrderStatus, :StoreID, :CreateTime, :UpdateTime)";
            
            // 设置默认值
            order.TotalPrice = order.TotalPrice ?? 0;
            order.StoreID = order.StoreID ?? 1; // 默认店铺ID
            order.CreateTime = DateTime.Now;
            order.UpdateTime = DateTime.Now;
            
            var result = await connection.ExecuteAsync(sql, order);
            
            if (result > 0)
            {
                // 获取刚插入的订单ID
                var getIdSql = "SELECT MAX(OrderID) FROM PUB.Orders WHERE StoreID = :StoreID";
                var orderId = await connection.QuerySingleAsync<int>(getIdSql, new { StoreID = order.StoreID });
                return orderId;
            }
            
            throw new Exception("创建订单失败");
        }

        // 添加订单详情
        public async Task<bool> AddOrderDetailAsync(OrderDetail detail)
        {
            using var connection = _dbService.CreateConnection();
            
            Console.WriteLine($"[OrderService] 准备添加订单详情:");
            Console.WriteLine($"  - OrderID: {detail.OrderID}");
            Console.WriteLine($"  - DishID: {detail.DishID}");
            Console.WriteLine($"  - Quantity: {detail.Quantity}");
            Console.WriteLine($"  - UnitPrice: {detail.UnitPrice}");
            Console.WriteLine($"  - Subtotal: {detail.Subtotal}");
            Console.WriteLine($"  - SpecialRequests: {detail.SpecialRequests ?? "null"}");
            
            // 首先验证菜品是否存在
            var dishCheckSql = "SELECT COUNT(*) FROM PUB.Dish WHERE DishID = :DishID";
            var dishExists = await connection.QuerySingleAsync<int>(dishCheckSql, new { DishID = detail.DishID });
            Console.WriteLine($"[OrderService] 菜品 {detail.DishID} 存在性检查: {(dishExists > 0 ? "存在" : "不存在")}");
            
            if (dishExists == 0)
            {
                Console.WriteLine($"[OrderService] 错误: 菜品 {detail.DishID} 不存在");
                throw new ArgumentException($"菜品 {detail.DishID} 不存在");
            }
            
            // 生成新的OrderDetailID（如果序列不存在，使用最大值+1的方式）
            var getMaxIdSql = "SELECT NVL(MAX(ORDERDETAILID), 0) + 1 FROM PUB.OrderDetail";
            var newOrderDetailId = await connection.QuerySingleAsync<int>(getMaxIdSql);
            Console.WriteLine($"[OrderService] 生成新的OrderDetailID: {newOrderDetailId}");
            
            var sql = @"
                INSERT INTO PUB.OrderDetail (ORDERDETAILID, OrderID, DishID, Quantity, UnitPrice, Subtotal, SpecialRequests)
                VALUES (:OrderDetailID, :OrderID, :DishID, :Quantity, :UnitPrice, :Subtotal, :SpecialRequests)";
                
            Console.WriteLine($"[OrderService] 执行SQL: {sql}");
            
            try
            {
                // 验证订单是否存在
                var orderCheckSql = "SELECT COUNT(*) FROM PUB.Orders WHERE OrderID = :OrderID";
                var orderExists = await connection.QuerySingleAsync<int>(orderCheckSql, new { OrderID = detail.OrderID });
                Console.WriteLine($"[OrderService] 订单 {detail.OrderID} 存在性检查: {(orderExists > 0 ? "存在" : "不存在")}");
                
                if (orderExists == 0)
                {
                    Console.WriteLine($"[OrderService] 错误: 订单 {detail.OrderID} 不存在");
                    throw new ArgumentException($"订单 {detail.OrderID} 不存在");
                }
                
                // 创建包含新OrderDetailID的参数对象
                var parameters = new
                {
                    OrderDetailID = newOrderDetailId,
                    OrderID = detail.OrderID,
                    DishID = detail.DishID,
                    Quantity = detail.Quantity,
                    UnitPrice = detail.UnitPrice,
                    Subtotal = detail.Subtotal,
                    SpecialRequests = detail.SpecialRequests
                };
                
                Console.WriteLine($"[OrderService] SQL参数: OrderDetailID={newOrderDetailId}, OrderID={detail.OrderID}, DishID={detail.DishID}, Quantity={detail.Quantity}, UnitPrice={detail.UnitPrice}, Subtotal={detail.Subtotal}, SpecialRequests={detail.SpecialRequests}");
                
                var result = await connection.ExecuteAsync(sql, parameters);
                Console.WriteLine($"[OrderService] 订单详情添加结果: {(result > 0 ? "成功" : "失败")}, 影响行数: {result}");
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderService] 添加订单详情失败: {ex.Message}");
                Console.WriteLine($"[OrderService] 完整错误信息: {ex}");
                Console.WriteLine($"[OrderService] 异常类型: {ex.GetType().Name}");
                
                // 记录具体的Oracle错误信息
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[OrderService] 内部异常: {ex.InnerException.Message}");
                    Console.WriteLine($"[OrderService] 内部异常类型: {ex.InnerException.GetType().Name}");
                }
                
                // 特别检查Oracle特定的错误
                if (ex.Message.Contains("ORA-") || (ex.InnerException?.Message.Contains("ORA-") == true))
                {
                    Console.WriteLine($"[OrderService] 检测到Oracle数据库错误");
                }
                
                throw;
            }
        }

        // 更新订单状态
        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            using var connection = _dbService.CreateConnection();
            
            Console.WriteLine($"准备更新订单 {orderId} 状态为: {status}");
            
            var sql = "UPDATE PUB.Orders SET OrderStatus = :Status, UpdateTime = :UpdateTime WHERE OrderID = :OrderId";
            var result = await connection.ExecuteAsync(sql, new { 
                Status = status, 
                OrderId = orderId,
                UpdateTime = DateTime.Now
            });
            
            Console.WriteLine($"订单状态更新结果: {(result > 0 ? "成功" : "失败")}, 影响行数: {result}");
            
            return result > 0;
        }

        // 结账
        public async Task<bool> CheckoutOrderAsync(int orderId)
        {
            Console.WriteLine($"开始执行订单结账: {orderId}");
            var result = await UpdateOrderStatusAsync(orderId, "已结账");
            Console.WriteLine($"订单结账结果: {(result ? "成功" : "失败")}");
            return result;
        }

        // 取消订单
        public async Task<bool> CancelOrderAsync(int orderId)
        {
            return await UpdateOrderStatusAsync(orderId, "已取消");
        }

        // 更新订单总金额
        public async Task<bool> UpdateOrderTotalAsync(int orderId)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                UPDATE PUB.Orders 
                SET TotalPrice = (
                    SELECT NVL(SUM(Subtotal), 0) 
                    FROM PUB.OrderDetail 
                    WHERE OrderID = :OrderId
                ),
                UpdateTime = :UpdateTime
                WHERE OrderID = :OrderId";
            var result = await connection.ExecuteAsync(sql, new { 
                OrderId = orderId,
                UpdateTime = DateTime.Now 
            });
            return result > 0;
        }

        // 生成订单小票
        public async Task<object> GenerateReceiptAsync(int orderId)
        {
            var order = await GetOrderByIdAsync(orderId);
            if (order == null)
                throw new Exception("订单不存在");

            var details = await GetOrderDetailsAsync(orderId);
            
            return new
            {
                OrderId = orderId,
                OrderTime = order.OrderTime,
                TableId = order.TableID,
                Items = details.Select(d => new
                {
                    DishName = d.DishName,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    Subtotal = d.Subtotal,
                    SpecialRequests = d.SpecialRequests
                }),
                TotalAmount = details.Sum(d => d.Subtotal),
                Status = order.OrderStatus,
                PrintTime = DateTime.Now
            };
        }

        // 打印订单（模拟打印功能）
        public async Task<bool> PrintOrderAsync(int orderId)
        {
            try
            {
                var receipt = await GenerateReceiptAsync(orderId);
                
                // 这里可以集成实际的打印机API
                Console.WriteLine($"=== 订单小票 #{orderId} ===");
                Console.WriteLine($"时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"订单号: {orderId}");
                Console.WriteLine("================================");
                
                // 实际应用中，这里会调用打印机驱动或打印服务
                // 现在只是模拟打印成功
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"打印失败: {ex.Message}");
                return false;
            }
        }

        // 获取订单的员工信息
        public async Task<dynamic?> GetOrderStaffInfoAsync(int orderId)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                SELECT 
                    NULL as StaffID,
                    '系统用户' as StaffName,
                    '服务员' as Position,
                    NULL as Phone,
                    ti.TableNumber,
                    ti.Area as TableArea,
                    ti.Capacity as TableCapacity
                FROM PUB.Orders o
                LEFT JOIN PUB.TableInfo ti ON o.TableID = ti.TableID
                WHERE o.OrderID = :OrderId";
            
            Console.WriteLine($"[OrderService] 查询订单员工信息: OrderID={orderId}");
            try
            {
                var result = await connection.QueryFirstOrDefaultAsync(sql, new { OrderId = orderId });
                Console.WriteLine($"[OrderService] 员工信息查询结果: {(result != null ? "找到信息" : "未找到信息")}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderService] 查询员工信息时发生错误: {ex.Message}");
                throw;
            }
        }

        // 获取订单的分类统计信息
        public async Task<IEnumerable<dynamic>> GetOrderCategoryStatsAsync(int orderId)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                SELECT 
                    c.CategoryID,
                    c.CategoryName,
                    COUNT(od.OrderDetailID) as ItemCount,
                    SUM(od.Quantity) as TotalQuantity,
                    SUM(od.Subtotal) as TotalAmount,
                    AVG(od.UnitPrice) as AvgPrice,
                    MIN(od.UnitPrice) as MinPrice,
                    MAX(od.UnitPrice) as MaxPrice
                FROM PUB.OrderDetail od
                INNER JOIN PUB.Dish d ON od.DishID = d.DishID
                LEFT JOIN PUB.Category c ON d.CategoryID = c.CategoryID
                WHERE od.OrderID = :OrderId
                GROUP BY c.CategoryID, c.CategoryName
                ORDER BY TotalAmount DESC";
            
            return await connection.QueryAsync(sql, new { OrderId = orderId });
        }

        // 获取增强的订单详情
        public async Task<dynamic?> GetEnhancedOrderDetailsAsync(int orderId)
        {
            using var connection = _dbService.CreateConnection();
            
            // 获取基本订单信息（不包含Staff字段）
            var orderSql = @"
                SELECT 
                    o.*,
                    '系统用户' as StaffName,
                    '服务员' as StaffPosition,
                    NULL as StaffPhone,
                    ti.TableNumber,
                    ti.Area as TableArea,
                    ti.Capacity as TableCapacity
                FROM PUB.Orders o
                LEFT JOIN PUB.TableInfo ti ON o.TableID = ti.TableID
                WHERE o.OrderID = :OrderId";
            
            Console.WriteLine($"[OrderService] 查询增强订单详情: OrderID={orderId}");
            var order = await connection.QueryFirstOrDefaultAsync(orderSql, new { OrderId = orderId });
            
            if (order == null)
            {
                Console.WriteLine($"[OrderService] 订单 {orderId} 不存在");
                return null;
            }

            Console.WriteLine($"[OrderService] 订单 {orderId} 存在，继续查询详细信息");

            // 获取详细菜品信息
            var details = await GetOrderDetailsAsync(orderId);
            
            // 获取分类统计
            var categoryStats = await GetOrderCategoryStatsAsync(orderId);
            
            // 计算高级统计
            var detailsList = details.ToList();
            if (detailsList.Any())
            {
                var totalItems = detailsList.Sum(d => Convert.ToInt32(d.Quantity));
                var totalAmount = detailsList.Sum(d => Convert.ToDecimal(d.Subtotal));
                var avgItemPrice = totalAmount / detailsList.Count;
                var mostExpensiveItem = detailsList.OrderByDescending(d => Convert.ToDecimal(d.Subtotal)).FirstOrDefault();
                var mostOrderedItem = detailsList.OrderByDescending(d => Convert.ToInt32(d.Quantity)).FirstOrDefault();

                return new
                {
                    Order = order,
                    Details = detailsList,
                    CategoryStats = categoryStats,
                    Summary = new
                    {
                        TotalItems = totalItems,
                        TotalAmount = totalAmount,
                        ItemCount = detailsList.Count,
                        AvgItemPrice = Math.Round((decimal)avgItemPrice, 2),
                        CategoryCount = categoryStats.Count(),
                        MostExpensiveItem = mostExpensiveItem != null ? new
                        {
                            DishName = mostExpensiveItem.DishName,
                            Subtotal = mostExpensiveItem.Subtotal
                        } : null,
                        MostOrderedItem = mostOrderedItem != null ? new
                        {
                            DishName = mostOrderedItem.DishName,
                            Quantity = mostOrderedItem.Quantity
                        } : null
                    }
                };
            }
            else
            {
                return new
                {
                    Order = order,
                    Details = new List<object>(),
                    CategoryStats = new List<object>(),
                    Summary = new
                    {
                        TotalItems = 0,
                        TotalAmount = 0m,
                        ItemCount = 0,
                        AvgItemPrice = 0m,
                        CategoryCount = 0,
                        MostExpensiveItem = (object?)null,
                        MostOrderedItem = (object?)null
                    }
                };
            }
        }
    }
}
