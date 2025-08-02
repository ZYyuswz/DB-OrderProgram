using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Services;
using RestaurantManagement.Models;

namespace RestaurantManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly MenuService _menuService;

        public MenuController(MenuService menuService)
        {
            _menuService = menuService;
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _menuService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("dishes")]
        public async Task<IActionResult> GetAllDishes()
        {
            try
            {
                Console.WriteLine("[MenuController] 开始获取所有菜品数据...");
                var dishes = await _menuService.GetAllDishesAsync();
                Console.WriteLine($"[MenuController] 成功获取到 {dishes?.Count() ?? 0} 个菜品");
                
                if (dishes != null && dishes.Any())
                {
                    Console.WriteLine($"[MenuController] 第一个菜品示例: {System.Text.Json.JsonSerializer.Serialize(dishes.First())}");
                }
                
                return Ok(dishes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MenuController] 获取菜品失败: {ex.Message}");
                Console.WriteLine($"[MenuController] 完整错误信息: {ex}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[MenuController] 内部异常: {ex.InnerException.Message}");
                }
                
                return StatusCode(500, new { message = "获取菜品数据失败", error = ex.Message });
            }
        }

        [HttpGet("categories/{categoryId}/dishes")]
        public async Task<IActionResult> GetDishesByCategory(int categoryId)
        {
            var dishes = await _menuService.GetDishesByCategoryAsync(categoryId);
            return Ok(dishes);
        }

        [HttpGet("dishes/{id}")]
        public async Task<IActionResult> GetDish(int id)
        {
            var dish = await _menuService.GetDishByIdAsync(id);
            if (dish == null)
                return NotFound();
            return Ok(dish);
        }

        [HttpPost("categories")]
        public async Task<IActionResult> AddCategory([FromBody] Category category)
        {
            var result = await _menuService.AddCategoryAsync(category);
            if (!result)
                return BadRequest();
            return Ok();
        }

        [HttpPost("dishes")]
        public async Task<IActionResult> AddDish([FromBody] Dish dish)
        {
            dish.CreateTime = DateTime.Now;
            dish.IsAvailable = true;
            var result = await _menuService.AddDishAsync(dish);
            if (!result)
                return BadRequest();
            return Ok();
        }

        [HttpPut("dishes/{id}")]
        public async Task<IActionResult> UpdateDish(int id, [FromBody] Dish dish)
        {
            dish.DishID = id;
            var result = await _menuService.UpdateDishAsync(dish);
            if (!result)
                return BadRequest();
            return Ok();
        }

        [HttpPut("dishes/{id}/availability")]
        public async Task<IActionResult> UpdateDishAvailability(int id, [FromBody] bool isAvailable)
        {
            var result = await _menuService.UpdateDishAvailabilityAsync(id, isAvailable);
            if (!result)
                return BadRequest();
            return Ok();
        }

        [HttpDelete("dishes/{id}")]
        public async Task<IActionResult> DeleteDish(int id)
        {
            var result = await _menuService.DeleteDishAsync(id);
            if (!result)
                return BadRequest();
            return Ok();
        }

        [HttpGet("dishes/{id}/recipe")]
        public async Task<IActionResult> GetDishRecipe(int id)
        {
            var recipe = await _menuService.GetDishRecipeAsync(id);
            return Ok(recipe);
        }

        // 为其他系统提供的接口
        [HttpGet]
        public async Task<IActionResult> GetMenu()
        {
            var categories = await _menuService.GetAllCategoriesAsync();
            var dishes = await _menuService.GetAllDishesAsync();
            return Ok(new { Categories = categories, Dishes = dishes });
        }

        [HttpGet("promotions")]
        public async Task<IActionResult> GetPromotions()
        {
            // TODO: 实现促销活动接口
            return Ok(new List<object>());
        }
    }
}
