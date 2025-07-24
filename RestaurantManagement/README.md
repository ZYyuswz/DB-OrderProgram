# 餐饮管理系统（商家端）部署说明

## 项目结构

```
RestaurantManagement/
├── Controllers/           # API控制器
│   ├── TablesController.cs      # 桌台管理API
│   ├── OrdersController.cs      # 订单管理API
│   ├── MenuController.cs        # 菜单管理API
│   ├── StaffController.cs       # 员工管理API
│   └── InventoryController.cs   # 库存管理API
├── Models/               # 数据模型
│   └── Models.cs
├── Services/             # 业务服务层
│   ├── DatabaseService.cs      # 数据库连接服务
│   ├── TableService.cs         # 桌台业务逻辑
│   ├── OrderService.cs         # 订单业务逻辑
│   ├── MenuService.cs          # 菜单业务逻辑
│   ├── StaffService.cs         # 员工业务逻辑
│   └── InventoryService.cs     # 库存业务逻辑
├── wwwroot/              # 前端静态文件
│   ├── css/
│   │   └── styles.css          # 主样式文件
│   ├── js/
│   │   └── common.js           # 通用JavaScript函数
│   ├── images/                 # 图片资源
│   ├── index.html              # 主页
│   ├── tables.html             # 桌台管理页面
│   ├── orders.html             # 订单管理页面（待创建）
│   ├── menu.html               # 菜单管理页面（待创建）
│   ├── staff.html              # 员工管理页面（待创建）
│   └── inventory.html          # 库存管理页面（待创建）
├── appsettings.json      # 应用配置文件
├── Program.cs            # 应用入口点
└── RestaurantManagement.csproj  # 项目文件
```

## 数据库配置

### 1. 修改连接字符串

编辑 `appsettings.json` 文件中的数据库连接信息：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_IP;Database=RestaurantDB;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=true;"
  }
}
```

请将以下参数替换为实际的数据库信息：
- `YOUR_SERVER_IP`: 数据库服务器IP地址
- `YOUR_USERNAME`: 数据库用户名
- `YOUR_PASSWORD`: 数据库密码

### 2. 确保数据库表结构

系统需要以下数据库表，请确保数据库中包含这些表：

- `TableInfo` - 桌台信息表
- `Orders` - 订单表
- `OrderDetail` - 订单详情表
- `Dish` - 菜品表
- `Category` - 菜品分类表
- `Staff` - 员工表
- `Attendance` - 考勤记录表
- `RawMaterial` - 原材料表
- `Supplier` - 供应商表
- `PurchaseRecord` - 采购记录表
- `Recipe` - 菜谱表（菜品与原材料关系）

## 运行步骤

### 1. 安装依赖

进入项目目录后，运行以下命令安装NuGet包：

```bash
dotnet restore
```

### 2. 配置数据库连接

修改 `appsettings.json` 中的连接字符串。

### 3. 构建项目

```bash
dotnet build
```

### 4. 运行项目

```bash
dotnet run
```

### 5. 访问系统

默认情况下，系统将在以下地址运行：
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001

打开浏览器访问上述地址即可使用系统。

## 功能模块

### 成员A - 订单与桌台管理
- **桌台管理**: 实时查看桌台状态，支持开台、清台操作
- **订单管理**: 查看订单详情，处理加菜、结账等操作
- **API接口**: 
  - `GET /api/tables` - 获取所有桌台状态
  - `GET /api/orders/{id}` - 获取订单详情
  - `POST /api/orders` - 创建新订单

### 成员B - 员工与考勤管理
- **员工管理**: 员工信息的增删改查
- **考勤管理**: 员工打卡、考勤记录查询
- **排班管理**: 员工排班安排（待完善）

### 成员C - 库存与菜单管理
- **菜单管理**: 菜品分类和菜品信息管理
- **库存管理**: 原材料库存监控、采购管理
- **供应商管理**: 供应商信息维护
- **API接口**:
  - `GET /api/menu` - 获取完整菜单信息
  - `GET /api/dishes/{id}` - 获取菜品详情

## 技术栈

- **后端**: ASP.NET Core 8.0
- **数据库**: SQL Server（使用Dapper ORM）
- **前端**: 原生HTML + CSS + JavaScript
- **依赖包**:
  - Microsoft.Data.SqlClient - SQL Server数据库连接
  - Dapper - 轻量级ORM
  - Newtonsoft.Json - JSON序列化

## 开发说明

### 添加新功能

1. 在 `Models/Models.cs` 中添加新的数据模型
2. 在 `Services/` 目录下创建对应的业务服务类
3. 在 `Controllers/` 目录下创建API控制器
4. 在 `wwwroot/` 下创建前端页面
5. 在 `Program.cs` 中注册新的服务

### 前端页面开发

- 所有页面共享 `css/styles.css` 样式文件
- 通用JavaScript函数位于 `js/common.js`
- API调用使用 `API.get()`, `API.post()` 等封装函数
- 通知消息使用 `Utils.showNotification()`

### API开发规范

- 所有API使用 RESTful 风格
- 返回JSON格式数据
- 错误时返回适当的HTTP状态码
- 使用异步方法提高性能

## 注意事项

1. 首次运行前必须配置正确的数据库连接字符串
2. 确保数据库服务器允许外部连接
3. 生产环境部署时建议使用HTTPS
4. 定期备份数据库数据
5. 监控系统性能和错误日志

## 待完成功能

1. 剩余前端页面的开发（orders.html, menu.html, staff.html, inventory.html）
2. 促销活动管理功能
3. 数据报表和统计功能
4. 用户权限管理
5. 系统日志记录
6. 移动端适配优化
