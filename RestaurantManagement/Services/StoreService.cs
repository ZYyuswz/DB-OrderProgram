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
            var sql = @"SELECT 
                STOREID as StoreID,
                STORENAME as StoreName,
                ADDRESS as Address,
                PHONE as Phone
                FROM PUB.STORE ORDER BY STOREID";
            return await connection.QueryAsync(sql);
        }
    }
}