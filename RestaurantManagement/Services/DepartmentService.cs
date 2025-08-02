using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantManagement.Services
{
    public class DepartmentService
    {
        private readonly DatabaseService _dbService;

        public DepartmentService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task<IEnumerable<dynamic>> GetAllDepartmentsAsync()
        {
            using var connection = _dbService.CreateConnection();
            var sql = "SELECT * FROM PUB.Department ORDER BY DepartmentID";
            return await connection.QueryAsync(sql);
        }
    }
}