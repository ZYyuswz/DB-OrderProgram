using Dapper;
using RestaurantManagement.Models;

namespace RestaurantManagement.Services
{
    public class InventoryService
    {
        private readonly DatabaseService _dbService;

        public InventoryService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        // 测试方法：获取原材料数量
        public async Task<int> GetMaterialCountAsync()
        {
            using var connection = _dbService.CreateConnection();
            var sql = "SELECT COUNT(*) FROM PUB.RawMaterial";
            return await connection.QuerySingleAsync<int>(sql);
        }

        // 获取所有原材料
        public async Task<IEnumerable<dynamic>> GetAllMaterialsAsync()
        {
            using var connection = _dbService.CreateConnection();
            // var sql = @"
            //     SELECT rm.MaterialID,
            //            rm.MaterialName,
            //            rm.Unit,
            //            rm.CurrentStock,
            //            rm.MinStock,
            //            rm.MaxStock,
            //            rm.UnitPrice,
            //            rm.SupplierID,
            //            rm.StaffID,
            //            rm.StoreID,
            //            rm.Status,
            //            rm.StorageLocation,
            //            rm.LastInTime,
            //            rm.LastInQuantity,
            //            rm.ExpiryDate,
            //            s.SupplierName,
            //            CASE WHEN rm.CurrentStock <= rm.MinStock THEN 1 ELSE 0 END as IsLowStock,
            //            '原材料' as category
            //     FROM PUB.RawMaterial rm
            //     LEFT JOIN PUB.Supplier s ON rm.SupplierID = s.SupplierID
            //     ORDER BY rm.MaterialName";
            var sql = "SELECT * FROM PUB.RawMaterial LEFT JOIN PUB.Supplier USING(SupplierID)";
            return await connection.QueryAsync(sql);
        }

        // 获取库存预警材料
        public async Task<IEnumerable<RawMaterial>> GetLowStockMaterialsAsync()
        {
            using var connection = _dbService.CreateConnection();
            var sql = "SELECT * FROM PUB.RawMaterial WHERE CurrentStock <= MinStock;";
            return await connection.QueryAsync<RawMaterial>(sql);
        }

        // 添加原材料
        public async Task<bool> AddMaterialAsync(RawMaterial material)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                INSERT INTO PUB.RawMaterial (MaterialName, Unit, CurrentStock, MinStock, UnitPrice, SupplierID)
                VALUES (@MaterialName, @Unit, @CurrentStock, @MinStock, @UnitPrice, @SupplierID)";
            var result = await connection.ExecuteAsync(sql, material);
            return result > 0;
        }

        // 更新原材料信息
        public async Task<bool> UpdateMaterialAsync(RawMaterial material)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                UPDATE PUB.RawMaterial 
                SET MaterialName = @MaterialName, Unit = @Unit, CurrentStock = @CurrentStock, 
                    MinStock = @MinStock, UnitPrice = @UnitPrice, SupplierID = @SupplierID
                WHERE MaterialID = @MaterialID";
            var result = await connection.ExecuteAsync(sql, material);
            return result > 0;
        }

        // 入库操作
        public async Task<bool> AddStockAsync(int materialId, decimal quantity)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                UPDATE PUB.RawMaterial 
                SET CurrentStock = CurrentStock + @Quantity,
                    LastInTime = @InTime,
                    LastInQuantity = @Quantity
                WHERE MaterialID = @MaterialId";
            var result = await connection.ExecuteAsync(sql, new { 
                Quantity = quantity, 
                InTime = DateTime.Now, 
                MaterialId = materialId 
            });
            return result > 0;
        }

        // 出库操作
        public async Task<bool> ReduceStockAsync(int materialId, decimal quantity)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                UPDATE PUB.RawMaterial 
                SET CurrentStock = CurrentStock - @Quantity
                WHERE MaterialID = @MaterialId AND CurrentStock >= @Quantity";
            var result = await connection.ExecuteAsync(sql, new { 
                Quantity = quantity, 
                MaterialId = materialId 
            });
            return result > 0;
        }

        // 获取所有供应商
        public async Task<IEnumerable<Supplier>> GetAllSuppliersAsync()
        {
            using var connection = _dbService.CreateConnection();
            var sql = "SELECT * FROM PUB.Supplier ORDER BY SupplierName";
            return await connection.QueryAsync<Supplier>(sql);
        }

        // 添加供应商
        public async Task<bool> AddSupplierAsync(Supplier supplier)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                INSERT INTO PUB.Supplier (SupplierName, ContactPerson, Phone, Address, Email)
                VALUES (@SupplierName, @ContactPerson, @Phone, @Address, @Email)";
            var result = await connection.ExecuteAsync(sql, supplier);
            return result > 0;
        }

        // 更新供应商
        public async Task<bool> UpdateSupplierAsync(Supplier supplier)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                UPDATE PUB.Supplier 
                SET SupplierName = @SupplierName, ContactPerson = @ContactPerson, 
                    Phone = @Phone, Address = @Address, Email = @Email
                WHERE SupplierID = @SupplierID";
            var result = await connection.ExecuteAsync(sql, supplier);
            return result > 0;
        }

        // 获取采购记录
        public async Task<IEnumerable<dynamic>> GetPurchaseRecordsAsync()
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                SELECT pr.*, s.SupplierName
                FROM PUB.PurchaseRecord pr
                INNER JOIN PUB.Supplier s ON pr.SupplierID = s.SupplierID
                ORDER BY pr.PurchaseDate DESC";
            return await connection.QueryAsync(sql);
        }

        // 创建采购记录
        public async Task<int> CreatePurchaseRecordAsync(PurchaseRecord purchase)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                INSERT INTO PUB.PurchaseRecord (SupplierID, PurchaseDate, TotalAmount, Status, Notes)
                VALUES (@SupplierID, @PurchaseDate, @TotalAmount, @Status, @Notes)
                RETURNING PurchaseID INTO :PurchaseID";
            
            var parameters = new 
            {
                purchase.SupplierID,
                purchase.PurchaseDate,
                purchase.TotalAmount,
                purchase.Status,
                purchase.Notes
            };
            
            var result = await connection.QuerySingleAsync<int>(sql, parameters);
            return result;
        }

        // 确认入库
        public async Task<bool> ConfirmPurchaseAsync(int purchaseId)
        {
            using var connection = _dbService.CreateConnection();
            
            // 更新采购记录状态
            var updatePurchase = "UPDATE PUB.PurchaseRecord SET Status = '已入库' WHERE PurchaseID = @PurchaseId";
            await connection.ExecuteAsync(updatePurchase, new { PurchaseId = purchaseId });

            // TODO: 这里需要根据采购详情更新库存
            // 实际实现中需要从 PurchaseDetail 表获取采购的具体材料和数量

            return true;
        }
    }
}
