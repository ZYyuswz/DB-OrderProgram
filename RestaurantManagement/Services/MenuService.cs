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
            var sql = @"
                SELECT CATEGORYID, CATEGORYNAME, DESCRIPTION, SORTORDER
                FROM PUB.CATEGORY 
                WHERE ISACTIVE = 'Y'
                ORDER BY SORTORDER";
            return await connection.QueryAsync<Category>(sql);
        }

        // 获取所有菜品
        public async Task<IEnumerable<Dish>> GetAllDishesAsync()
        {
            using var connection = _dbService.CreateConnection();
            
            Console.WriteLine("[MenuService] 开始执行GetAllDishesAsync方法...");
            
            var sql = @"
                SELECT DISHID, DISHNAME, CATEGORYID, PRICE, DESCRIPTION, 
                       IMAGEURL, 
                       CASE WHEN ISAVAILABLE = 'Y' THEN 1 ELSE 0 END as ISAVAILABLE,
                       CREATETIME
                FROM PUB.DISH 
                ORDER BY DISHNAME";
                
            Console.WriteLine($"[MenuService] 执行SQL查询: {sql}");
            
            try
            {
                var result = await connection.QueryAsync<Dish>(sql);
                Console.WriteLine($"[MenuService] SQL查询成功，返回 {result?.Count() ?? 0} 条记录");
                
                if (result != null && result.Any())
                {
                    var firstDish = result.First();
                    Console.WriteLine($"[MenuService] 第一条记录: DishID={firstDish.DishID}, DishName={firstDish.DishName}, IsAvailable={firstDish.IsAvailable}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MenuService] SQL查询失败: {ex.Message}");
                Console.WriteLine($"[MenuService] 完整异常信息: {ex}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[MenuService] 内部异常: {ex.InnerException.Message}");
                }
                
                // 特别检查Oracle相关错误
                if (ex.Message.Contains("ORA-") || (ex.InnerException?.Message.Contains("ORA-") == true))
                {
                    Console.WriteLine($"[MenuService] 检测到Oracle数据库错误");
                }
                
                throw;
            }
        }

        // 根据分类获取菜品
        public async Task<IEnumerable<Dish>> GetDishesByCategoryAsync(int categoryId)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                SELECT DISHID, DISHNAME, CATEGORYID, PRICE, DESCRIPTION, 
                       IMAGEURL, 
                       CASE WHEN ISAVAILABLE = 'Y' THEN 1 ELSE 0 END as ISAVAILABLE,
                       CREATEDTIME
                FROM PUB.DISH 
                WHERE CATEGORYID = :CategoryId AND ISAVAILABLE = 'Y'
                ORDER BY DISHNAME";
            return await connection.QueryAsync<Dish>(sql, new { CategoryId = categoryId });
        }

        // 根据ID获取菜品
        public async Task<Dish?> GetDishByIdAsync(int dishId)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                SELECT DISHID, DISHNAME, CATEGORYID, PRICE, DESCRIPTION, 
                       IMAGEURL, 
                       CASE WHEN ISAVAILABLE = 'Y' THEN 1 ELSE 0 END as ISAVAILABLE,
                       CREATEDTIME
                FROM PUB.DISH 
                WHERE DISHID = :DishId";
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
