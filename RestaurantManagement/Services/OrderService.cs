using Dapper;
using RestaurantManagement.Models;

namespace RestaurantManagement.Services
{
    public class OrderService
    {
        private readonly DatabaseService _dbService;

        public OrderService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        // 获取所有订单
        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            using var connection = _dbService.CreateConnection();
            var sql = "SELECT * FROM PUB.Orders ORDER BY OrderTime DESC";
            return await connection.QueryAsync<Order>(sql);
        }

        // 根据ID获取订单详情
        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            using var connection = _dbService.CreateConnection();
            var sql = "SELECT * FROM PUB.Orders WHERE OrderID = @OrderId";
            return await connection.QueryFirstOrDefaultAsync<Order>(sql, new { OrderId = orderId });
        }

        // 获取订单的菜品详情
        public async Task<IEnumerable<dynamic>> GetOrderDetailsAsync(int orderId)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                SELECT od.*, d.DishName 
                FROM PUB.OrderDetail od
                INNER JOIN PUB.Dish d ON od.DishID = d.DishID
                WHERE od.OrderID = @OrderId";
            return await connection.QueryAsync(sql, new { OrderId = orderId });
        }

        // 根据桌台ID获取当前订单
        public async Task<Order?> GetCurrentOrderByTableAsync(int tableId)
        {
            using var connection = _dbService.CreateConnection();
            var sql = "SELECT * FROM PUB.Orders WHERE TableID = @TableId AND OrderStatus = '进行中'";
            return await connection.QueryFirstOrDefaultAsync<Order>(sql, new { TableId = tableId });
        }

        // 创建新订单
        public async Task<int> CreateOrderAsync(Order order)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                INSERT INTO PUB.Orders (TableID, CustomerID, StaffID, OrderTime, TotalAmount, OrderStatus, Notes)
                OUTPUT INSERTED.OrderID
                VALUES (@TableID, @CustomerID, @StaffID, @OrderTime, @TotalAmount, @OrderStatus, @Notes)";
            return await connection.QuerySingleAsync<int>(sql, order);
        }

        // 添加订单详情
        public async Task<bool> AddOrderDetailAsync(OrderDetail detail)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                INSERT INTO PUB.OrderDetail (OrderID, DishID, Quantity, UnitPrice, Subtotal, SpecialRequests)
                VALUES (@OrderID, @DishID, @Quantity, @UnitPrice, @Subtotal, @SpecialRequests)";
            var result = await connection.ExecuteAsync(sql, detail);
            return result > 0;
        }

        // 更新订单状态
        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            using var connection = _dbService.CreateConnection();
            var sql = "UPDATE PUB.Orders SET OrderStatus = @Status WHERE OrderID = @OrderId";
            var result = await connection.ExecuteAsync(sql, new { Status = status, OrderId = orderId });
            return result > 0;
        }

        // 结账
        public async Task<bool> CheckoutOrderAsync(int orderId)
        {
            return await UpdateOrderStatusAsync(orderId, "已结账");
        }

        // 取消订单
        public async Task<bool> CancelOrderAsync(int orderId)
        {
            return await UpdateOrderStatusAsync(orderId, "已取消");
        }

        // 更新订单总金额
        public async Task<bool> UpdateOrderTotalAsync(int orderId)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                UPDATE PUB.Orders 
                SET TotalAmount = (
                    SELECT ISNULL(SUM(Subtotal), 0) 
                    FROM OrderDetail 
                    WHERE OrderID = @OrderId
                )
                WHERE OrderID = @OrderId";
            var result = await connection.ExecuteAsync(sql, new { OrderId = orderId });
            return result > 0;
        }
    }
}
