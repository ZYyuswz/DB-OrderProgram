using ConsoleApp1.Models;
using ConsoleApp1.Controllers;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Configuration;

namespace ConsoleApp1.Services
{
    public class CustomerService
    {
        private readonly ILogger<CustomerService> _logger;
        private readonly string _connectionString;

        public CustomerService(ILogger<CustomerService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _connectionString = configuration.GetConnectionString("OracleConnection")
                ?? throw new ArgumentNullException("Oracle connection string not found");
        }

        /// <summary>
        /// 获取客户档案信息
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <returns>客户档案信息</returns>
        public async Task<CustomerProfileInfo?> GetCustomerProfileAsync(int customerId)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT 
                        c.CustomerID,
                        c.CustomerName,
                        c.Phone,
                        c.Email,
                        c.VIPLevel,
                        c.TotalConsumption,
                        c.RegisterTime,
                        CASE 
                            WHEN c.VIPLevel = 1 THEN '青铜会员'
                            WHEN c.VIPLevel = 2 THEN '白银会员'
                            WHEN c.VIPLevel = 3 THEN '黄金会员'
                            WHEN c.VIPLevel = 4 THEN '白金会员'
                            WHEN c.VIPLevel = 5 THEN '钻石会员'
                            ELSE '普通会员'
                        END AS VipLevelName
                    FROM PUB.Customer c
                    WHERE c.CustomerID = :CustomerId AND c.Status = '正常'";

                using var command = new OracleCommand(query, connection);
                command.Parameters.Add(":CustomerId", OracleDbType.Int32).Value = customerId;

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var customerIdFromDb = reader.GetInt32(reader.GetOrdinal("CustomerID"));
                    var totalConsumption = reader.IsDBNull(reader.GetOrdinal("TotalConsumption")) ? 0 : reader.GetDecimal(reader.GetOrdinal("TotalConsumption"));
                    var currentVipLevel = reader.IsDBNull(reader.GetOrdinal("VIPLevel")) ? 1 : reader.GetInt32(reader.GetOrdinal("VIPLevel"));

                    // 根据消费总额计算实际应该的等级
                    var calculatedLevel = CalculateLevelByConsumption(totalConsumption);
                    var calculatedLevelCode = GetLevelCode(calculatedLevel);

                    // 如果计算出的等级与数据库中的等级不一致，更新数据库
                    if (calculatedLevelCode != currentVipLevel)
                    {
                        _logger.LogInformation($"客户 {customerIdFromDb} 的会员等级需要更新: {currentVipLevel} -> {calculatedLevelCode}");
                        await UpdateCustomerVipLevelInDb(customerIdFromDb, calculatedLevelCode);
                        currentVipLevel = calculatedLevelCode; // 使用更新后的等级
                    }

                    return new CustomerProfileInfo
                    {
                        CustomerId = customerIdFromDb,
                        CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                        Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                        Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                        VipLevelName = GetLevelName(currentVipLevel),
                        RegisterTime = reader.GetDateTime(reader.GetOrdinal("RegisterTime"))
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取客户 {customerId} 档案信息失败");
                throw;
            }
        }

        /// <summary>
        /// 更新客户档案信息
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <param name="updateInfo">更新信息</param>
        /// <returns>是否更新成功</returns>
        public async Task<bool> UpdateCustomerProfileAsync(int customerId, CustomerUpdateInfo updateInfo)
        {
            // 1. 记录方法入口和接收到的原始数据
            _logger.LogInformation("Attempting to update profile for CustomerID: {CustomerId}", customerId);
            _logger.LogInformation("Received update data: CustomerName='{Name}', Phone='{Phone}', Email='{Email}'",
                                   updateInfo.CustomerName, updateInfo.Phone, updateInfo.Email);

            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();
                _logger.LogInformation("Database connection opened successfully.");

                // 2. 构建 SQL 查询
                // 我们保持这个查询不变，因为它在逻辑上是正确的
                var query = @"
            UPDATE PUB.Customer 
            SET 
                CustomerName = :CustomerName,
                Phone = :Phone,
                Email = :Email
            WHERE CustomerID = :CustomerId AND Status = '正常'";

                using var command = new OracleCommand(query, connection);

                // 3. 安全地绑定参数
                // 这里我们显式地处理 null 和空字符串，增加代码的明确性
                command.Parameters.Add(":CustomerId", OracleDbType.Int32).Value = customerId;

                command.Parameters.Add(":CustomerName", OracleDbType.Varchar2).Value =
                    string.IsNullOrEmpty(updateInfo.CustomerName) ? (object)DBNull.Value : updateInfo.CustomerName;

                command.Parameters.Add(":Phone", OracleDbType.Varchar2).Value =
                    string.IsNullOrEmpty(updateInfo.Phone) ? (object)DBNull.Value : updateInfo.Phone;

                command.Parameters.Add(":Email", OracleDbType.Varchar2).Value =
                    string.IsNullOrEmpty(updateInfo.Email) ? (object)DBNull.Value : updateInfo.Email;


                // 4. 执行前的终极诊断日志 (这是最重要的部分)
                _logger.LogInformation("------------------- Pre-Execution Diagnosis -------------------");
                _logger.LogInformation("Executing SQL Command: {SQL}", command.CommandText);

                foreach (OracleParameter p in command.Parameters)
                {
                    // 对参数值进行安全处理，以防日志记录本身出错
                    string paramValueStr;
                    if (p.Value == null || p.Value == DBNull.Value)
                    {
                        paramValueStr = "[DBNull]";
                    }
                    else
                    {
                        paramValueStr = p.Value.ToString();
                    }
                    _logger.LogInformation("  -> Param: {Name} | Type: {Type} | Value: '{Value}'",
                                           p.ParameterName, p.OracleDbType, paramValueStr);
                }
                _logger.LogInformation("-------------------------------------------------------------");


                // 5. 执行命令 (你的 line 121 就在这里或附近)
                var rowsAffected = await command.ExecuteNonQueryAsync();

                // 6. 记录执行结果
                if (rowsAffected > 0)
                {
                    _logger.LogInformation("Successfully updated profile for CustomerID: {CustomerId}. Rows affected: {Rows}", customerId, rowsAffected);
                }
                else
                {
                    _logger.LogWarning("Update command executed but no rows were affected for CustomerID: {CustomerId}. The customer might not exist or status is not '正常'.", customerId);
                }

                return rowsAffected > 0;
            }
            catch (OracleException oraEx)
            {
                // 7. 捕获并记录详细的 Oracle 异常
                _logger.LogError(oraEx, "An OracleException occurred while updating CustomerID {CustomerId}. Error Number: {ErrorNumber}",
                                 customerId, oraEx.Number);
                // 将原始异常再次抛出，以便上层可以捕获到500错误
                throw;
            }
            catch (Exception ex)
            {
                // 8. 捕获其他类型的异常
                _logger.LogError(ex, "A general exception occurred while updating CustomerID {CustomerId}.", customerId);
                throw;
            }
        }

        /// <summary>
        /// 根据消费总额计算会员等级
        /// </summary>
        /// <param name="totalConsumption">累计消费金额</param>
        /// <returns>等级字符串</returns>
        private string CalculateLevelByConsumption(decimal totalConsumption)
        {
            if (totalConsumption >= 1000) return "diamond";      // 钻石会员: ≥1000
            if (totalConsumption >= 500) return "platinum";      // 白金会员: 500-999.99
            if (totalConsumption >= 200) return "gold";          // 黄金会员: 200-499.99
            if (totalConsumption >= 100) return "silver";        // 白银会员: 100-199.99
            return "bronze";                                      // 青铜会员: 0-99.99
        }

        /// <summary>
        /// 将等级字符串转换为数值
        /// </summary>
        /// <param name="levelString">等级字符串</param>
        /// <returns>等级数值</returns>
        private int GetLevelCode(string levelString)
        {
            return levelString switch
            {
                "bronze" => 1,
                "silver" => 2,
                "gold" => 3,
                "platinum" => 4,
                "diamond" => 5,
                _ => 1
            };
        }

        /// <summary>
        /// 将等级数值转换为名称
        /// </summary>
        /// <param name="levelCode">等级数值</param>
        /// <returns>等级名称</returns>
        private string GetLevelName(int levelCode)
        {
            return levelCode switch
            {
                1 => "青铜会员",
                2 => "白银会员",
                3 => "黄金会员",
                4 => "白金会员",
                5 => "钻石会员",
                _ => "普通会员"
            };
        }

        /// <summary>
        /// 更新数据库中的客户VIP等级
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <param name="newLevelCode">新等级代码</param>
        /// <returns>是否更新成功</returns>
        private async Task<bool> UpdateCustomerVipLevelInDb(int customerId, int newLevelCode)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();

                var updateSql = "UPDATE PUB.Customer SET VIPLevel = :VIPLevel WHERE CustomerID = :CustomerID";
                using var command = new OracleCommand(updateSql, connection);
                command.Parameters.Add(":CustomerID", OracleDbType.Int32).Value = customerId;
                command.Parameters.Add(":VIPLevel", OracleDbType.Int32).Value = newLevelCode;

                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新客户 {customerId} VIP等级失败");
                return false;
            }
        }
    }
}