using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace DBManagement.Utils
{
    // 数据库通用工具类，支持所有实体的增删改查操作
    public static class DbUtils
    {
        // 新增单个实体
        // T 实体类型
        // db 数据库上下文
        // entity 要添加的实体对象
        // returns 操作结果和提示信息
        // where T : class 表示 T 必须是一个引用类型
        public static (bool success, string message) AddEntity<T>(DbContext db, T entity) where T : class
        {
            try
            {
                db.Set<T>().Add(entity); // db.Set<T>()：返回 DbSet<T> 对象
                var result = db.SaveChanges(); // SaveChanges() 方法会将所有更改保存到数据库
                Console.WriteLine("SaveChanges");
                return (result > 0, result > 0 ? "添加成功" : "没有数据被添加");
            }
            catch (Exception ex)
            {
                Console.WriteLine("添加失败:  " + ex.ToString());
                return (false, $"添加失败: {ex.Message}");
            }
        }

        // 批量新增实体
        // entities 要添加的实体集合
        // returns 操作结果和提示信息
        public static (bool success, string message) AddEntities<T>(DbContext db, IEnumerable<T> entities) where T : class
        {
            try
            {
                db.Set<T>().AddRange(entities);
                var result = db.SaveChanges();
                return (result > 0, $"成功添加 {result} 条记录");
            }
            catch (Exception ex)
            {
                Console.WriteLine("添加失败:  " + ex.ToString());
                return (false, $"批量添加失败: {ex.Message}");
            }
        }

        // 查询所有实体（无跟踪，适合只读场景）
        // returns 实体列表
        public static List<T> GetAll<T>(DbContext db) where T : class
        {
            return db.Set<T>().AsNoTracking().ToList(); // AsNoTracking() 方法表示查询结果不进行跟踪，适合只读操作
        }

        // 按条件查询实体（无跟踪，适合只读场景）
        // predicate 查询条件表达式
        // returns 实体列表
        public static List<T> GetWhere<T>(DbContext db, Expression<Func<T, bool>> predicate) where T : class
        {
            return db.Set<T>().Where(predicate).AsNoTracking().ToList();
        }

        // 按主键查询单个实体
        // id 主键
        // returns 实体对象
        public static T GetById<T>(DbContext db, object id) where T : class
        {
            return db.Set<T>().Find(id);
        }

        // 更新单个实体的指定属性（先查找数据库是否存在，再只修改指定属性）
        // updateAction 对实体进行属性赋值的委托
        // returns 操作结果和提示信息
        public static (bool success, string message) UpdateEntity<T>(DbContext db, object id, Action<T> updateAction) where T : class
        {
            try
            {
                var entity = db.Set<T>().Find(id);
                if (entity == null)
                    return (false, "未找到指定ID的记录");

                updateAction(entity);
                var result = db.SaveChanges();
                return (result > 0, "更新成功");
            }
            catch (Exception ex)
            {
                Console.WriteLine("更新失败:  " + ex.ToString());
                return (false, $"更新失败: {ex.Message}");
            }
        }

        // 条件批量更新（只修改指定属性，先查找数据库是否存在）
        // updateAction 对实体进行属性赋值的委托
        // returns 操作结果、提示信息和影响行数
        public static (bool success, string message, int affectedRows) UpdateWhere<T>(
            DbContext db,
            Expression<Func<T, bool>> predicate,
            Action<T> updateAction) where T : class
        {
            try
            {
                var entities = db.Set<T>().Where(predicate).ToList();
                if (!entities.Any())
                    return (false, "没有找到符合条件的记录", 0);

                foreach (var entity in entities)
                {
                    updateAction(entity);
                }

                var result = db.SaveChanges();
                return (result > 0, $"成功更新 {result} 条记录", result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("更新失败:  " + ex.ToString());
                return (false, $"条件更新失败: {ex.Message}", 0);
            }
        }

        // 删除单个实体
        // entity 要删除的实体对象
        // returns 操作结果和提示信息
        public static (bool success, string message) DeleteEntity<T>(DbContext db, T entity) where T : class
        {
            try
            {
                db.Set<T>().Remove(entity);
                var result = db.SaveChanges();
                return (result > 0, result > 0 ? "删除成功" : "没有数据被删除");
            }
            catch (Exception ex)
            {
                Console.WriteLine("删除失败:  " + ex.ToString());
                return (false, $"删除失败: {ex.Message}");
            }
        }

        // 按主键删除单个实体
        // id 主键
        // returns 操作结果和提示信息
        public static (bool success, string message) DeleteById<T>(DbContext db, object id) where T : class
        {
            try
            {
                var entity = db.Set<T>().Find(id);
                if (entity == null)
                    return (false, "未找到指定ID的记录");

                db.Set<T>().Remove(entity);
                var result = db.SaveChanges();
                return (result > 0, result > 0 ? "删除成功" : "没有数据被删除");
            }
            catch (Exception ex)
            {
                Console.WriteLine("删除失败:  " + ex.ToString());
                return (false, $"删除失败: {ex.Message}");
            }
        }

        // 条件批量删除
        // predicate 查询条件表达式
        // returns 操作结果、提示信息和影响行数
        public static (bool success, string message, int affectedRows) DeleteWhere<T>(
            DbContext db,
            Expression<Func<T, bool>> predicate) where T : class
        {
            try
            {
                var entities = db.Set<T>().Where(predicate).ToList();
                if (!entities.Any())
                    return (false, "没有找到符合条件的记录", 0);

                db.Set<T>().RemoveRange(entities);
                var result = db.SaveChanges();
                return (result > 0, $"成功删除 {result} 条记录", result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("条件删除失败:  " + ex.ToString());
                return (false, $"条件删除失败: {ex.Message}", 0);
            }
        }
    }
}