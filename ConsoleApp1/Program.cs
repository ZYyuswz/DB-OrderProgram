using ConsoleApp1.Services;

var builder = WebApplication.CreateBuilder(args);

// 配置服务监听所有网络接口
builder.WebHost.UseUrls("http://localhost:5002");

// 添加服务到容器
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // 保持属性名的大小写，不转换为小写
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 注册自定义服务
builder.Services.AddScoped<DatabaseService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<PointsService>();
builder.Services.AddScoped<MemberService>();

// 配置CORS以支持小程序跨域请求
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMiniProgram", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 配置HTTP请求管道
// 始终启用 Swagger（用于测试）
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowMiniProgram");
app.UseRouting();
app.MapControllers();

// 启动API服务
app.Logger.LogInformation("=== 餐饮门店智能管理系统 - Web API 启动 ===");
app.Logger.LogInformation("API服务已启动，支持小程序前端访问");
app.Logger.LogInformation("API文档地址: http://localhost:5002/swagger");

app.Run();