// RestaurantDbContext 类是基于 Entity Framework Core（EF Core）实现的数据库上下文类
// 主要作用是映射数据库结构、管理实体与数据库表的关系、提供数据访问入口
using Microsoft.EntityFrameworkCore;

namespace B_DBManagement.Models
{
    // RestaurantDbContext 继承自 DbContext,封装了数据库连接、查询、保存等基础操作
    public class RestaurantDbContext : DbContext
    {
        public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options)
            : base(options)
        {
        }

        // 定义所有DbSet，实体与数据库表的映射入口（也就是对应数据库中的表）
        public DbSet<Category> Categories { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<DishPromotion> DishPromotions { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<TableInfo> TableInfos { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Customer> Customers { get; set; }

        // OnModelCreating方法配置实体关系与约束
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 订单（Order）与订单详情（OrderDetail）：一对多关系
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)       // 订单详情属于一个订单
                .WithMany(o => o.OrderDetails) // 一个订单包含多个订单详情
                .HasForeignKey(od => od.OrderId); // 外键为 OrderDetail 表的 OrderId 字段

            // 订单详情与菜品 多对一
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Dish)
                .WithMany(d => d.OrderDetails)
                .HasForeignKey(od => od.DishId);

            // 菜品促销关联 多对一
            modelBuilder.Entity<DishPromotion>()
                .HasOne(dp => dp.Dish)
                .WithMany(d => d.DishPromotions)
                .HasForeignKey(dp => dp.DishId);

            modelBuilder.Entity<DishPromotion>()
                .HasOne(dp => dp.Promotion)
                .WithMany(p => p.DishPromotions)
                .HasForeignKey(dp => dp.PromotionId);
        }
    }
}