using RestaurantManagement.Services;

var builder = WebApplication.CreateBuilder(args);

// 配置JSON序列化选项
// builder.Services.ConfigureHttpJsonOptions(options =>
// {
//     options.SerializerOptions.PropertyNamingPolicy = null; // 保持原始命名
// });

// 添加服务
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });
builder.Services.AddScoped<DatabaseService>();
builder.Services.AddScoped<TableService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<MenuService>();
builder.Services.AddScoped<StaffService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<DepartmentService>();
builder.Services.AddScoped<StoreService>();
builder.Services.AddScoped<ScheduleService>();

var app = builder.Build();

// 配置HTTP请求管道
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllers();

// 默认路由到登录页面
app.MapFallbackToFile("login.html");

app.Run();
