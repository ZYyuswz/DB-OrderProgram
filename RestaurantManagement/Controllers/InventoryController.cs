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

        [HttpGet("materials")]
        public async Task<IActionResult> GetAllMaterials()
        {
            var materials = await _inventoryService.GetAllMaterialsAsync();
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
            purchase.PurchaseDate = DateTime.Now;
            purchase.Status = "待入库";
            var purchaseId = await _inventoryService.CreatePurchaseRecordAsync(purchase);
            return Ok(new { PurchaseId = purchaseId });
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
