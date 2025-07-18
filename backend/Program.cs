using Microsoft.EntityFrameworkCore;
using DBManagement.Models;

// 创建 Web 应用构建器实例，用于配置和构建应用
var builder = WebApplication.CreateBuilder(args);

// 添加数据库上下文服务，配置 Oracle 数据库连接
builder.Services.AddDbContext<RestaurantDbContext>(options =>
{
    // 从配置文件中读取数据库连接参数， ASP.NET Core 默认会从 appsettings.json 文件读取
    var config = builder.Configuration;
    string host = config["Database:Host"];        // 数据库主机地址
    int port = config.GetValue<int>("Database:Port");  // 数据库端口
    string serviceName = config["Database:ServiceName"];  // 数据库服务名
    string user = config["Database:User"];        // 数据库用户名
    string password = config["Database:Password"];  // 数据库密码

    // 构建 Oracle 数据库连接字符串
    string connString = $"User Id={user};Password={password};" +
                       $"Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={host})(PORT={port})))" +
                       $"(CONNECT_DATA=(SERVICE_NAME={serviceName})))";

    // 使用构建的连接字符串配置 Oracle 数据库提供程序
    options.UseOracle(connString);
});

// 注册控制器服务，使应用能够处理 HTTP 请求
builder.Services.AddControllers();

// 注册 Swagger 服务（用于生成 API 文档）
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 构建 Web 应用实例
var app = builder.Build();

// 在开发环境中启用 Swagger UI，方便 API 测试和调试
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();         // 启用 Swagger 中间件
    app.UseSwaggerUI();       // 启用 Swagger UI 界面
}

// 配置应用的请求处理管道
app.UseRouting();             // 启用路由功能
app.UseAuthorization();       // 启用授权功能
app.MapControllers();         // 映射控制器路由

// 应用启动时初始化数据库
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // 获取数据库上下文实例
        var db = services.GetRequiredService<RestaurantDbContext>();

        // 调用自定义初始化方法初始化数据库结构和数据
        DBManagement.Utils.Init.InitializeAll(db);
        Console.WriteLine("数据库初始化成功！");
    }
    catch (Exception ex)
    {
        Console.WriteLine("数据库初始化失败: " + ex.Message);
    }
}

// 启动应用，开始监听 HTTP 请求
app.Run();