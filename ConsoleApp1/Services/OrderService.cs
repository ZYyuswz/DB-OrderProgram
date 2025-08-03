using Oracle.ManagedDataAccess.Client;
using ConsoleApp1.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConsoleApp1.Services
{
    /// <summary>
    /// 订单服务类
    /// </summary>
    public class OrderService
    {
        private readonly string _connectionString;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IConfiguration configuration, ILogger<OrderService> logger)
        {
            _connectionString = configuration.GetConnectionString("OracleConnection") 
                ?? throw new ArgumentNullException("Oracle connection string not found");
            _logger = logger;
        }

        /// <summary>
        /// 获取指定客户的所有订单
        /// </summary>
        public async Task<List<Orders>> GetOrdersByCustomerIdAsync(int customerId)
        {
            var orders = new List<Orders>();
            
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();
                
                var sql = @"SELECT o.OrderID, o.OrderTime, o.TableID, o.CustomerID, o.TotalPrice, 
                                  o.OrderStatus, o.StoreID, o.CreateTime, o.UpdateTime,
                                  c.CustomerName, s.StoreName, t.TableNumber
                           FROM PUB.Orders o
                           LEFT JOIN PUB.Customer c ON o.CustomerID = c.CustomerID
                           LEFT JOIN PUB.Store s ON o.StoreID = s.StoreID
                           LEFT JOIN PUB.TableInfo t ON o.TableID = t.TableID
                           WHERE o.CustomerID = :customerId
                           ORDER BY o.OrderTime DESC";
                
                using var command = new OracleCommand(sql, connection);
                command.Parameters.Add(":customerId", OracleDbType.Int32).Value = customerId;
                
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    orders.Add(new Orders
                    {
                        OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                        OrderTime = reader.GetDateTime(reader.GetOrdinal("OrderTime")),
                        TableID = reader.IsDBNull(reader.GetOrdinal("TableID")) ? null : reader.GetInt32(reader.GetOrdinal("TableID")),
                        CustomerID = reader.IsDBNull(reader.GetOrdinal("CustomerID")) ? null : reader.GetInt32(reader.GetOrdinal("CustomerID")),
                        TotalPrice = reader.GetDecimal(reader.GetOrdinal("TotalPrice")),
                        OrderStatus = reader.GetString(reader.GetOrdinal("OrderStatus")),
                        StaffID = null, // 字段不存在，设为null
                        StoreID = reader.IsDBNull(reader.GetOrdinal("StoreID")) ? null : reader.GetInt32(reader.GetOrdinal("StoreID")),
                        CreateTime = reader.GetDateTime(reader.GetOrdinal("CreateTime")),
                        UpdateTime = reader.GetDateTime(reader.GetOrdinal("UpdateTime")),
                        CustomerName = reader.IsDBNull(reader.GetOrdinal("CustomerName")) ? null : reader.GetString(reader.GetOrdinal("CustomerName")),
                        StoreName = reader.IsDBNull(reader.GetOrdinal("StoreName")) ? null : reader.GetString(reader.GetOrdinal("StoreName")),
                        StaffName = null, // StaffID字段不存在，StaffName也设为null
                        TableNumber = reader.IsDBNull(reader.GetOrdinal("TableNumber")) ? null : reader.GetString(reader.GetOrdinal("TableNumber"))
                    });
                }
                
                _logger.LogInformation($"成功查询到客户 {customerId} 的 {orders.Count} 个订单");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查询客户 {customerId} 的订单失败");
            }
            
            return orders;
        }

        /// <summary>
        /// 获取订单详情
        /// </summary>
        public async Task<List<OrderDetail>> GetOrderDetailsAsync(int orderId)
        {
            var details = new List<OrderDetail>();
            
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();
                
                var sql = @"SELECT od.OrderDetailID, od.OrderID, od.DishID, od.Quantity, 
                                  od.UnitPrice, od.Subtotal, od.SpecialRequests,
                                  d.DishName
                           FROM PUB.OrderDetail od
                           LEFT JOIN PUB.Dish d ON od.DishID = d.DishID
                           WHERE od.OrderID = :orderId
                           ORDER BY od.OrderDetailID";
                
                using var command = new OracleCommand(sql, connection);
                command.Parameters.Add(":orderId", OracleDbType.Int32).Value = orderId;
                
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    details.Add(new OrderDetail
                    {
                        OrderDetailID = reader.GetInt32(reader.GetOrdinal("OrderDetailID")),
                        OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                        DishID = reader.GetInt32(reader.GetOrdinal("DishID")),
                        Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                        UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                        Subtotal = reader.GetDecimal(reader.GetOrdinal("Subtotal")),
                        SpecialRequests = reader.IsDBNull(reader.GetOrdinal("SpecialRequests")) ? null : reader.GetString(reader.GetOrdinal("SpecialRequests")),
                        DishName = reader.IsDBNull(reader.GetOrdinal("DishName")) ? null : reader.GetString(reader.GetOrdinal("DishName"))
                    });
                }
                
                _logger.LogInformation($"成功查询到订单 {orderId} 的 {details.Count} 个详情项");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查询订单 {orderId} 的详情失败");
            }
            
            return details;
        }

        /// <summary>
        /// 创建订单
        /// </summary>
        public async Task<int?> CreateOrderAsync(Orders order)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();
                
                var sql = @"INSERT INTO PUB.Orders (OrderID, OrderTime, TableID, CustomerID, TotalPrice, 
                                                   OrderStatus, StoreID, CreateTime, UpdateTime)
                           VALUES (PUB.seq_order_id.NEXTVAL, :OrderTime, :TableID, :CustomerID, :TotalPrice, 
                                  :OrderStatus, :StoreID, SYSDATE, SYSDATE)
                           RETURNING OrderID INTO :OrderID";
                
                using var command = new OracleCommand(sql, connection);
                command.Parameters.Add(":OrderTime", OracleDbType.Date).Value = order.OrderTime;
                command.Parameters.Add(":TableID", OracleDbType.Int32).Value = order.TableID ?? (object)DBNull.Value;
                command.Parameters.Add(":CustomerID", OracleDbType.Int32).Value = order.CustomerID ?? (object)DBNull.Value;
                command.Parameters.Add(":TotalPrice", OracleDbType.Decimal).Value = order.TotalPrice;
                command.Parameters.Add(":OrderStatus", OracleDbType.Varchar2).Value = order.OrderStatus;
                command.Parameters.Add(":StoreID", OracleDbType.Int32).Value = order.StoreID ?? (object)DBNull.Value;
                
                var orderIdParam = new OracleParameter(":OrderID", OracleDbType.Decimal)
                {
                    Direction = System.Data.ParameterDirection.Output
                };
                command.Parameters.Add(orderIdParam);
                
                await command.ExecuteNonQueryAsync();
                
                // 正确处理Oracle返回的OracleDecimal类型
                var oracleDecimal = (Oracle.ManagedDataAccess.Types.OracleDecimal)orderIdParam.Value;
                var newOrderId = oracleDecimal.ToInt32();
                _logger.LogInformation($"成功创建订单，订单ID: {newOrderId}");
                
                // 如果有客户ID，自动添加积分记录
                if (order.CustomerID.HasValue && order.TotalPrice > 0)
                {
                    await AddPointsForOrderAsync(order.CustomerID.Value, newOrderId, order.TotalPrice);
                }
                
                return newOrderId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建订单失败");
                return null;
            }
        }

        /// <summary>
        /// 为订单添加积分（内部方法）
        /// </summary>
        private async Task<bool> AddPointsForOrderAsync(int customerId, int orderId, decimal orderAmount)
        {
            try
            {
                // 检查是否已有积分记录，避免重复添加
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();
                
                var checkSql = @"SELECT COUNT(*) FROM PUB.PointsRecord WHERE OrderID = :orderId";
                using var checkCommand = new OracleCommand(checkSql, connection);
                checkCommand.Parameters.Add(":orderId", OracleDbType.Int32).Value = orderId;
                
                var existingCount = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
                if (existingCount > 0)
                {
                    _logger.LogInformation($"订单 {orderId} 已有积分记录，跳过添加");
                    return true;
                }
                
                // 计算积分（假设1元=1积分）
                int pointsEarned = (int)Math.Floor(orderAmount);
                
                if (pointsEarned <= 0)
                {
                    _logger.LogInformation($"订单金额 {orderAmount} 不足以获得积分");
                    return true;
                }
                
                // 添加积分记录
                var sql = @"INSERT INTO PUB.PointsRecord (RecordID, CustomerID, OrderID, PointsChange, RecordType, RecordTime, Description)
                           VALUES (PUB.seq_points_record_id.NEXTVAL, :CustomerID, :OrderID, :PointsChange, :RecordType, :RecordTime, :Description)";
                
                using var command = new OracleCommand(sql, connection);
                command.Parameters.Add(":CustomerID", OracleDbType.Int32).Value = customerId;
                command.Parameters.Add(":OrderID", OracleDbType.Int32).Value = orderId;
                command.Parameters.Add(":PointsChange", OracleDbType.Int32).Value = pointsEarned;
                command.Parameters.Add(":RecordType", OracleDbType.Varchar2).Value = "消费获得";
                command.Parameters.Add(":RecordTime", OracleDbType.Date).Value = DateTime.Now;
                command.Parameters.Add(":Description", OracleDbType.Varchar2).Value = $"订单消费获得积分，消费金额: ¥{orderAmount:F2}";
                
                int rowsAffected = await command.ExecuteNonQueryAsync();
                
                if (rowsAffected > 0)
                {
                    // 更新客户积分余额
                    var updateSql = @"UPDATE PUB.Customer SET VIPPoints = VIPPoints + :pointsChange WHERE CustomerID = :customerId";
                    using var updateCommand = new OracleCommand(updateSql, connection);
                    updateCommand.Parameters.Add(":pointsChange", OracleDbType.Int32).Value = pointsEarned;
                    updateCommand.Parameters.Add(":customerId", OracleDbType.Int32).Value = customerId;
                    
                    await updateCommand.ExecuteNonQueryAsync();
                    
                    _logger.LogInformation($"订单 {orderId} 消费 {orderAmount} 元，获得 {pointsEarned} 积分");
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"为订单 {orderId} 添加积分失败");
                return false;
            }
        }

        /// <summary>
        /// 添加订单详情
        /// </summary>
        public async Task<bool> AddOrderDetailAsync(OrderDetail detail)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();
                
                // 先检查序列是否存在并获取下一个值
                var checkSeqSql = @"SELECT PUB.seq_order_detail_id.NEXTVAL FROM DUAL";
                using var checkCommand = new OracleCommand(checkSeqSql, connection);
                var nextIdResult = await checkCommand.ExecuteScalarAsync();
                var nextId = Convert.ToInt32(nextIdResult); // 直接转换为int
                
                var sql = @"INSERT INTO PUB.OrderDetail (OrderDetailID, OrderID, DishID, Quantity, UnitPrice, Subtotal, SpecialRequests)
                           VALUES (:OrderDetailID, :OrderID, :DishID, :Quantity, :UnitPrice, :Subtotal, :SpecialRequests)";
                
                using var command = new OracleCommand(sql, connection);
                command.Parameters.Add(":OrderDetailID", OracleDbType.Int32).Value = nextId;
                command.Parameters.Add(":OrderID", OracleDbType.Int32).Value = detail.OrderID;
                command.Parameters.Add(":DishID", OracleDbType.Int32).Value = detail.DishID;
                command.Parameters.Add(":Quantity", OracleDbType.Int32).Value = detail.Quantity;
                command.Parameters.Add(":UnitPrice", OracleDbType.Decimal).Value = detail.UnitPrice;
                command.Parameters.Add(":Subtotal", OracleDbType.Decimal).Value = detail.Subtotal;
                command.Parameters.Add(":SpecialRequests", OracleDbType.Varchar2).Value = detail.SpecialRequests ?? (object)DBNull.Value;
                
                int rowsAffected = await command.ExecuteNonQueryAsync();
                _logger.LogInformation($"成功添加订单详情，影响行数: {rowsAffected}");
                
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加订单详情失败");
                return false;
            }
        }

        /// <summary>
        /// 更新订单状态
        /// </summary>
        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();
                
                var sql = @"UPDATE PUB.Orders SET OrderStatus = :status, UpdateTime = SYSDATE WHERE OrderID = :orderId";
                
                using var command = new OracleCommand(sql, connection);
                command.Parameters.Add(":status", OracleDbType.Varchar2).Value = status;
                command.Parameters.Add(":orderId", OracleDbType.Int32).Value = orderId;
                
                int rowsAffected = await command.ExecuteNonQueryAsync();
                _logger.LogInformation($"成功更新订单 {orderId} 状态为 {status}，影响行数: {rowsAffected}");
                
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新订单 {orderId} 状态失败");
                return false;
            }
        }

        /// <summary>
        /// 删除订单（同时删除相关详情）
        /// </summary>
        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();
                
                using var transaction = connection.BeginTransaction();
                
                try
                {
                    // 先删除订单详情
                    var deleteDetailsSql = @"DELETE FROM PUB.OrderDetail WHERE OrderID = :orderId";
                    using var deleteDetailsCommand = new OracleCommand(deleteDetailsSql, connection);
                    deleteDetailsCommand.Transaction = transaction;
                    deleteDetailsCommand.Parameters.Add(":orderId", OracleDbType.Int32).Value = orderId;
                    
                    int detailsDeleted = await deleteDetailsCommand.ExecuteNonQueryAsync();
                    
                    // 再删除订单主记录
                    var deleteOrderSql = @"DELETE FROM PUB.Orders WHERE OrderID = :orderId";
                    using var deleteOrderCommand = new OracleCommand(deleteOrderSql, connection);
                    deleteOrderCommand.Transaction = transaction;
                    deleteOrderCommand.Parameters.Add(":orderId", OracleDbType.Int32).Value = orderId;
                    
                    int orderDeleted = await deleteOrderCommand.ExecuteNonQueryAsync();
                    
                    transaction.Commit();
                    
                    _logger.LogInformation($"成功删除订单 {orderId}，删除了 {detailsDeleted} 个详情项和 {orderDeleted} 个主记录");
                    return orderDeleted > 0;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除订单 {orderId} 失败");
                return false;
            }
        }

        /// <summary>
        /// 获取所有订单（分页）
        /// </summary>
        public async Task<List<Orders>> GetAllOrdersAsync(int pageSize = 10, int pageNumber = 1)
        {
            var orders = new List<Orders>();
            
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();
                
                var sql = @"SELECT * FROM (
                               SELECT o.OrderID, o.OrderTime, o.TableID, o.CustomerID, o.TotalPrice, 
                                      o.OrderStatus, o.StoreID, o.CreateTime, o.UpdateTime,
                                      c.CustomerName, s.StoreName, t.TableNumber,
                                      ROW_NUMBER() OVER (ORDER BY o.OrderTime DESC) AS rn
                               FROM PUB.Orders o
                               LEFT JOIN PUB.Customer c ON o.CustomerID = c.CustomerID
                               LEFT JOIN PUB.Store s ON o.StoreID = s.StoreID
                               LEFT JOIN PUB.TableInfo t ON o.TableID = t.TableID
                            ) WHERE rn BETWEEN :startRow AND :endRow";
                
                int startRow = (pageNumber - 1) * pageSize + 1;
                int endRow = pageNumber * pageSize;
                
                using var command = new OracleCommand(sql, connection);
                command.Parameters.Add(":startRow", OracleDbType.Int32).Value = startRow;
                command.Parameters.Add(":endRow", OracleDbType.Int32).Value = endRow;
                
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    orders.Add(new Orders
                    {
                        OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                        OrderTime = reader.GetDateTime(reader.GetOrdinal("OrderTime")),
                        TableID = reader.IsDBNull(reader.GetOrdinal("TableID")) ? null : reader.GetInt32(reader.GetOrdinal("TableID")),
                        CustomerID = reader.IsDBNull(reader.GetOrdinal("CustomerID")) ? null : reader.GetInt32(reader.GetOrdinal("CustomerID")),
                        TotalPrice = reader.GetDecimal(reader.GetOrdinal("TotalPrice")),
                        OrderStatus = reader.GetString(reader.GetOrdinal("OrderStatus")),
                        StaffID = null, // 字段不存在，设为null
                        StoreID = reader.IsDBNull(reader.GetOrdinal("StoreID")) ? null : reader.GetInt32(reader.GetOrdinal("StoreID")),
                        CreateTime = reader.GetDateTime(reader.GetOrdinal("CreateTime")),
                        UpdateTime = reader.GetDateTime(reader.GetOrdinal("UpdateTime")),
                        CustomerName = reader.IsDBNull(reader.GetOrdinal("CustomerName")) ? null : reader.GetString(reader.GetOrdinal("CustomerName")),
                        StoreName = reader.IsDBNull(reader.GetOrdinal("StoreName")) ? null : reader.GetString(reader.GetOrdinal("StoreName")),
                        StaffName = null, // StaffID字段不存在，StaffName也设为null
                        TableNumber = reader.IsDBNull(reader.GetOrdinal("TableNumber")) ? null : reader.GetString(reader.GetOrdinal("TableNumber"))
                    });
                }
                
                _logger.LogInformation($"成功查询到第 {pageNumber} 页的 {orders.Count} 个订单");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询订单列表失败");
            }
            
            return orders;
        }

        /// <summary>
        /// 创建测试订单和详情
        /// </summary>
        public async Task<bool> CreateTestOrderWithDetailsAsync()
        {
            try
            {
                // 检查是否已有订单，避免重复创建
                var existingOrders = await GetAllOrdersAsync(1, 1);
                if (existingOrders.Count > 0)
                {
                    _logger.LogInformation("订单已存在，跳过创建");
                    return true;
                }

                // 使用随机数避免唯一约束冲突
                var random = new Random();
                var randomSuffix = random.Next(1000, 9999);
                
                // 创建测试订单
                var testOrder = new Orders
                {
                    OrderTime = DateTime.Now.AddSeconds(randomSuffix), 
                    CustomerID = 1, 
                    TotalPrice = 45.80m + (randomSuffix / 100m), 
                    OrderStatus = "待处理",
                    StoreID = 1, 
                    TableID = null  
                };

                var orderId = await CreateOrderAsync(testOrder);
                
                if (orderId.HasValue)
                {
                    // 现在尝试添加订单详情，使用现有的菜品ID
                    var detail = new OrderDetail
                    {
                        OrderID = orderId.Value,
                        DishID = 2, // 使用确实存在的菜品ID=2
                        Quantity = 2,
                        UnitPrice = 22.90m,
                        Subtotal = 45.80m, // 2 * 22.90
                        SpecialRequests = $"测试详情-{DateTime.Now:HHmmss}-{randomSuffix}"
                    };

                    bool detailAdded = await AddOrderDetailAsync(detail);

                    _logger.LogInformation($"创建测试订单完成：订单ID={orderId}, 详情添加={detailAdded}");
                    return detailAdded;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建测试订单失败");
                return false;
            }
        }

        /// <summary>
        /// 为现有订单批量添加积分记录
        /// </summary>
        public async Task<bool> AddPointsForExistingOrdersAsync(int customerId)
        {
            try
            {
                _logger.LogInformation($"开始为客户 {customerId} 的现有订单添加积分记录");
                
                // 获取客户的所有订单
                var orders = await GetOrdersByCustomerIdAsync(customerId);
                int addedCount = 0;
                
                foreach (var order in orders)
                {
                    if (order.TotalPrice > 0)
                    {
                        bool success = await AddPointsForOrderAsync(customerId, order.OrderID, order.TotalPrice);
                        if (success)
                        {
                            addedCount++;
                        }
                    }
                }
                
                _logger.LogInformation($"为客户 {customerId} 的 {orders.Count} 个订单添加了 {addedCount} 条积分记录");
                return addedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"为客户 {customerId} 的现有订单添加积分记录失败");
                return false;
            }
        }
    }
}