// OrderService 是一个服务层类，主要负责处理与 “Order” 相关的业务逻辑和数据处理
// 通俗讲就是OrderController需要的方法
using ConsoleApp1.Models;
using DBManagement.Models;
using DBManagement.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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

        // VIP折扣数组
        private static readonly decimal[] VIPDiscounts = new decimal[]
        {
            1.00m,   // 0不使用
            0.99m,   // 等级1
            0.95m,   // 等级2
            0.90m,   // 等级3
            0.85m,   // 等级4
            0.80m    // 等级5
        };


        public decimal CalculatePrice(long customerId, decimal totalPrice)
        {
            try
            {
                // 1. 查询客户积分
                var customer = _db.Customers
                    .FirstOrDefault(c => c.CustomerId == customerId);
                if (customer == null)
                { 
                    return totalPrice;
                }
                Console.WriteLine($"客户找到: VIPLevel={customer.VIPLevel}, VIPPoints={customer.VIPPoints}");

                int vipPoints = customer.VIPPoints;
                int declineMoney = vipPoints / 100; // 每100积分抵1元
                if (declineMoney > totalPrice)
                {
                    declineMoney = (int)totalPrice; // 抵扣金额不能超过总价
                }

                // 2. 根据积分计算折扣
                int viplevel = customer.VIPLevel;
                decimal discount = VIPDiscounts[viplevel];

                // 3. 计算最终价格
                decimal finalPrice = (totalPrice - declineMoney)* discount;
                Console.WriteLine($"计算最终价: {finalPrice} (抵扣={declineMoney}, 折扣={discount})");
                // 4. 更新会员积分（减少使用的积分）
                if (declineMoney > 0)
                {
                    customer.VIPPoints -= declineMoney*100;
                    // 确保积分不为负数
                    if (customer.VIPPoints < 0)
                    {
                        customer.VIPPoints = 0;
                    }
                    Console.WriteLine($"更新积分: 原={customer.VIPPoints + declineMoney * 100}, 新={customer.VIPPoints}");
                    // 更新数据库
                    _db.Customers.Update(customer);
                    _db.SaveChanges();
                }

                return finalPrice;
            }
            catch (Exception ex)
            {
                // 记录日志或处理异常
                Console.WriteLine($"计算价格时出错: {ex.Message}");
                return totalPrice;
            }
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

                // 计算订单总价（所有明细小计之和）
                order.TotalPrice = order.OrderDetails.Sum(d => d.Subtotal);

                // 计算订单最终价格，调用函数
                order.FinalPrice = CalculatePrice(order.CustomerId,order.TotalPrice);


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

        public (bool success, string message) ContinueOrder(int tableId, List<OrderDetail> orderDetails)
        {
            try
            {
                // 根据 tableId 查找未结账的订单
                var order = _db.Orders
                   .Include(o => o.OrderDetails)
                   .FirstOrDefault(o => o.TableId == tableId && o.OrderStatus != "已结账");

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

        // 根据orderId获取订单总价、最终价和菜品明细
        public (bool success, string message, object data) GetOrderDetailWithDishName(int orderId)
        {
            // 查询订单及其详情和菜品信息
            var order = _db.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
                return (false, "未找到该订单", null);

            // 查询所有订单详情及对应菜品名
            var details = _db.OrderDetails
                .Where(od => od.OrderId == orderId)
                .Join(_db.Dishes,
                      od => od.DishId,
                      d => d.DishId,
                      (od, d) => new
                      {
                          DishName = d.DishName,
                          Quantity = od.Quantity
                      })
                .ToList();

            var result = new
            {
                OrderId = order.OrderId,
                TotalPrice = order.TotalPrice,
                FinalPrice = order.FinalPrice,
                Dishes = details
            };

            return (true, "查询成功", result);
        }
    }
}