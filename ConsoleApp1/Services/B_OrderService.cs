// OrderService 是一个服务层类，主要负责处理与 “Order” 相关的业务逻辑和数据处理
// 通俗讲就是OrderController需要的方法
using DBManagement.Models;
using DBManagement.Utils;
using Microsoft.EntityFrameworkCore;
namespace DBManagement.Service
{
    public class OrderService
    {
        private readonly RestaurantDbContext _db;

        // 通过构造函数注入数据库上下文
        public OrderService(RestaurantDbContext db)
        {
            _db = db;
        }

        // 获取所有可用订单并转换为Order列表
        public List<Order> GetAllOrders()
        {
            // 从数据库查询订单实体
            var orders = _db.Orders
                .Include(o => o.OrderDetails) // 加载关联的订单详情
                .Select(d => d) // 直接返回实体，无需显式映射
                .ToList(); // 执行查询并转换为列表

            return orders;
        }

        // 创建订单及订单详情
        public (bool success, string message, int order_id) CreateOrder(Order order, List<OrderDetail> orderDetails)
        {
            try
            {
                // 生成订单ID
                order.OrderId = GenerateOrderId();

                // 设置订单基本信息
                order.CreateTime = DateTime.Now;
                order.UpdateTime = DateTime.Now;
                order.OrderStatus = "待处理"; // 默认状态
                
                int min_orderDetail_id = GenerateDetailId();
                foreach (var detail in orderDetails)
                {
                    // 生成订单明细ID
                    detail.OrderDetailId = min_orderDetail_id++;
                    // 设置外键关联
                    detail.OrderId = order.OrderId;
                    // 计算明细小计（单价 * 数量）
                    detail.Subtotal = detail.UnitPrice * detail.Quantity;
                    // 添加到订单的详情集合
                    order.OrderDetails.Add(detail);
                }

                //// 设置订单的详情集合
                //order.OrderDetails = orderDetails;

                // 计算订单总价（所有明细小计之和）
                order.TotalPrice = order.OrderDetails.Sum(d => d.Subtotal);

                // 先设置finalPrice为0，后续可根据实际情况更新
                order.FinalPrice = 0;

                // 只需要添加主订单，关联的详情会自动处理
                _db.Orders.Add(order);
                _db.SaveChanges();

                return (true, "订单创建成功", order.OrderId);
            }
            catch (Exception ex)
            {
                return (false, $"订单创建失败: {ex.Message}", -1); // -1表示创建错误
            }
        }

        public (bool success, string message) ContinueOrder(int customer_id, List<OrderDetail> orderDetails)
        {
            try
            {
                // 根据 customerId 查找未结账的订单
                var order = _db.Orders
                   .Include(o => o.OrderDetails)
                   .FirstOrDefault(o => o.CustomerId == customer_id && o.OrderStatus != "已结账");

                if (order == null)
                {
                    return (false, "未找到符合条件的订单");
                }

                foreach (var detail in orderDetails)
                { 
                    // 生成订单明细ID
                    detail.OrderDetailId = GenerateDetailId();
                    Console.WriteLine($"新增的OrderDetailId：{detail.OrderDetailId} ");
                    // 设置外键关联
                    detail.OrderId = order.OrderId;
                    // 计算明细小计（单价 * 数量）
                    detail.Subtotal = detail.UnitPrice * detail.Quantity;
                    // 添加到订单的详情集合
                    order.OrderDetails.Add(detail);
                    DbUtils.AddEntity(_db, detail);
                }

                // 计算订单总价（所有明细小计之和）
                order.TotalPrice = order.OrderDetails.Sum(d => d.Subtotal);

                // 保存更改
                _db.SaveChanges();

                return (true, "继续下单成功");
            }
            catch (Exception ex)
            {
                return (false, $"继续下单失败: {ex.Message}");
            }
        }

        public (bool success, string message) DeleteOrder(int orderId)
        {
            try
            {
                // 1. 查找订单及关联的订单详情
                var order = _db.Orders
                    .Include(o => o.OrderDetails) // 加载关联的 OrderDetail
                    .FirstOrDefault(o => o.OrderId == orderId);

                if (order == null)
                {
                    return (false, $"未找到 ID 为 {orderId} 的订单");
                }

                // 2. 先删除关联的所有订单详情
                _db.OrderDetails.RemoveRange(order.OrderDetails);

                // 3. 再删除订单本身
                _db.Orders.Remove(order);

                // 4. 保存更改（一次性提交删除操作）
                _db.SaveChanges();

                return (true, $"订单 {orderId} 及关联详情已成功删除");
            }
            catch (Exception ex)
            {
                return (false, $"删除订单失败: {ex.Message}");
            }
        }

        // 生成下一个订单ID
        private int GenerateOrderId()
        {
            // 从数据库获取当前最大的 OrderId
            int maxId = _db.Orders.Max(o => (int?)o.OrderId) ?? 0;
            return Interlocked.Increment(ref maxId);
        }

        // 生成下一个订单明细ID
        private int GenerateDetailId()
        {
            // 从数据库获取当前最大的 OrderDetailId
            int maxId = _db.OrderDetails.Max(od => (int?)od.OrderDetailId) ?? 0;
            return Interlocked.Increment(ref maxId);
        }
    }
}