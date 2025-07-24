using Dapper;
using RestaurantManagement.Models;

namespace RestaurantManagement.Services
{
    public class TableService
    {
        private readonly DatabaseService _dbService;

        public TableService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        // 获取所有桌台状态
        public async Task<IEnumerable<TableInfo>> GetAllTablesAsync()
        {
            using var connection = _dbService.CreateConnection();
            var sql = "SELECT * FROM TableInfo ORDER BY TableNumber";
            return await connection.QueryAsync<TableInfo>(sql);
        }

        // 根据ID获取桌台信息
        public async Task<TableInfo?> GetTableByIdAsync(int tableId)
        {
            using var connection = _dbService.CreateConnection();
            var sql = "SELECT * FROM TableInfo WHERE TableID = @TableId";
            return await connection.QueryFirstOrDefaultAsync<TableInfo>(sql, new { TableId = tableId });
        }

        // 更新桌台状态
        public async Task<bool> UpdateTableStatusAsync(int tableId, string status)
        {
            using var connection = _dbService.CreateConnection();
            var sql = "UPDATE TableInfo SET Status = @Status WHERE TableID = @TableId";
            var result = await connection.ExecuteAsync(sql, new { Status = status, TableId = tableId });
            return result > 0;
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
            var sql = "UPDATE TableInfo SET Status = @Status, LastCleanTime = @CleanTime WHERE TableID = @TableId";
            var result = await connection.ExecuteAsync(sql, new { 
                Status = "清洁中", 
                CleanTime = DateTime.Now, 
                TableId = tableId 
            });
            return result > 0;
        }

        // 完成清洁（设置桌台为空闲状态）
        public async Task<bool> FinishCleaningAsync(int tableId)
        {
            return await UpdateTableStatusAsync(tableId, "空闲");
        }
    }
}
