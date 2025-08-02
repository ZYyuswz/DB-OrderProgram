using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantManagement.Services
{
    public class StoreService
    {
        private readonly DatabaseService _dbService;

        public StoreService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task<IEnumerable<dynamic>> GetAllStoresAsync()
        {
            using var connection = _dbService.CreateConnection();
            var sql = "SELECT * FROM PUB.Store ORDER BY StoreID";
            return await connection.QueryAsync(sql);
        }
    }
}