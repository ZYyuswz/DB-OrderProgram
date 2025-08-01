using Dapper;
using RestaurantManagement.Models;

namespace RestaurantManagement.Services
{
    public class MenuService
    {
        private readonly DatabaseService _dbService;

        public MenuService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        // 获取所有分�?
        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            using var connection = _dbService.CreateConnection();
            var sql = "SELECT * FROM Category ORDER BY SortOrder";
            return await connection.QueryAsync<Category>(sql);
        }

        // 获取所有菜�?
        public async Task<IEnumerable<dynamic>> GetAllDishesAsync()
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                SELECT d.*, c.CategoryName 
                FROM PUB.Dish d
                INNER JOIN PUB.Category c ON d.CategoryID = c.CategoryID
                ORDER BY c.SortOrder, d.DishName";
            return await connection.QueryAsync(sql);
        }

        // 根据分类获取菜品
        public async Task<IEnumerable<Dish>> GetDishesByCategoryAsync(int categoryId)
        {
            using var connection = _dbService.CreateConnection();
            var sql = "SELECT * FROM PUB.Dish WHERE CategoryID = :CategoryId AND IsAvailable = 1";
            return await connection.QueryAsync<Dish>(sql, new { CategoryId = categoryId });
        }

        // 根据ID获取菜品
        public async Task<Dish?> GetDishByIdAsync(int dishId)
        {
            using var connection = _dbService.CreateConnection();
            var sql = "SELECT * FROM PUB.Dish WHERE DishID = :DishId";
            return await connection.QueryFirstOrDefaultAsync<Dish>(sql, new { DishId = dishId });
        }

        // 添加分类
        public async Task<bool> AddCategoryAsync(Category category)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                INSERT INTO PUB.Category (CategoryName, Description, SortOrder)
                VALUES (:CategoryName, :Description, :SortOrder)";
            var result = await connection.ExecuteAsync(sql, category);
            return result > 0;
        }

        // 添加菜品
        public async Task<bool> AddDishAsync(Dish dish)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                INSERT INTO PUB.Dish (DishName, CategoryID, Price, Description, ImageURL, IsAvailable, CreatedTime)
                VALUES (:DishName, :CategoryID, :Price, :Description, :ImageURL, :IsAvailable, :CreatedTime)";
            var result = await connection.ExecuteAsync(sql, dish);
            return result > 0;
        }

        // 更新菜品
        public async Task<bool> UpdateDishAsync(Dish dish)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                UPDATE PUB.Dish 
                SET DishName = :DishName, CategoryID = :CategoryID, Price = :Price, 
                    Description = :Description, ImageURL = :ImageURL, IsAvailable = :IsAvailable
                WHERE DishID = :DishID";
            var result = await connection.ExecuteAsync(sql, dish);
            return result > 0;
        }

        // 更新菜品可用状�?
        public async Task<bool> UpdateDishAvailabilityAsync(int dishId, bool isAvailable)
        {
            using var connection = _dbService.CreateConnection();
            var sql = "UPDATE PUB.Dish SET IsAvailable = :IsAvailable WHERE DishID = :DishId";
            var result = await connection.ExecuteAsync(sql, new { IsAvailable = isAvailable, DishId = dishId });
            return result > 0;
        }

        // 删除菜品（软删除 - 设置为不可用�?
        public async Task<bool> DeleteDishAsync(int dishId)
        {
            return await UpdateDishAvailabilityAsync(dishId, false);
        }

        // 获取菜品配方
        public async Task<IEnumerable<dynamic>> GetDishRecipeAsync(int dishId)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                SELECT r.*, rm.MaterialName, rm.Unit
                FROM PUB.Recipe r
                INNER JOIN PUB.RawMaterial rm ON r.MaterialID = rm.MaterialID
                WHERE r.DishID = :DishId";
            return await connection.QueryAsync(sql, new { DishId = dishId });
        }
    }
}
