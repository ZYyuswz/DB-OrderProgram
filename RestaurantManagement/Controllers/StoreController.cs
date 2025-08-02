using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Services;

namespace RestaurantManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoreController : ControllerBase
    {
        private readonly StoreService _storeService;

        public StoreController(StoreService storeService)
        {
            _storeService = storeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _storeService.GetAllStoresAsync();
            return Ok(list);
        }
    }
}