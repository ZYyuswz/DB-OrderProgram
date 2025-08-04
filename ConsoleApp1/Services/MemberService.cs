using ConsoleApp1.Models;
using ConsoleApp1.Controllers;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Configuration;

namespace ConsoleApp1.Services
{
    public class MemberService
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<MemberService> _logger;
        private readonly string _connectionString;

        public MemberService(DatabaseService databaseService, ILogger<MemberService> logger, IConfiguration configuration)
        {
            _databaseService = databaseService;
            _logger = logger;
            _connectionString = configuration.GetConnectionString("OracleConnection") 
                ?? throw new ArgumentNullException("Oracle connection string not found");
        }

        /// <summary>
        /// 获取客户会员信息
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <returns>会员信息</returns>
        public async Task<MemberInfo?> GetCustomerMemberInfoAsync(int customerId)
        {
            try
            {
                // 先更新客户的累计消费金额，确保数据是最新的
                await UpdateCustomerTotalConsumptionAsync(customerId);
                
                // 获取客户基本信息
                var customer = await GetCustomerAsync(customerId);
                if (customer == null)
                {
                    return null;
                }

                // 获取会员等级规则
                var levels = await GetMemberLevelsAsync();
                
                // 计算当前等级和下一等级
                var currentLevel = CalculateCurrentLevel(customer.TotalConsumption, levels);
                var nextLevel = CalculateNextLevel(customer.TotalConsumption, levels);

                // 计算升级进度
                var progressToNextLevel = CalculateProgressToNextLevel(customer.TotalConsumption, currentLevel, nextLevel);

                var memberInfo = new MemberInfo
                {
                    CustomerId = customer.CustomerID,
                    CustomerName = customer.CustomerName,
                    TotalConsumption = customer.TotalConsumption,
                    CurrentLevel = currentLevel.LevelCode,
                    CurrentLevelName = currentLevel.LevelName,
                    NextLevel = nextLevel?.LevelCode ?? "",
                    NextLevelName = nextLevel?.LevelName ?? "已达最高等级",
                    NextLevelThreshold = nextLevel?.MinConsumption ?? 0,
                    ProgressToNextLevel = progressToNextLevel,
                    VipPoints = customer.VIPPoints,
                    RegisterTime = customer.RegisterTime,
                    Privileges = currentLevel.Privileges
                };

                return memberInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取客户 {customerId} 会员信息失败");
                throw;
            }
        }

        /// <summary>
        /// 获取会员等级规则
        /// </summary>
        /// <returns>会员等级列表</returns>
        public async Task<List<MemberLevel>> GetMemberLevelsAsync()
        {
            return await Task.FromResult(new List<MemberLevel>
            {
                new MemberLevel
                {
                    LevelCode = "bronze",
                    LevelName = "青铜会员",
                    MinConsumption = 0,
                    MaxConsumption = 99.99m,
                    LevelColor = "#CD7F32",
                    LevelIcon = "🥉",
                    Privileges = new List<MemberPrivilege>
                    {
                        new MemberPrivilege
                        {
                            PrivilegeType = "discount",
                            PrivilegeName = "新人优惠",
                            PrivilegeDesc = "享受9.5折优惠",
                            PrivilegeValue = "95%",
                            PrivilegeIcon = "💰"
                        },
                        new MemberPrivilege
                        {
                            PrivilegeType = "points",
                            PrivilegeName = "积分奖励",
                            PrivilegeDesc = "消费1元获得1积分",
                            PrivilegeValue = "1:1",
                            PrivilegeIcon = "⭐"
                        }
                    }
                },
                new MemberLevel
                {
                    LevelCode = "silver",
                    LevelName = "白银会员",
                    MinConsumption = 100,
                    MaxConsumption = 499.99m,
                    LevelColor = "#C0C0C0",
                    LevelIcon = "🥈",
                    Privileges = new List<MemberPrivilege>
                    {
                        new MemberPrivilege
                        {
                            PrivilegeType = "discount",
                            PrivilegeName = "会员折扣",
                            PrivilegeDesc = "享受9折优惠",
                            PrivilegeValue = "90%",
                            PrivilegeIcon = "💰"
                        },
                        new MemberPrivilege
                        {
                            PrivilegeType = "points",
                            PrivilegeName = "积分奖励",
                            PrivilegeDesc = "消费1元获得1.2积分",
                            PrivilegeValue = "1:1.2",
                            PrivilegeIcon = "⭐"
                        },
                        new MemberPrivilege
                        {
                            PrivilegeType = "birthday",
                            PrivilegeName = "生日特权",
                            PrivilegeDesc = "生日当月享受额外8折优惠",
                            PrivilegeValue = "80%",
                            PrivilegeIcon = "🎂"
                        }
                    }
                },
                new MemberLevel
                {
                    LevelCode = "gold",
                    LevelName = "黄金会员",
                    MinConsumption = 500,
                    MaxConsumption = 999.99m,
                    LevelColor = "#FFD700",
                    LevelIcon = "🥇",
                    Privileges = new List<MemberPrivilege>
                    {
                        new MemberPrivilege
                        {
                            PrivilegeType = "discount",
                            PrivilegeName = "黄金折扣",
                            PrivilegeDesc = "享受8.5折优惠",
                            PrivilegeValue = "85%",
                            PrivilegeIcon = "💰"
                        },
                        new MemberPrivilege
                        {
                            PrivilegeType = "points",
                            PrivilegeName = "积分奖励",
                            PrivilegeDesc = "消费1元获得1.5积分",
                            PrivilegeValue = "1:1.5",
                            PrivilegeIcon = "⭐"
                        },
                        new MemberPrivilege
                        {
                            PrivilegeType = "birthday",
                            PrivilegeName = "生日特权",
                            PrivilegeDesc = "生日当月享受额外7.5折优惠",
                            PrivilegeValue = "75%",
                            PrivilegeIcon = "🎂"
                        },
                        new MemberPrivilege
                        {
                            PrivilegeType = "priority",
                            PrivilegeName = "优先服务",
                            PrivilegeDesc = "享受优先制作服务",
                            PrivilegeValue = "优先",
                            PrivilegeIcon = "⚡"
                        }
                    }
                },
                new MemberLevel
                {
                    LevelCode = "platinum",
                    LevelName = "铂金会员",
                    MinConsumption = 1000,
                    MaxConsumption = 1999.99m,
                    LevelColor = "#E5E4E2",
                    LevelIcon = "💎",
                    Privileges = new List<MemberPrivilege>
                    {
                        new MemberPrivilege
                        {
                            PrivilegeType = "discount",
                            PrivilegeName = "铂金折扣",
                            PrivilegeDesc = "享受8折优惠",
                            PrivilegeValue = "80%",
                            PrivilegeIcon = "💰"
                        },
                        new MemberPrivilege
                        {
                            PrivilegeType = "points",
                            PrivilegeName = "积分奖励",
                            PrivilegeDesc = "消费1元获得2积分",
                            PrivilegeValue = "1:2",
                            PrivilegeIcon = "⭐"
                        },
                        new MemberPrivilege
                        {
                            PrivilegeType = "birthday",
                            PrivilegeName = "生日特权",
                            PrivilegeDesc = "生日当月享受额外7折优惠",
                            PrivilegeValue = "70%",
                            PrivilegeIcon = "🎂"
                        },
                        new MemberPrivilege
                        {
                            PrivilegeType = "priority",
                            PrivilegeName = "优先服务",
                            PrivilegeDesc = "享受VIP优先制作服务",
                            PrivilegeValue = "VIP优先",
                            PrivilegeIcon = "⚡"
                        },
                        new MemberPrivilege
                        {
                            PrivilegeType = "free_delivery",
                            PrivilegeName = "免费配送",
                            PrivilegeDesc = "享受免费外送服务",
                            PrivilegeValue = "免费",
                            PrivilegeIcon = "🚗"
                        }
                    }
                },
                new MemberLevel
                {
                    LevelCode = "diamond",
                    LevelName = "钻石会员",
                    MinConsumption = 2000,
                    MaxConsumption = null,
                    LevelColor = "#B9F2FF",
                    LevelIcon = "💎",
                    Privileges = new List<MemberPrivilege>
                    {
                        new MemberPrivilege
                        {
                            PrivilegeType = "discount",
                            PrivilegeName = "钻石折扣",
                            PrivilegeDesc = "享受7.5折优惠",
                            PrivilegeValue = "75%",
                            PrivilegeIcon = "💰"
                        },
                        new MemberPrivilege
                        {
                            PrivilegeType = "points",
                            PrivilegeName = "积分奖励",
                            PrivilegeDesc = "消费1元获得2.5积分",
                            PrivilegeValue = "1:2.5",
                            PrivilegeIcon = "⭐"
                        },
                        new MemberPrivilege
                        {
                            PrivilegeType = "birthday",
                            PrivilegeName = "生日特权",
                            PrivilegeDesc = "生日当月享受额外6.5折优惠",
                            PrivilegeValue = "65%",
                            PrivilegeIcon = "🎂"
                        },
                        new MemberPrivilege
                        {
                            PrivilegeType = "priority",
                            PrivilegeName = "至尊服务",
                            PrivilegeDesc = "享受至尊VIP服务",
                            PrivilegeValue = "至尊VIP",
                            PrivilegeIcon = "⚡"
                        },
                        new MemberPrivilege
                        {
                            PrivilegeType = "free_delivery",
                            PrivilegeName = "免费配送",
                            PrivilegeDesc = "享受免费外送服务",
                            PrivilegeValue = "免费",
                            PrivilegeIcon = "🚗"
                        },
                        new MemberPrivilege
                        {
                            PrivilegeType = "exclusive",
                            PrivilegeName = "专属服务",
                            PrivilegeDesc = "享受专属客服和定制服务",
                            PrivilegeValue = "专属",
                            PrivilegeIcon = "👑"
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 手动设置客户累计消费金额
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <param name="amount">累计消费金额</param>
        /// <returns>是否设置成功</returns>
        public async Task<bool> SetCustomerTotalConsumptionAsync(int customerId, decimal amount)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();
                
                // 先检查客户是否存在
                var checkSql = "SELECT COUNT(*) FROM PUB.Customer WHERE CustomerID = :CustomerID";
                using var checkCommand = new OracleCommand(checkSql, connection);
                checkCommand.Parameters.Add(":CustomerID", OracleDbType.Int32).Value = customerId;
                var customerExists = Convert.ToInt32(await checkCommand.ExecuteScalarAsync()) > 0;
                
                if (!customerExists)
                {
                    _logger.LogWarning($"客户 {customerId} 不存在，无法设置累计消费金额");
                    return false;
                }

                // 手动设置累计消费金额
                var updateSql = "UPDATE PUB.Customer SET TotalConsumption = :TotalConsumption WHERE CustomerID = :CustomerID";
                
                using var updateCommand = new OracleCommand(updateSql, connection);
                updateCommand.Parameters.Add(":CustomerID", OracleDbType.Int32).Value = customerId;
                updateCommand.Parameters.Add(":TotalConsumption", OracleDbType.Decimal).Value = amount;

                var rowsAffected = await updateCommand.ExecuteNonQueryAsync();
                
                _logger.LogInformation($"客户 {customerId} 累计消费金额已手动设置为: {amount}，影响行数: {rowsAffected}");
                
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"手动设置客户 {customerId} 累计消费金额失败");
                return false;
            }
        }

        /// <summary>
        /// 计算并更新客户累计消费金额
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <returns>是否更新成功</returns>
        public async Task<bool> UpdateCustomerTotalConsumptionAsync(int customerId)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();
                
                // 开始事务
                using var transaction = connection.BeginTransaction();
                
                // 计算客户的累计消费金额 - 移除状态过滤，与消费统计保持一致
                var calculateSql = @"
                    SELECT NVL(SUM(o.TotalPrice), 0) as TotalConsumption
                    FROM PUB.Orders o
                    WHERE o.CustomerID = :CustomerID";
                
                decimal totalConsumption = 0;
                using (var calculateCommand = new OracleCommand(calculateSql, connection))
                {
                    calculateCommand.Transaction = transaction;
                    calculateCommand.Parameters.Add(":CustomerID", OracleDbType.Int32).Value = customerId;
                    var result = await calculateCommand.ExecuteScalarAsync();
                    totalConsumption = Convert.ToDecimal(result ?? 0);
                    _logger.LogInformation($"计算得到客户 {customerId} 的累计消费金额: {totalConsumption}");
                }

                // 检查客户当前的数据
                var checkSql = "SELECT CustomerID, CustomerName, TotalConsumption FROM PUB.Customer WHERE CustomerID = :CustomerID";
                using var checkCommand = new OracleCommand(checkSql, connection);
                checkCommand.Transaction = transaction;
                checkCommand.Parameters.Add(":CustomerID", OracleDbType.Int32).Value = customerId;
                using var checkReader = await checkCommand.ExecuteReaderAsync();
                
                if (!await checkReader.ReadAsync())
                {
                    _logger.LogWarning($"客户 {customerId} 不存在，无法更新累计消费金额");
                    return false;
                }
                
                var currentConsumption = checkReader.GetDecimal(checkReader.GetOrdinal("TotalConsumption"));
                var customerName = checkReader.GetString(checkReader.GetOrdinal("CustomerName"));
                _logger.LogInformation($"客户 {customerId}({customerName}) 当前累计消费: {currentConsumption}，准备更新为: {totalConsumption}");
                checkReader.Close();

                // 更新客户的累计消费金额
                var updateSql = "UPDATE PUB.Customer SET TotalConsumption = :TotalConsumption WHERE CustomerID = :CustomerID";
                
                using var updateCommand = new OracleCommand(updateSql, connection);
                updateCommand.Transaction = transaction;
                updateCommand.Parameters.Add(":TotalConsumption", OracleDbType.Decimal).Value = totalConsumption;
                updateCommand.Parameters.Add(":CustomerID", OracleDbType.Int32).Value = customerId;

                _logger.LogInformation($"执行UPDATE语句: {updateSql}");
                _logger.LogInformation($"参数: CustomerID={customerId}, TotalConsumption={totalConsumption}");
                
                var rowsAffected = await updateCommand.ExecuteNonQueryAsync();
                
                _logger.LogInformation($"UPDATE语句执行完成，影响行数: {rowsAffected}");
                
                // 再次查询确认更新是否成功
                var verifySql = "SELECT TotalConsumption FROM PUB.Customer WHERE CustomerID = :CustomerID";
                using var verifyCommand = new OracleCommand(verifySql, connection);
                verifyCommand.Transaction = transaction;
                verifyCommand.Parameters.Add(":CustomerID", OracleDbType.Int32).Value = customerId;
                var updatedConsumption = Convert.ToDecimal(await verifyCommand.ExecuteScalarAsync());
                
                _logger.LogInformation($"更新后客户 {customerId} 的累计消费金额: {updatedConsumption}");
                
                // 提交事务
                transaction.Commit();
                _logger.LogInformation($"事务已提交");
                
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新客户 {customerId} 累计消费金额失败");
                return false;
            }
        }

        /// <summary>
        /// 更新客户会员等级
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <returns>是否更新成功</returns>
        public async Task<bool> UpdateCustomerMemberLevelAsync(int customerId)
        {
            try
            {
                var customer = await GetCustomerAsync(customerId);
                if (customer == null)
                {
                    return false;
                }

                var levels = await GetMemberLevelsAsync();
                var currentLevel = CalculateCurrentLevel(customer.TotalConsumption, levels);

                // 更新数据库中的会员等级
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();
                
                var updateSql = "UPDATE PUB.Customer SET VIPLevel = :VIPLevel WHERE CustomerID = :CustomerID";
                
                using var command = new OracleCommand(updateSql, connection);
                command.Parameters.Add(":CustomerID", OracleDbType.Int32).Value = customerId;
                command.Parameters.Add(":VIPLevel", OracleDbType.Int32).Value = GetLevelCode(currentLevel.LevelCode);

                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新客户 {customerId} 会员等级失败");
                return false;
            }
        }

        /// <summary>
        /// 获取客户的订单数据用于调试
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <returns>订单列表</returns>
        public async Task<List<object>> GetDebugCustomerOrdersAsync(int customerId)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();
                
                var sql = @"
                    SELECT OrderID, CustomerID, TotalPrice, OrderStatus, OrderTime
                    FROM PUB.Orders 
                    WHERE CustomerID = :CustomerID 
                    ORDER BY OrderTime DESC";
                
                using var command = new OracleCommand(sql, connection);
                command.Parameters.Add(":CustomerID", OracleDbType.Int32).Value = customerId;
                using var reader = await command.ExecuteReaderAsync();
                
                var orders = new List<object>();
                while (await reader.ReadAsync())
                {
                    orders.Add(new {
                        OrderID = reader.GetInt32(reader.GetOrdinal("OrderID")),
                        CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                        TotalPrice = reader.GetDecimal(reader.GetOrdinal("TotalPrice")),
                        OrderStatus = reader.GetString(reader.GetOrdinal("OrderStatus")),
                        OrderTime = reader.GetDateTime(reader.GetOrdinal("OrderTime")).ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }
                
                _logger.LogInformation($"客户 {customerId} 有 {orders.Count} 个订单记录");
                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取客户 {customerId} 订单数据失败");
                return new List<object>();
            }
        }

        /// <summary>
        /// 获取客户列表用于调试
        /// </summary>
        /// <returns>客户列表</returns>
        public async Task<List<object>> GetDebugCustomerListAsync()
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();
                
                var sql = "SELECT CustomerID, CustomerName, TotalConsumption, VIPPoints FROM PUB.Customer ORDER BY CustomerID";
                
                using var command = new OracleCommand(sql, connection);
                using var reader = await command.ExecuteReaderAsync();
                
                var customers = new List<object>();
                while (await reader.ReadAsync())
                {
                    customers.Add(new {
                        CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                        CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                        TotalConsumption = reader.GetDecimal(reader.GetOrdinal("TotalConsumption")),
                        VIPPoints = reader.GetInt32(reader.GetOrdinal("VIPPoints"))
                    });
                }
                
                _logger.LogInformation($"找到 {customers.Count} 个客户记录");
                return customers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取客户列表失败");
                return new List<object>();
            }
        }

        /// <summary>
        /// 获取客户消费统计
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <returns>消费统计</returns>
        public async Task<ConsumptionStats> GetCustomerConsumptionStatsAsync(int customerId)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();
                
                var statsSql = @"
                    SELECT 
                        c.CustomerID,
                        c.TotalConsumption,
                        NVL(COUNT(o.OrderID), 0) as TotalOrders,
                        NVL(SUM(CASE WHEN o.OrderTime >= SYSDATE - 30 THEN o.TotalPrice ELSE 0 END), 0) as MonthlyConsumption,
                        NVL(COUNT(CASE WHEN o.OrderTime >= SYSDATE - 30 THEN o.OrderID ELSE NULL END), 0) as MonthlyOrders,
                        NVL(AVG(o.TotalPrice), 0) as AverageOrderAmount,
                        NVL(MAX(o.OrderTime), c.RegisterTime) as LastOrderTime,
                        NVL(s.StoreName, '未知门店') as FavoriteStore
                    FROM PUB.Customer c
                    LEFT JOIN PUB.Orders o ON c.CustomerID = o.CustomerID
                    LEFT JOIN PUB.Store s ON o.StoreID = s.StoreID
                    WHERE c.CustomerID = :CustomerID
                    GROUP BY c.CustomerID, c.TotalConsumption, c.RegisterTime, s.StoreName
                    ORDER BY COUNT(o.OrderID) DESC";

                using var command = new OracleCommand(statsSql, connection);
                command.Parameters.Add(":CustomerID", OracleDbType.Int32).Value = customerId;
                
                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    return new ConsumptionStats
                    {
                        CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                        TotalConsumption = reader.GetDecimal(reader.GetOrdinal("TotalConsumption")),
                        TotalOrders = reader.GetInt32(reader.GetOrdinal("TotalOrders")),
                        MonthlyConsumption = reader.GetDecimal(reader.GetOrdinal("MonthlyConsumption")),
                        MonthlyOrders = reader.GetInt32(reader.GetOrdinal("MonthlyOrders")),
                        AverageOrderAmount = reader.GetDecimal(reader.GetOrdinal("AverageOrderAmount")),
                        LastOrderTime = reader.GetDateTime(reader.GetOrdinal("LastOrderTime")),
                        FavoriteStore = reader.GetString(reader.GetOrdinal("FavoriteStore"))
                    };
                }

                return new ConsumptionStats { CustomerId = customerId };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取客户 {customerId} 消费统计失败");
                return new ConsumptionStats { CustomerId = customerId };
            }
        }

        #region 私有方法

        /// <summary>
        /// 获取客户信息
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <returns>客户信息</returns>
        private async Task<Customer?> GetCustomerAsync(int customerId)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();
                
                var sql = "SELECT * FROM PUB.Customer WHERE CustomerID = :CustomerID";
                
                using var command = new OracleCommand(sql, connection);
                command.Parameters.Add(":CustomerID", OracleDbType.Int32).Value = customerId;
                
                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    return new Customer
                    {
                        CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                        CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                        Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                        Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                        Birthday = reader.IsDBNull(reader.GetOrdinal("Birthday")) ? null : reader.GetDateTime(reader.GetOrdinal("Birthday")),
                        Gender = reader.IsDBNull(reader.GetOrdinal("Gender")) ? null : reader.GetString(reader.GetOrdinal("Gender"))[0],
                        RegisterTime = reader.GetDateTime(reader.GetOrdinal("RegisterTime")),
                        LastVisitTime = null, // 字段不存在，设为null
                        TotalConsumption = reader.GetDecimal(reader.GetOrdinal("TotalConsumption")),
                        VIPLevel = reader.IsDBNull(reader.GetOrdinal("VIPLevel")) ? null : reader.GetInt32(reader.GetOrdinal("VIPLevel")),
                        VIPPoints = reader.GetInt32(reader.GetOrdinal("VIPPoints")),
                        Status = reader.GetString(reader.GetOrdinal("Status")),
                        PreferredStoreID = null // 暂时设为null
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取客户 {customerId} 信息失败");
                return null;
            }
        }

        /// <summary>
        /// 计算当前会员等级
        /// </summary>
        /// <param name="totalConsumption">累计消费金额</param>
        /// <param name="levels">会员等级列表</param>
        /// <returns>当前等级</returns>
        private MemberLevel CalculateCurrentLevel(decimal totalConsumption, List<MemberLevel> levels)
        {
            var sortedLevels = levels.OrderByDescending(l => l.MinConsumption).ToList();
            
            foreach (var level in sortedLevels)
            {
                if (totalConsumption >= level.MinConsumption)
                {
                    return level;
                }
            }

            return levels.OrderBy(l => l.MinConsumption).First();
        }

        /// <summary>
        /// 计算下一会员等级
        /// </summary>
        /// <param name="totalConsumption">累计消费金额</param>
        /// <param name="levels">会员等级列表</param>
        /// <returns>下一等级</returns>
        private MemberLevel? CalculateNextLevel(decimal totalConsumption, List<MemberLevel> levels)
        {
            var sortedLevels = levels.OrderBy(l => l.MinConsumption).ToList();
            
            foreach (var level in sortedLevels)
            {
                if (totalConsumption < level.MinConsumption)
                {
                    return level;
                }
            }

            return null; // 已达最高等级
        }

        /// <summary>
        /// 计算升级进度
        /// </summary>
        /// <param name="totalConsumption">累计消费金额</param>
        /// <param name="currentLevel">当前等级</param>
        /// <param name="nextLevel">下一等级</param>
        /// <returns>升级进度百分比</returns>
        private decimal CalculateProgressToNextLevel(decimal totalConsumption, MemberLevel currentLevel, MemberLevel? nextLevel)
        {
            if (nextLevel == null)
            {
                return 100; // 已达最高等级
            }

            var currentThreshold = currentLevel.MinConsumption;
            var nextThreshold = nextLevel.MinConsumption;
            var progressRange = nextThreshold - currentThreshold;
            var currentProgress = totalConsumption - currentThreshold;

            if (progressRange <= 0)
            {
                return 100;
            }

            var percentage = (currentProgress / progressRange) * 100;
            return Math.Max(0, Math.Min(100, percentage));
        }

        /// <summary>
        /// 将等级代码转换为数值
        /// </summary>
        /// <param name="levelCode">等级代码</param>
        /// <returns>等级数值</returns>
        private int GetLevelCode(string levelCode)
        {
            return levelCode switch
            {
                "bronze" => 1,
                "silver" => 2,
                "gold" => 3,
                "platinum" => 4,
                "diamond" => 5,
                _ => 1
            };
        }

        #endregion
    }
}