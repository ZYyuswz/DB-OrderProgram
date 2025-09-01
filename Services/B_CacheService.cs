using DBManagement.Models;

namespace DBManagement.Service
{
    public class CacheService
    {
        private readonly RestaurantDbContext _db;

        // 通过构造函数注入数据库上下文
        public CacheService(RestaurantDbContext db)
        {
            _db = db;
        }

        public (bool success, string message, int cache_id) CreateCache(ShoppingCache cache)
        {
            try
            {
                // 生成CacheId
                cache.CacheId = GenerateCacheId();
                cache.Quantity = 1;
                _db.ShoppingCache.Add(cache);
                _db.SaveChanges();
                return (true, "缓存创建成功", cache.CacheId);
            }
            catch (Exception ex)
            {
                return (false, $"缓存创建失败: {ex.Message}", -1); // -1表示创建错误
            }
        }

        public (bool success, string message) DeleteCache(int cacheId)
        {
            try
            {
                // 1. 查找
                var cache = _db.ShoppingCache
                    .FirstOrDefault(o => o.CacheId == cacheId);

                if (cache == null)
                {
                    return (false, $"未找到 ID 为 {cacheId} 的缓存");
                }

                // 2. 删除
                _db.ShoppingCache.Remove(cache);

                // 3. 保存更改（一次性提交删除操作）
                _db.SaveChanges();

                return (true, $"缓存 {cacheId} 成功删除");
            }
            catch (Exception ex)
            {
                return (false, $"删除缓存失败: {ex.Message}");
            }
        }

        public List<ShoppingCache> ReturnCacheByTable(int tableId)
        {
            // 1. 查询符合条件的记录
            var result = _db.Set<ShoppingCache>()
                           .Where(sc => sc.TableId == tableId && sc.Status == "PENDING")
                           .ToList();
            // 2. 如果找到记录，更新Status
            if (result.Any())
            {
                foreach (var cache in result)
                {
                    cache.Status = "ORDERED";
                }
                // 3. 保存更改到数据库
                _db.SaveChanges();
            }
            return result;
        }

        // 生成下一个CacheId
        private int GenerateCacheId()
        {
            // 从数据库获取当前最大的 CacheId
            int maxId = _db.ShoppingCache.Max(o => (int?)o.CacheId) ?? 0;
            return Interlocked.Increment(ref maxId);
        }


        public (bool success, string message) DeleteAllCacheByTableId(int tableId)
        {
            try
            {
                // 1. 查找所有匹配的记录
                var caches = _db.ShoppingCache
                    .Where(o => o.TableId == tableId)
                    .ToList();

                if (caches == null || !caches.Any())
                {
                    return (false, $"未找到桌号为 {tableId} 的缓存记录");
                }

                // 2. 删除所有记录
                _db.ShoppingCache.RemoveRange(caches);

                // 3. 保存更改
                _db.SaveChanges();

                return (true, $"成功删除桌号为 {tableId} 的 {caches.Count} 条缓存记录");
            }
            catch (Exception ex)
            {
                return (false, $"删除失败: {ex.Message}");
            }
        }
    }
}
