using Oracle.ManagedDataAccess.Client;
using ConsoleApp1.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConsoleApp1.Services
{
    /// <summary>
    /// 数据库服务类
    /// </summary>
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(IConfiguration configuration, ILogger<DatabaseService> logger)
{
    _connectionString = configuration.GetConnectionString("OracleConnection")
        ?? throw new ArgumentNullException("Oracle connection string not found");
    _logger = logger;
}

        public string GetConnectionString()
        {
            return _connectionString;
        }

        public string GetConnectionString(string connectionStringName)
        {
            return _connectionString; // 这里可以根据需要实现按名称获取连接字符串
        }

        /// <summary>
        /// 测试数据库连接
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();

                _logger.LogInformation("Oracle数据库连接成功！");

                // 测试简单查询
                using var command = new OracleCommand("SELECT SYSDATE FROM DUAL", connection);
                var result = await command.ExecuteScalarAsync();

                _logger.LogInformation($"数据库当前时间: {result}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据库连接失败");
                return false;
            }
        }

        /// <summary>
        /// 获取所有门店
        /// </summary>
        public async Task<List<Store>> GetAllStoresAsync()
        {
            var stores = new List<Store>();

            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"SELECT StoreID, StoreName, Address, Phone, ManagerID, 
                                  OpeningHours, Status, OpenDate, RegionID, 
                                  StoreSize, MonthlyRent, CreateTime, UpdateTime 
                           FROM PUB.Store 
                           ORDER BY StoreID";

                using var command = new OracleCommand(sql, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    stores.Add(new Store
                    {
                        StoreID = reader.GetInt32(reader.GetOrdinal("StoreID")),
                        StoreName = reader.GetString(reader.GetOrdinal("StoreName")),
                        Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                        Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                        ManagerID = reader.IsDBNull(reader.GetOrdinal("ManagerID")) ? null : reader.GetInt32(reader.GetOrdinal("ManagerID")),
                        OpeningHours = reader.IsDBNull(reader.GetOrdinal("OpeningHours")) ? null : reader.GetString(reader.GetOrdinal("OpeningHours")),
                        Status = reader.GetString(reader.GetOrdinal("Status")),
                        OpenDate = reader.IsDBNull(reader.GetOrdinal("OpenDate")) ? null : reader.GetDateTime(reader.GetOrdinal("OpenDate")),
                        RegionID = reader.IsDBNull(reader.GetOrdinal("RegionID")) ? null : reader.GetInt32(reader.GetOrdinal("RegionID")),
                        StoreSize = reader.IsDBNull(reader.GetOrdinal("StoreSize")) ? null : reader.GetDecimal(reader.GetOrdinal("StoreSize")),
                        MonthlyRent = reader.IsDBNull(reader.GetOrdinal("MonthlyRent")) ? null : reader.GetDecimal(reader.GetOrdinal("MonthlyRent")),
                        CreateTime = reader.GetDateTime(reader.GetOrdinal("CreateTime")),
                        UpdateTime = reader.GetDateTime(reader.GetOrdinal("UpdateTime"))
                    });
                }

                _logger.LogInformation($"成功查询到 {stores.Count} 个门店");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询门店信息失败");
            }

            return stores;
        }

        /// <summary>
        /// 添加测试门店
        /// </summary>
        public async Task<bool> AddTestStoreAsync()
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"INSERT INTO PUB.Store (StoreID, StoreName, Address, Phone, OpeningHours, Status, OpenDate, CreateTime, UpdateTime)
                           VALUES (PUB.seq_store_id.NEXTVAL, :StoreName, :Address, :Phone, :OpeningHours, :Status, :OpenDate, SYSDATE, SYSDATE)";

                using var command = new OracleCommand(sql, connection);
                // 使用随机数避免唯一约束冲突
                var random = new Random();
                var randomSuffix = random.Next(1000, 9999);

                command.Parameters.Add(":StoreName", OracleDbType.Varchar2).Value = $"测试门店{randomSuffix}";
                command.Parameters.Add(":Address", OracleDbType.Varchar2).Value = "上海市杨浦区四平路1239号同济大学";
                command.Parameters.Add(":Phone", OracleDbType.Varchar2).Value = "021-12345678";
                command.Parameters.Add(":OpeningHours", OracleDbType.Varchar2).Value = "09:00-21:00";
                command.Parameters.Add(":Status", OracleDbType.Varchar2).Value = "营业中";
                command.Parameters.Add(":OpenDate", OracleDbType.Date).Value = DateTime.Now;

                int rowsAffected = await command.ExecuteNonQueryAsync();
                _logger.LogInformation($"成功添加测试门店，影响行数: {rowsAffected}");

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加测试门店失败");
                return false;
            }
        }

        /// <summary>
        /// 获取所有客户
        /// </summary>
        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            var customers = new List<Customer>();

            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"SELECT CustomerID, CustomerName, Phone, Email, Birthday, 
                                  Gender, RegisterTime, TotalConsumption, 
                                  VIPLevel, VIPPoints, Status 
                           FROM PUB.Customer 
                           ORDER BY CustomerID";

                using var command = new OracleCommand(sql, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    customers.Add(new Customer
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
                        // PreferredStoreID = reader.IsDBNull(reader.GetOrdinal("PreferredStoreID")) ? null : reader.GetInt32(reader.GetOrdinal("PreferredStoreID"))
                        PreferredStoreID = null // 暂时跳过此字段，避免字段名错误
                    });
                }

                _logger.LogInformation($"成功查询到 {customers.Count} 个客户");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询客户信息失败");
            }

            return customers;
        }

        /// <summary>
        /// 添加测试客户
        /// </summary>
        public async Task<bool> AddTestCustomerAsync()
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"INSERT INTO PUB.Customer (CustomerID, CustomerName, Phone, Email, Gender, RegisterTime, TotalConsumption, VIPPoints, Status)
                           VALUES (PUB.seq_customer_id.NEXTVAL, :CustomerName, :Phone, :Email, :Gender, SYSDATE, :TotalConsumption, :VIPPoints, :Status)";

                using var command = new OracleCommand(sql, connection);
                // 使用随机数避免唯一约束冲突
                var random = new Random();
                var randomSuffix = random.Next(1000, 9999);

                command.Parameters.Add(":CustomerName", OracleDbType.Varchar2).Value = $"测试客户{randomSuffix}";
                command.Parameters.Add(":Phone", OracleDbType.Varchar2).Value = $"138{randomSuffix:D4}8000";
                command.Parameters.Add(":Email", OracleDbType.Varchar2).Value = $"test{randomSuffix}@tongji.edu.cn";
                command.Parameters.Add(":Gender", OracleDbType.Char).Value = "M";
                command.Parameters.Add(":TotalConsumption", OracleDbType.Decimal).Value = 0;
                command.Parameters.Add(":VIPPoints", OracleDbType.Int32).Value = 0;
                command.Parameters.Add(":Status", OracleDbType.Varchar2).Value = "正常";

                int rowsAffected = await command.ExecuteNonQueryAsync();
                _logger.LogInformation($"成功添加测试客户，影响行数: {rowsAffected}");

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加测试客户失败");
                return false;
            }
        }

        /// <summary>
        /// 检查Customer表的字段名
        /// </summary>
        public async Task CheckCustomerTableColumnsAsync()
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"SELECT column_name FROM all_tab_columns 
                           WHERE owner = 'PUB' AND table_name = 'CUSTOMER' 
                           ORDER BY column_id";

                using var command = new OracleCommand(sql, connection);
                using var reader = await command.ExecuteReaderAsync();

                _logger.LogInformation("Customer表的字段列表:");
                while (await reader.ReadAsync())
                {
                    var columnName = reader.GetString(reader.GetOrdinal("COLUMN_NAME"));
                    _logger.LogInformation($"  - {columnName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查Customer表字段失败");
            }
        }

        /// <summary>
        /// 检查Orders表的字段名
        /// </summary>
        public async Task CheckOrdersTableColumnsAsync()
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"SELECT column_name FROM all_tab_columns 
                           WHERE owner = 'PUB' AND table_name = 'ORDERS' 
                           ORDER BY column_id";

                using var command = new OracleCommand(sql, connection);
                using var reader = await command.ExecuteReaderAsync();

                _logger.LogInformation("Orders表的字段列表:");
                while (await reader.ReadAsync())
                {
                    var columnName = reader.GetString(reader.GetOrdinal("COLUMN_NAME"));
                    _logger.LogInformation($"  - {columnName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查Orders表字段失败");
            }
        }

        /// <summary>
        /// 检查OrderDetail表的字段名
        /// </summary>
        public async Task CheckOrderDetailTableColumnsAsync()
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"SELECT column_name FROM all_tab_columns 
                           WHERE owner = 'PUB' AND table_name = 'ORDERDETAIL' 
                           ORDER BY column_id";

                using var command = new OracleCommand(sql, connection);
                using var reader = await command.ExecuteReaderAsync();

                _logger.LogInformation("OrderDetail表的字段列表:");
                while (await reader.ReadAsync())
                {
                    var columnName = reader.GetString(reader.GetOrdinal("COLUMN_NAME"));
                    _logger.LogInformation($"  - {columnName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查OrderDetail表字段失败");
            }
        }

        /// <summary>
        /// 查询现有的菜品ID
        /// </summary>
        public async Task<List<int>> GetExistingDishIdsAsync()
        {
            var dishIds = new List<int>();

            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"SELECT DishID FROM PUB.Dish WHERE ROWNUM <= 5 ORDER BY DishID";

                using var command = new OracleCommand(sql, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    dishIds.Add(reader.GetInt32(reader.GetOrdinal("DishID")));
                }

                _logger.LogInformation($"找到现有菜品ID: {string.Join(", ", dishIds)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "查询现有菜品ID失败");
            }

            return dishIds;
        }

        /// <summary>
        /// 检查表是否存在
        /// </summary>
        public async Task<bool> CheckTablesExistAsync()
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"SELECT COUNT(*) FROM all_tables 
                           WHERE owner = 'PUB' AND table_name IN ('CUSTOMER', 'STORE', 'DISH', 'ORDERS', 'ORDERDETAIL')";

                using var command = new OracleCommand(sql, connection);
                var count = Convert.ToInt32(await command.ExecuteScalarAsync());

                _logger.LogInformation($"找到 {count} 个核心表");
                return count >= 5; // 至少有5个核心表
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查表存在性失败");
                return false;
            }
        }
    }
}