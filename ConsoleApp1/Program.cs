using ConsoleApp1.Services;
using DBManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ========== 1. 配置基础服务 ==========
// 配置服务监听所有网络接口
builder.WebHost.UseUrls("http://localhost:5002");

// 注册控制器和Swagger
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // 保持属性名的大小写，不转换为小写
        // options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "餐饮门店智能管理系统",
        Version = "v1",
        Description = "餐饮门店智能管理系统 Web API"
    });

    // 添加这行代码解决SchemaId冲突
    c.CustomSchemaIds(type => type.FullName);
});

// ========== 2. 注册所有自定义服务 ==========
// 注册自定义服务
builder.Services.AddScoped<DatabaseService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<PointsService>();
builder.Services.AddScoped<MemberService>();
builder.Services.AddScoped<QRCodeService>();
builder.Services.AddHttpClient();

// 来自第二个后端（B_DBManagement）的数据库上下文
builder.Services.AddDbContext<RestaurantDbContext>(options =>
{
    var config = builder.Configuration;
    string host = config["Database:Host"];
    int port = config.GetValue<int>("Database:Port");
    string serviceName = config["Database:ServiceName"];
    string user = config["Database:User"];
    string password = config["Database:Password"];

    string connString = $"User Id={user};Password={password};" +
                       $"Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={host})(PORT={port})))" +
                       $"(CONNECT_DATA=(SERVICE_NAME={serviceName})))";

    options.UseOracle(connString);
});

// ========== 3. 配置跨域（CORS） ==========
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMiniProgram", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ========== 4. 构建应用 ==========
var app = builder.Build();

// ========== 5. 中间件配置 ==========
// 始终启用Swagger（无论环境）
app.UseSwagger();
app.UseSwaggerUI();
// 启用静态文件支持（用于HTML测试页面）
app.UseStaticFiles();
app.UseCors("AllowMiniProgram");
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// ========== 6. 初始化数据库 ==========
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // 初始化第二个后端的数据库
        var db = services.GetRequiredService<RestaurantDbContext>();
        DBManagement.Utils.Init.InitializeAll(db);
        Console.WriteLine("数据库初始化成功！");
    }
    catch (Exception ex)
    {
        Console.WriteLine("数据库初始化失败: " + ex.Message);
    }
}

// ========== 7. 启动应用 ==========
app.Logger.LogInformation("=== 餐饮门店智能管理系统 - Web API 启动 ===");
app.Logger.LogInformation("API服务已启动，支持小程序前端访问");
app.Logger.LogInformation("API文档地址: http://localhost:5002/swagger");

app.Run();