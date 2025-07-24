using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Services;
using RestaurantManagement.Models;

namespace RestaurantManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TablesController : ControllerBase
    {
        private readonly TableService _tableService;
        private readonly OrderService _orderService;

        public TablesController(TableService tableService, OrderService orderService)
        {
            _tableService = tableService;
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTables()
        {
            var tables = await _tableService.GetAllTablesAsync();
            return Ok(tables);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTable(int id)
        {
            var table = await _tableService.GetTableByIdAsync(id);
            if (table == null)
                return NotFound();
            return Ok(table);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateTableStatus(int id, [FromBody] string status)
        {
            var result = await _tableService.UpdateTableStatusAsync(id, status);
            if (!result)
                return BadRequest();
            return Ok();
        }

        [HttpPost("{id}/open")]
        public async Task<IActionResult> OpenTable(int id)
        {
            var result = await _tableService.OpenTableAsync(id);
            if (!result)
                return BadRequest();
            return Ok();
        }

        [HttpPost("{id}/clean")]
        public async Task<IActionResult> CleanTable(int id)
        {
            var result = await _tableService.CleanTableAsync(id);
            if (!result)
                return BadRequest();
            return Ok();
        }

        [HttpPost("{id}/finish-cleaning")]
        public async Task<IActionResult> FinishCleaning(int id)
        {
            var result = await _tableService.FinishCleaningAsync(id);
            if (!result)
                return BadRequest();
            return Ok();
        }

        [HttpGet("{id}/current-order")]
        public async Task<IActionResult> GetCurrentOrder(int id)
        {
            var order = await _orderService.GetCurrentOrderByTableAsync(id);
            if (order == null)
                return NotFound();
            return Ok(order);
        }
    }
}
