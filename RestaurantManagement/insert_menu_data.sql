-- 插入菜品分类测试数据
INSERT INTO PUB.Category (CategoryID, CategoryName, Description, SortOrder, IsActive) VALUES (1, '热菜', '各种热炒菜品', 1, 'Y');
INSERT INTO PUB.Category (CategoryID, CategoryName, Description, SortOrder, IsActive) VALUES (2, '凉菜', '各种凉拌菜品', 2, 'Y');
INSERT INTO PUB.Category (CategoryID, CategoryName, Description, SortOrder, IsActive) VALUES (3, '汤品', '各种汤类', 3, 'Y');
INSERT INTO PUB.Category (CategoryID, CategoryName, Description, SortOrder, IsActive) VALUES (4, '主食', '米饭面条等主食', 4, 'Y');
INSERT INTO PUB.Category (CategoryID, CategoryName, Description, SortOrder, IsActive) VALUES (5, '饮品', '各种饮料', 5, 'Y');

-- 插入菜品测试数据
INSERT INTO PUB.Dish (DishID, StoreID, DishName, Price, CategoryID, IsAvailable, Description, ImageURL, EstimatedTime, CreateTime, UpdateTime) 
VALUES (1, 1, '宫保鸡丁', 28.00, 1, 'Y', '经典川菜，香辣可口', '', 15, SYSDATE, SYSDATE);

INSERT INTO PUB.Dish (DishID, StoreID, DishName, Price, CategoryID, IsAvailable, Description, ImageURL, EstimatedTime, CreateTime, UpdateTime) 
VALUES (2, 1, '麻婆豆腐', 18.00, 1, 'Y', '传统川菜，麻辣鲜香', '', 12, SYSDATE, SYSDATE);

INSERT INTO PUB.Dish (DishID, StoreID, DishName, Price, CategoryID, IsAvailable, Description, ImageURL, EstimatedTime, CreateTime, UpdateTime) 
VALUES (3, 1, '糖醋里脊', 32.00, 1, 'Y', '酸甜可口，老少皆宜', '', 18, SYSDATE, SYSDATE);

INSERT INTO PUB.Dish (DishID, StoreID, DishName, Price, CategoryID, IsAvailable, Description, ImageURL, EstimatedTime, CreateTime, UpdateTime) 
VALUES (4, 1, '凉拌黄瓜', 8.00, 2, 'Y', '清爽开胃小菜', '', 5, SYSDATE, SYSDATE);

INSERT INTO PUB.Dish (DishID, StoreID, DishName, Price, CategoryID, IsAvailable, Description, ImageURL, EstimatedTime, CreateTime, UpdateTime) 
VALUES (5, 1, '蒜泥白肉', 26.00, 2, 'Y', '经典川味凉菜', '', 8, SYSDATE, SYSDATE);

INSERT INTO PUB.Dish (DishID, StoreID, DishName, Price, CategoryID, IsAvailable, Description, ImageURL, EstimatedTime, CreateTime, UpdateTime) 
VALUES (6, 1, '番茄鸡蛋汤', 12.00, 3, 'Y', '家常营养汤品', '', 10, SYSDATE, SYSDATE);

INSERT INTO PUB.Dish (DishID, StoreID, DishName, Price, CategoryID, IsAvailable, Description, ImageURL, EstimatedTime, CreateTime, UpdateTime) 
VALUES (7, 1, '紫菜蛋花汤', 10.00, 3, 'Y', '清淡营养汤品', '', 8, SYSDATE, SYSDATE);

INSERT INTO PUB.Dish (DishID, StoreID, DishName, Price, CategoryID, IsAvailable, Description, ImageURL, EstimatedTime, CreateTime, UpdateTime) 
VALUES (8, 1, '白米饭', 3.00, 4, 'Y', '香软白米饭', '', 2, SYSDATE, SYSDATE);

INSERT INTO PUB.Dish (DishID, StoreID, DishName, Price, CategoryID, IsAvailable, Description, ImageURL, EstimatedTime, CreateTime, UpdateTime) 
VALUES (9, 1, '炒河粉', 15.00, 4, 'Y', '广式炒河粉', '', 12, SYSDATE, SYSDATE);

INSERT INTO PUB.Dish (DishID, StoreID, DishName, Price, CategoryID, IsAvailable, Description, ImageURL, EstimatedTime, CreateTime, UpdateTime) 
VALUES (10, 1, '可乐', 5.00, 5, 'Y', '冰镇可口可乐', '', 1, SYSDATE, SYSDATE);

INSERT INTO PUB.Dish (DishID, StoreID, DishName, Price, CategoryID, IsAvailable, Description, ImageURL, EstimatedTime, CreateTime, UpdateTime) 
VALUES (11, 1, '橙汁', 8.00, 5, 'Y', '鲜榨橙汁', '', 3, SYSDATE, SYSDATE);

-- 提交事务
COMMIT;
