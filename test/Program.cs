using Oracle.ManagedDataAccess.Client;
using System;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Oracle数据库连接测试程序 ===");
        Console.WriteLine();

        var dbManager = new OracleDbManager();
        await dbManager.TestConnectionAsync();
        
        Console.WriteLine();
        Console.WriteLine("按任意键退出...");
        Console.ReadKey();
    }
}

public class OracleDbManager
{
    public async Task TestConnectionAsync()
    {
        // 1. 构建连接字符串（根据appsettings.json中的配置）
        string host = "114.55.142.198"; // 数据库服务器IP
        int port = 1521;                // 端口号
        string serviceName = "XEPDB1";  // 服务名
        string user = "XCY";            // 用户名
        string password = "TONGJI";     // 密码

        string connStr = $"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={host})(PORT={port}))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME={serviceName})));User Id={user};Password={password};";

        Console.WriteLine($"连接信息:");
        Console.WriteLine($"- 服务器: {host}:{port}");
        Console.WriteLine($"- 服务名: {serviceName}");
        Console.WriteLine($"- 用户名: {user}");
        Console.WriteLine();

        // 2. 创建并打开连接
        using (OracleConnection conn = new OracleConnection(connStr))
        {
            try
            {
                Console.WriteLine("正在尝试连接到 Oracle 数据库...");
                await conn.OpenAsync();
                Console.WriteLine("✅ 数据库连接成功！");
                Console.WriteLine();

                // 3. 测试基本查询
                await TestBasicQuery(conn);

                // 4. 获取数据库信息
                await GetDatabaseInfo(conn);

                // 5. 查询用户表
                await GetUserTables(conn);

                // 6. 自定义查询
                await CustomQuery(conn);

            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ 数据库连接或查询失败");
                Console.WriteLine($"错误信息: {ex.Message}");
                Console.WriteLine();

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"内部错误: {ex.InnerException.Message}");
                }
            }
        }
    }

    private async Task TestBasicQuery(OracleConnection conn)
    {
        Console.WriteLine("🔍 执行基本测试查询...");
        try
        {
            string sql = "SELECT 'Hello from Oracle!' AS message FROM DUAL";
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            {
                object result = await cmd.ExecuteScalarAsync();
                Console.WriteLine($"查询结果: {result}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 基本查询失败: {ex.Message}");
        }
        Console.WriteLine();
    }

    private async Task GetDatabaseInfo(OracleConnection conn)
    {
        Console.WriteLine("📊 获取数据库信息...");
        try
        {
            // 获取Oracle版本
            string versionSql = "SELECT banner FROM v$version WHERE banner LIKE 'Oracle%'";
            using (OracleCommand cmd = new OracleCommand(versionSql, conn))
            {
                object version = await cmd.ExecuteScalarAsync();
                Console.WriteLine($"Oracle版本: {version}");
            }

            // 获取当前用户
            string userSql = "SELECT USER FROM DUAL";
            using (OracleCommand cmd = new OracleCommand(userSql, conn))
            {
                object currentUser = await cmd.ExecuteScalarAsync();
                Console.WriteLine($"当前用户: {currentUser}");
            }

            // 获取数据库时间
            string timeSql = "SELECT SYSDATE FROM DUAL";
            using (OracleCommand cmd = new OracleCommand(timeSql, conn))
            {
                object dbTime = await cmd.ExecuteScalarAsync();
                Console.WriteLine($"数据库时间: {dbTime}");
            }

            Console.WriteLine($"本地时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 获取数据库信息失败: {ex.Message}");
        }
        Console.WriteLine();
    }

    private async Task GetUserTables(OracleConnection conn)
    {
        Console.WriteLine("📋 查询用户表列表...");
        try
        {
            string tablesSql = "SELECT table_name FROM user_tables ORDER BY table_name";
            using (OracleCommand cmd = new OracleCommand(tablesSql, conn))
            using (OracleDataReader reader = await cmd.ExecuteReaderAsync())
            {
                var tables = new List<string>();
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }

                if (tables.Count > 0)
                {
                    Console.WriteLine($"找到 {tables.Count} 个用户表:");
                    foreach (var table in tables)
                    {
                        Console.WriteLine($"  - {table}");
                    }
                }
                else
                {
                    Console.WriteLine("当前用户没有任何表");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 查询表列表失败: {ex.Message}");
        }
    }

    private async Task CustomQuery(OracleConnection conn)
    {
        Console.WriteLine("🔍 执行自定义查询 - 流式处理查询PUB.TABLEINFO表...");
        try
        {
            string sql = "SELECT * FROM PUB.TABLEINFO";
            using (OracleCommand cmd = new OracleCommand(sql, conn))
            using (OracleDataReader reader = await cmd.ExecuteReaderAsync())
            {
                // 获取表结构信息
                int columnCount = reader.FieldCount;
                Console.WriteLine($"表结构 ({columnCount} 列):");
                for (int i = 0; i < columnCount; i++)
                {
                    Console.WriteLine($"  {i + 1}. {reader.GetName(i)} ({reader.GetDataTypeName(i)})");
                }
                Console.WriteLine();

                // 流式读取所有数据
                int rowCount = 0;
                Console.WriteLine("表数据:");
                Console.WriteLine(new string('-', 80));
                
                while (await reader.ReadAsync())
                {
                    rowCount++;
                    Console.Write($"第{rowCount}行: ");
                    
                    for (int i = 0; i < columnCount; i++)
                    {
                        object value = reader.IsDBNull(i) ? "NULL" : reader.GetValue(i);
                        Console.Write($"{reader.GetName(i)}={value}");
                        if (i < columnCount - 1) Console.Write(", ");
                    }
                    Console.WriteLine();
                    
                    // 每10行显示一个进度提示
                    if (rowCount % 10 == 0)
                    {
                        Console.WriteLine($"[已读取 {rowCount} 行...]");
                    }
                }
                
                Console.WriteLine(new string('-', 80));
                Console.WriteLine($"✅ 流式处理完成，总共读取了 {rowCount} 行数据");
                
                if (rowCount == 0)
                {
                    Console.WriteLine("⚠️ 表中没有数据");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 流式查询失败: {ex.Message}");
        }
        Console.WriteLine();
    }
}
