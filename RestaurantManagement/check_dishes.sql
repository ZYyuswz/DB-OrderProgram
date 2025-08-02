-- 检查菜品表中的数据
SELECT DishID, DishName, Price, IsAvailable, CategoryID 
FROM PUB.Dish 
WHERE DishID IN (1, 2, 3, 4, 5)
ORDER BY DishID;

-- 检查菜品2是否存在
SELECT COUNT(*) as Count 
FROM PUB.Dish 
WHERE DishID = 2;

-- 检查所有菜品
SELECT DishID, DishName, Price, IsAvailable, CategoryID 
FROM PUB.Dish 
ORDER BY DishID;
