using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Services;
using Oracle.ManagedDataAccess.Client;

namespace RestaurantManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly DatabaseService _databaseService;

        public DatabaseController(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        /// <summary>
        /// 测试数据库连接
        /// </summary>
        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var isConnected = await _databaseService.TestConnectionAsync();
                
                return Ok(new
                {
                    success = isConnected,
                    message = isConnected ? "Oracle数据库连接成功" : "Oracle数据库连接失败",
                    timestamp = DateTime.Now,
                    database = "Oracle Database"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"数据库连接测试失败: {ex.Message}",
                    timestamp = DateTime.Now
                });
            }
        }

        /// <summary>
        /// 获取数据库表列表
        /// </summary>
        [HttpGet("tables")]
        public async Task<IActionResult> GetTables()
        {
            try
            {
                var tables = await _databaseService.GetTablesAsync();
                
                return Ok(new
                {
                    success = true,
                    message = "获取表列表成功",
                    count = tables.Count,
                    tables = tables,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"获取表列表失败: {ex.Message}",
                    timestamp = DateTime.Now
                });
            }
        }

        /// <summary>
        /// 获取数据库信息
        /// </summary>
        [HttpGet("info")]
        public async Task<IActionResult> GetDatabaseInfo()
        {
            try
            {
                using var connection = new OracleConnection(_databaseService.CreateConnection().ConnectionString);
                await connection.OpenAsync();

                // 获取Oracle版本信息
                using var versionCommand = new OracleCommand("SELECT * FROM v$version WHERE banner LIKE 'Oracle%'", connection);
                var versionResult = await versionCommand.ExecuteScalarAsync();
                var version = versionResult?.ToString() ?? "Unknown";

                // 获取当前用户
                using var userCommand = new OracleCommand("SELECT USER FROM DUAL", connection);
                var userResult = await userCommand.ExecuteScalarAsync();
                var currentUser = userResult?.ToString() ?? "Unknown";

                // 获取当前时间
                using var timeCommand = new OracleCommand("SELECT SYSDATE FROM DUAL", connection);
                var dbTime = await timeCommand.ExecuteScalarAsync();

                return Ok(new
                {
                    success = true,
                    message = "数据库信息获取成功",
                    info = new
                    {
                        version = version,
                        currentUser = currentUser,
                        serverTime = dbTime,
                        connectionString = "Data Source=47.110.57.201:1521/XEPDB1"
                    },
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"获取数据库信息失败: {ex.Message}",
                    timestamp = DateTime.Now
                });
            }
        }
    }
}
