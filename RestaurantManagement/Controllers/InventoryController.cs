using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Services;
using RestaurantManagement.Models;

namespace RestaurantManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryService _inventoryService;

        public InventoryController(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet("test")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var count = await _inventoryService.GetMaterialCountAsync();
                return Ok(new { message = "数据库连接成功", materialCount = count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("materials")]
        public async Task<IActionResult> GetAllMaterials()
        {
            var materials = await _inventoryService.GetAllMaterialsAsync();
            Console.WriteLine($"[InventoryController] 第一个原材料示例: {System.Text.Json.JsonSerializer.Serialize(materials.First())}");
            
            return Ok(materials);
        }

        [HttpGet("materials/low-stock")]
        public async Task<IActionResult> GetLowStockMaterials()
        {
            var materials = await _inventoryService.GetLowStockMaterialsAsync();
            return Ok(materials);
        }

        [HttpPost("materials")]
        public async Task<IActionResult> AddMaterial([FromBody] RawMaterial material)
        {
            var result = await _inventoryService.AddMaterialAsync(material);
            if (!result)
                return BadRequest();
            return Ok();
        }

        [HttpPut("materials/{id}")]
        public async Task<IActionResult> UpdateMaterial(int id, [FromBody] RawMaterial material)
        {
            material.MaterialID = id;
            var result = await _inventoryService.UpdateMaterialAsync(material);
            if (!result)
                return BadRequest();
            return Ok();
        }

        [HttpPost("materials/{id}/add-stock")]
        public async Task<IActionResult> AddStock(int id, [FromBody] decimal quantity)
        {
            var result = await _inventoryService.AddStockAsync(id, quantity);
            if (!result)
                return BadRequest();
            return Ok();
        }

        [HttpPost("materials/{id}/reduce-stock")]
        public async Task<IActionResult> ReduceStock(int id, [FromBody] decimal quantity)
        {
            var result = await _inventoryService.ReduceStockAsync(id, quantity);
            if (!result)
                return BadRequest();
            return Ok();
        }

        [HttpGet("suppliers")]
        public async Task<IActionResult> GetAllSuppliers()
        {
            var suppliers = await _inventoryService.GetAllSuppliersAsync();
            Console.WriteLine($"[InventoryController] 第一个供应商示例: {System.Text.Json.JsonSerializer.Serialize(suppliers.First())}");
            Console.WriteLine("Raw suppliers data:", suppliers); // 调试日志
            return Ok(suppliers);
        }

        [HttpPost("suppliers")]
        public async Task<IActionResult> AddSupplier([FromBody] Supplier supplier)
        {
            var result = await _inventoryService.AddSupplierAsync(supplier);
            if (!result)
                return BadRequest();
            return Ok();
        }

        [HttpPut("suppliers/{id}")]
        public async Task<IActionResult> UpdateSupplier(int id, [FromBody] Supplier supplier)
        {
            supplier.SupplierID = id;
            var result = await _inventoryService.UpdateSupplierAsync(supplier);
            if (!result)
                return BadRequest();
            return Ok();
        }

        [HttpGet("purchases")]
        public async Task<IActionResult> GetPurchaseRecords()
        {
            var records = await _inventoryService.GetPurchaseRecordsAsync();
            return Ok(records);
        }

        [HttpPost("purchases")]
        public async Task<IActionResult> CreatePurchaseRecord([FromBody] PurchaseRecord purchase)
        {
            try
            {
                if (purchase == null)
                {
                    return BadRequest(new { error = "请求体为空" });
                }
                if (purchase.SupplierID <= 0)
                {
                    return BadRequest(new { error = "供应商ID无效" });
                }

                Console.WriteLine($"[CreatePurchaseRecord] 收到请求: {System.Text.Json.JsonSerializer.Serialize(purchase)}");

                purchase.PurchaseDate = DateTime.Now;
                purchase.Status = string.IsNullOrWhiteSpace(purchase.Status) ? "待收货" : purchase.Status;
                var purchaseId = await _inventoryService.CreatePurchaseRecordAsync(purchase);
                Console.WriteLine($"[CreatePurchaseRecord] 创建成功, PurchaseID={purchaseId}");
                return Ok(new { PurchaseId = purchaseId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CreatePurchaseRecord] 发生异常: {ex.Message}\n{ex.StackTrace}");
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("purchases/{id}/confirm")]
        public async Task<IActionResult> ConfirmPurchase(int id)
        {
            var result = await _inventoryService.ConfirmPurchaseAsync(id);
            if (!result)
                return BadRequest();
            return Ok();
        }
    }
}
