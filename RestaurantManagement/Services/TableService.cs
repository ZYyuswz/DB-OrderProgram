using Dapper;
using RestaurantManagement.Models;

namespace RestaurantManagement.Services
{
    public class TableService
    {
        private readonly DatabaseService _dbService;

        static TableService()
        {
            // 配置Dapper的Oracle字段映射
            DefaultTypeMap.MatchNamesWithUnderscores = false;
        }

        public TableService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        // 获取所有桌台状态
        public async Task<IEnumerable<TableInfo>> GetAllTablesAsync()
        {
            using var connection = _dbService.CreateConnection();
            var sql = "SELECT * FROM PUB.TableInfo ORDER BY TableNumber";
            return await connection.QueryAsync<TableInfo>(sql);
        }

        // 根据ID获取桌台信息
        public async Task<TableInfo?> GetTableByIdAsync(int tableId)
        {
            using var connection = _dbService.CreateConnection();
            var sql = $"SELECT * FROM PUB.TableInfo WHERE TableID = {tableId}";
            return await connection.QueryFirstOrDefaultAsync<TableInfo>(sql);
        }

        // 更新桌台状态
        public async Task<bool> UpdateTableStatusAsync(int tableId, string status)
        {
            try
            {
                Console.WriteLine($"[TableService] 更新桌台状态: TableID={tableId}, Status={status}");
                
                using var connection = _dbService.CreateConnection();
                
                // 先查询当前状态
                var currentSql = "SELECT STATUS FROM PUB.TABLEINFO WHERE TABLEID = :TableId";
                var currentStatus = await connection.QueryFirstOrDefaultAsync<string>(currentSql, new { TableId = tableId });
                Console.WriteLine($"[TableService] 当前状态: {currentStatus}");
                
                // 使用与OrderService相同的参数绑定方式
                var sql = "UPDATE PUB.TABLEINFO SET STATUS = :Status WHERE TABLEID = :TableId";
                
                Console.WriteLine($"[TableService] 执行SQL: {sql}");
                Console.WriteLine($"[TableService] 参数: Status={status}, TableId={tableId}");
                
                var result = await connection.ExecuteAsync(sql, new { Status = status, TableId = tableId });
                
                Console.WriteLine($"[TableService] 更新结果: 影响行数={result}");
                
                // 再次查询验证更新结果
                var newStatus = await connection.QueryFirstOrDefaultAsync<string>(currentSql, new { TableId = tableId });
                Console.WriteLine($"[TableService] 更新后状态: {newStatus}");
                
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TableService] 更新桌台状态失败: {ex.Message}");
                Console.WriteLine($"[TableService] 错误详情: {ex}");
                return false;
            }
        }

        // 开台（设置桌台为占用状态）
        public async Task<bool> OpenTableAsync(int tableId)
        {
            return await UpdateTableStatusAsync(tableId, "占用");
        }

        // 清台（设置桌台为清洁中状态）
        public async Task<bool> CleanTableAsync(int tableId)
        {
            using var connection = _dbService.CreateConnection();
            var cleanTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var sql = $"UPDATE PUB.TableInfo SET Status = '清洁中', LastCleanTime = TO_DATE('{cleanTime}', 'YYYY-MM-DD HH24:MI:SS') WHERE TableID = {tableId}";
            var result = await connection.ExecuteAsync(sql);
            return result > 0;
        }

        // 完成清洁（设置桌台为空闲状态）
        public async Task<bool> FinishCleaningAsync(int tableId)
        {
            return await UpdateTableStatusAsync(tableId, "空闲");
        }

        // 完成清洁（设置桌台为空闲状态）- 新方法名
        public async Task<bool> CompleteCleaningAsync(int tableId)
        {
            return await UpdateTableStatusAsync(tableId, "空闲");
        }
    }
}
