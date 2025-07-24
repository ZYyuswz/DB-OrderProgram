using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace RestaurantManagement.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string not found.");
        }

        public IDbConnection CreateConnection()
        {
            return new OracleConnection(_connectionString);
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();
                
                // 测试查询以确保连接正常工作
                using var command = new OracleCommand("SELECT 'Connection Test' FROM DUAL", connection);
                var result = await command.ExecuteScalarAsync();
                return result != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"数据库连接测试失败: {ex.Message}");
                return false;
            }
        }

        // 添加一个方法来获取数据库表信息
        public async Task<List<string>> GetTablesAsync(string owner = "XCY")
        {
            var tables = new List<string>();
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();
                
                var sql = "SELECT table_name FROM all_tables WHERE owner = :owner ORDER BY table_name";
                using var command = new OracleCommand(sql, connection);
                command.Parameters.Add(new OracleParameter("owner", owner.ToUpper()));
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"获取表信息失败: {ex.Message}");
            }
            return tables;
        }
    }
}
