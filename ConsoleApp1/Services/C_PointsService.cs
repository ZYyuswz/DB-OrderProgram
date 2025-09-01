using Oracle.ManagedDataAccess.Client;
using ConsoleApp1.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConsoleApp1.Services
{
    /// <summary>
    /// 积分服务类
    /// </summary>
    public class PointsService
    {
        private readonly string _connectionString;
        private readonly ILogger<PointsService> _logger;

        public PointsService(IConfiguration configuration, ILogger<PointsService> logger)
        {
            _connectionString = configuration.GetConnectionString("OracleConnection")
                ?? throw new ArgumentNullException("Oracle connection string not found");
            _logger = logger;
        }

        /// <summary>
        /// 获取客户的积分记录
        /// </summary>
        public async Task<List<PointsRecord>> GetCustomerPointsRecordsAsync(int customerId, int page = 1, int pageSize = 10)
        {
            var records = new List<PointsRecord>();

            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"SELECT * FROM (
                               SELECT pr.RecordID, pr.CustomerID, pr.OrderID, pr.PointsChange, 
                                      pr.RecordType, pr.RecordTime, pr.Description,
                                      c.CustomerName, o.TotalPrice as OrderAmount, 
                                      o.OrderTime, s.StoreName,
                                      ROW_NUMBER() OVER (ORDER BY pr.RecordTime DESC) AS rn
                               FROM PUB.PointsRecord pr
                               LEFT JOIN PUB.Customer c ON pr.CustomerID = c.CustomerID
                               LEFT JOIN PUB.Orders o ON pr.OrderID = o.OrderID
                               LEFT JOIN PUB.Store s ON o.StoreID = s.StoreID
                               WHERE pr.CustomerID = :customerId
                            ) WHERE rn BETWEEN :startRow AND :endRow
                            ORDER BY RecordTime DESC";

                int startRow = (page - 1) * pageSize + 1;
                int endRow = page * pageSize;

                using var command = new OracleCommand(sql, connection);
                command.Parameters.Add(":customerId", OracleDbType.Int32).Value = customerId;
                command.Parameters.Add(":startRow", OracleDbType.Int32).Value = startRow;
                command.Parameters.Add(":endRow", OracleDbType.Int32).Value = endRow;

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    records.Add(new PointsRecord
                    {
                        RecordID = reader.GetInt32(reader.GetOrdinal("RecordID")),
                        CustomerID = reader.IsDBNull(reader.GetOrdinal("CustomerID")) ? null : reader.GetInt32(reader.GetOrdinal("CustomerID")),
                        OrderID = reader.IsDBNull(reader.GetOrdinal("OrderID")) ? null : reader.GetInt32(reader.GetOrdinal("OrderID")),
                        PointsChange = reader.GetInt32(reader.GetOrdinal("PointsChange")),
                        RecordType = reader.GetString(reader.GetOrdinal("RecordType")),
                        RecordTime = reader.GetDateTime(reader.GetOrdinal("RecordTime")),
                        Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                        CustomerName = reader.IsDBNull(reader.GetOrdinal("CustomerName")) ? null : reader.GetString(reader.GetOrdinal("CustomerName")),
                        OrderAmount = reader.IsDBNull(reader.GetOrdinal("OrderAmount")) ? null : reader.GetDecimal(reader.GetOrdinal("OrderAmount")),
                        OrderTime = reader.IsDBNull(reader.GetOrdinal("OrderTime")) ? null : reader.GetDateTime(reader.GetOrdinal("OrderTime")).ToString("yyyy-MM-dd HH:mm:ss"),
                        StoreName = reader.IsDBNull(reader.GetOrdinal("StoreName")) ? null : reader.GetString(reader.GetOrdinal("StoreName"))
                    });
                }

                _logger.LogInformation($"成功查询到客户 {customerId} 的 {records.Count} 条积分记录");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查询客户 {customerId} 的积分记录失败");
            }

            return records;
        }

        /// <summary>
        /// 获取客户当前积分余额
        /// </summary>
        public async Task<int> GetCustomerPointsBalanceAsync(int customerId)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"SELECT VIPPoints FROM PUB.Customer WHERE CustomerID = :customerId";

                using var command = new OracleCommand(sql, connection);
                command.Parameters.Add(":customerId", OracleDbType.Int32).Value = customerId;

                var result = await command.ExecuteScalarAsync();

                if (result != null && result != DBNull.Value)
                {
                    var points = Convert.ToInt32(result);
                    _logger.LogInformation($"客户 {customerId} 当前积分余额: {points}");
                    return points;
                }

                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"查询客户 {customerId} 积分余额失败");
                return 0;
            }
        }

        /// <summary>
        /// 添加积分记录
        /// </summary>
        public async Task<bool> AddPointsRecordAsync(PointsRecord record)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"INSERT INTO PUB.PointsRecord (RecordID, CustomerID, OrderID, PointsChange, RecordType, RecordTime, Description)
                           VALUES (PUB.seq_points_record_id.NEXTVAL, :CustomerID, :OrderID, :PointsChange, :RecordType, :RecordTime, :Description)";

                using var command = new OracleCommand(sql, connection);
                command.Parameters.Add(":CustomerID", OracleDbType.Int32).Value = record.CustomerID ?? (object)DBNull.Value;
                command.Parameters.Add(":OrderID", OracleDbType.Int32).Value = record.OrderID ?? (object)DBNull.Value;
                command.Parameters.Add(":PointsChange", OracleDbType.Int32).Value = record.PointsChange;
                command.Parameters.Add(":RecordType", OracleDbType.Varchar2).Value = record.RecordType;
                command.Parameters.Add(":RecordTime", OracleDbType.Date).Value = record.RecordTime;
                command.Parameters.Add(":Description", OracleDbType.Varchar2).Value = record.Description ?? (object)DBNull.Value;

                int rowsAffected = await command.ExecuteNonQueryAsync();
                _logger.LogInformation($"成功添加积分记录，影响行数: {rowsAffected}");

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加积分记录失败");
                return false;
            }
        }

        /// <summary>
        /// 更新客户积分余额
        /// </summary>
        public async Task<bool> UpdateCustomerPointsAsync(int customerId, int pointsChange)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"UPDATE PUB.Customer SET VIPPoints = VIPPoints + :pointsChange WHERE CustomerID = :customerId";

                using var command = new OracleCommand(sql, connection);
                command.Parameters.Add(":pointsChange", OracleDbType.Int32).Value = pointsChange;
                command.Parameters.Add(":customerId", OracleDbType.Int32).Value = customerId;

                int rowsAffected = await command.ExecuteNonQueryAsync();
                _logger.LogInformation($"成功更新客户 {customerId} 积分，变动: {pointsChange}，影响行数: {rowsAffected}");

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新客户 {customerId} 积分失败");
                return false;
            }
        }

        /// <summary>
        /// 根据订单自动计算并添加积分
        /// </summary>
        public async Task<bool> AddPointsForOrderAsync(int customerId, int orderId, decimal orderAmount)
        {
            try
            {
                // 计算积分（假设1元=1积分）
                int pointsEarned = (int)Math.Floor(orderAmount);

                if (pointsEarned <= 0)
                {
                    _logger.LogInformation($"订单金额 {orderAmount} 不足以获得积分");
                    return true;
                }

                // 添加积分记录
                var pointsRecord = new PointsRecord
                {
                    CustomerID = customerId,
                    OrderID = orderId,
                    PointsChange = pointsEarned,
                    RecordType = "消费获得",
                    RecordTime = DateTime.Now,
                    Description = $"订单消费获得积分，消费金额: ¥{orderAmount:F2}"
                };

                bool recordAdded = await AddPointsRecordAsync(pointsRecord);
                if (!recordAdded)
                {
                    return false;
                }

                // 更新客户积分余额
                bool balanceUpdated = await UpdateCustomerPointsAsync(customerId, pointsEarned);

                _logger.LogInformation($"订单 {orderId} 消费 {orderAmount} 元，获得 {pointsEarned} 积分");
                return balanceUpdated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"为订单 {orderId} 添加积分失败");
                return false;
            }
        }

        /// <summary>
        /// 创建测试积分记录
        /// </summary>
        public async Task<bool> CreateTestPointsRecordAsync()
        {
            try
            {
                // 检查是否已有积分记录，避免重复创建
                var existingRecords = await GetCustomerPointsRecordsAsync(1, 1, 1);
                if (existingRecords.Count > 0)
                {
                    _logger.LogInformation("积分记录已存在，跳过创建");
                    return true;
                }

                // 为测试客户1创建一些积分记录
                var testRecords = new List<PointsRecord>
                {
                    new PointsRecord
                    {
                        CustomerID = 1,
                        OrderID = 1,
                        PointsChange = 168,
                        RecordType = "消费获得",
                        RecordTime = DateTime.Now.AddDays(-2),
                        Description = "订单消费获得积分，消费金额: ¥168.50"
                    },
                    new PointsRecord
                    {
                        CustomerID = 1,
                        OrderID = 2,
                        PointsChange = 89,
                        RecordType = "消费获得",
                        RecordTime = DateTime.Now.AddDays(-3),
                        Description = "订单消费获得积分，消费金额: ¥89.80"
                    },
                    new PointsRecord
                    {
                        CustomerID = 1,
                        OrderID = 3,
                        PointsChange = 245,
                        RecordType = "消费获得",
                        RecordTime = DateTime.Now.AddDays(-4),
                        Description = "订单消费获得积分，消费金额: ¥245.60"
                    }
                };

                foreach (var record in testRecords)
                {
                    await AddPointsRecordAsync(record);
                }

                // 更新客户积分余额
                int totalPoints = testRecords.Sum(r => r.PointsChange);
                await UpdateCustomerPointsAsync(1, totalPoints);

                _logger.LogInformation($"成功创建测试积分记录，总积分: {totalPoints}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建测试积分记录失败");
                return false;
            }
        }
    }
}