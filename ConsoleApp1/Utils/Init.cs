using DBManagement.Models;


namespace DBManagement.Utils
{
    public static class Init
    {
        // 初始化入口，后续可扩展更多初始化方法
        public static void InitializeAll(RestaurantDbContext db)
        {
            //InitializeStores(db);
            //InitializeCategories(db);
            //InitializeDishes(db);
            //InitializeCustomer(db);
            //InitializeTable(db);
            //InitializeOrder(db);
        }

        // 初始化订单，先删除所有订单
        private static void InitializeOrder(RestaurantDbContext db)
        {
            var (success, message) = DbUtils.DeleteAllEntities<Order>(db);
            Console.WriteLine(message); // 输出操作结果信息
            // 如果需要根据结果做进一步处理，可以这样：
            if (!success)
            {
                Console.WriteLine("清空Order表失败");
            }
        }

        private static void InitializeStores(RestaurantDbContext db)
        {
            var store = new Store
            {
                StoreId = 1,
                StoreName = "旗舰店",
                Address = "北京市朝阳区CBD核心区1号",
                Phone = "010-88888888",
                ManagerId = null, // 如有员工可指定
                OpeningHours = "10:00-22:00",
                Status = "营业中",
                OpenDate = new DateTime(2022, 1, 1),
                RegionId = null, // 如有区域可指定
                StoreSize = 300.00m,
                MonthlyRent = 50000.00m,
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now
            };

            var exist = db.Stores.Count(s => s.StoreId == store.StoreId) > 0;
            if (!exist)
            {
                DbUtils.AddEntity(db, store);
            }
        }

        // 分类表初始化
        private static void InitializeCategories(RestaurantDbContext db)
        {
            var categories = new List<Category>
            {
                new Category { CategoryID = 1, CategoryName = "热菜", SortOrder = 1, IsActive = "Y" },
                new Category { CategoryID = 2, CategoryName = "凉菜", SortOrder = 2, IsActive = "Y" },
                new Category { CategoryID = 3, CategoryName = "饮品", SortOrder = 3, IsActive = "Y" }
            };
            foreach (var cat in categories)
            {
                // 使用DbUtils的AddEntity方法添加，已存在则跳过
                var exist = db.Categories.Count(c => c.CategoryID == cat.CategoryID) > 0;
                if (!exist)
                {
                    DbUtils.AddEntity(db, cat);
                }
            }
        }

        // 菜品表初始化
        private static void InitializeDishes(RestaurantDbContext db)
        {
            var now = DateTime.Now;
            var dishes = new List<Dish>
            {
                new Dish
                {
                    DishId = 1,
                    DishName = "宫保鸡丁",
                    Price = 28.00m,
                    CategoryId = 1, // 热菜
                    IsAvailable = "Y",
                    Description = "经典川菜，微辣，鸡肉鲜嫩。",
                    ImageUrl = "/api/1",
                    EstimatedTime = 15,
                    StoreId = 1,
                    CreateTime = now,
                    UpdateTime = now
                },
                new Dish
                {
                    DishId = 2,
                    DishName = "拍黄瓜",
                    Price = 12.00m,
                    CategoryId = 2, // 凉菜
                    IsAvailable = "Y",
                    Description = "清爽开胃，蒜香浓郁。",
                    ImageUrl = "/api/2",
                    EstimatedTime = 5,
                    StoreId = 1,
                    CreateTime = now,
                    UpdateTime = now
                },
                new Dish
                {
                    DishId = 3,
                    DishName = "可乐",
                    Price = 6.00m,
                    CategoryId = 3, // 饮品
                    IsAvailable = "Y",
                    Description = "冰镇可乐，畅爽解渴。",
                    ImageUrl = "/api/3",
                    EstimatedTime = 2,
                    StoreId = 1,
                    CreateTime = now,
                    UpdateTime = now
                }
            };
            foreach (var dish in dishes)
            {
                // 使用DbUtils的AddEntity方法添加，已存在则跳过
                var exist = db.Dishes.Count(d => d.DishId == dish.DishId) > 0;
                if (!exist)
                {
                    DbUtils.AddEntity(db, dish);
                }
            }
        }

        private static void InitializeCustomer(RestaurantDbContext db)
        {
            // 创建ID为1的实体，设置基础信息
            var Customer = new Customer
            {
                CustomerId = 1,
                CustomerName = "默认会员", // 会员名称（必填项）
                Phone = "13800138000", // 联系电话
                Email = "vip@example.com", // 邮箱
                Gender = "M",
                Birthday = new DateTime(1990, 1, 1), // 生日
                RegisterTime = DateTime.Now, // 注册时间（默认当前时间）
                Status = "正常", // 会员状态
                TotalConsumption = 0, // 初始消费总额为0
                VIPPoints = 0, // 初始积分
                VIPLevelId = null // 初始无会员等级（可根据实际等级表设置）
            };

            // 检查ID为1的是否已存在
            bool exists = db.Customers.Count(c => c.CustomerId == Customer.CustomerId) > 0;
            if (!exists)
            {
                // 使用工具类添加实体
                DbUtils.AddEntity(db, Customer);
            }
        }

        private static void InitializeTable(RestaurantDbContext db)
        {
            // 创建桌号为1的桌台实体
            var table = new TableInfo
            {
                TableId = 1,                   // 桌台ID
                TableNumber = "A-1",              // 桌号（业务标识，此处与ID一致）
                Area = "大厅",                  // 所在区域（如大厅、包间等）
                Capacity = 4,                   // 可容纳人数
                Status = "空闲",                // 初始状态为空闲
                StoreId = 1,
            };

            // 检查ID为1的桌台是否已存在
            bool exists = db.TableInfos.Count(t => t.TableId == table.TableId) > 0;
            if (!exists)
            {
                // 不存在则添加到数据库
                DbUtils.AddEntity(db, table);
            }
        }
    }
}