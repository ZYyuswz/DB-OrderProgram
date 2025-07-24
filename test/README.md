# Oracle数据库连接测试项目

这是一个简单的C#控制台应用程序，用于测试Oracle数据库连接。

## 项目信息

- **项目名称**: test
- **框架**: .NET 8.0
- **数据库**: Oracle Database
- **驱动**: Oracle.ManagedDataAccess.Core

## 数据库连接信息

- **服务器**: 114.55.142.198:1521
- **服务名**: XEPDB1
- **用户名**: XCY
- **密码**: TONGJI

## 功能特性

1. **连接测试**: 验证数据库连接是否正常
2. **基本查询**: 执行简单的SELECT查询
3. **数据库信息**: 获取Oracle版本、当前用户、数据库时间
4. **表列表**: 查询用户表和所有可访问的表

## 运行方法

### 1. 命令行运行
```bash
cd test
dotnet run
```

### 2. 构建后运行
```bash
cd test
dotnet build
dotnet bin/Debug/net8.0/test.dll
```

## 输出示例

```
=== Oracle数据库连接测试程序 ===

连接信息:
- 服务器: 114.55.142.198:1521
- 服务名: XEPDB1
- 用户名: XCY

正在尝试连接到 Oracle 数据库...
✅ 数据库连接成功！

🔍 执行基本测试查询...
查询结果: Hello from Oracle!

📊 获取数据库信息...
Oracle版本: Oracle Database 21c Express Edition Release 21.0.0.0.0
当前用户: XCY
数据库时间: 2024/07/24 18:30:45
本地时间: 2024-07-24 18:30:45

📋 查询用户表列表...
找到 5 个用户表:
  - EMPLOYEES
  - DEPARTMENTS
  - ORDERS
  - PRODUCTS
  - CUSTOMERS

按任意键退出...
```

## 故障排除

### 常见错误及解决方案

1. **连接超时**
   - 检查网络连接
   - 确认服务器IP和端口是否正确
   - 检查防火墙设置

2. **用户名或密码错误**
   - 确认用户名和密码是否正确
   - 检查用户是否有连接权限

3. **服务名错误**
   - 确认SERVICE_NAME是否正确
   - 可以尝试使用SID而不是SERVICE_NAME

4. **Oracle驱动问题**
   - 确保已安装Oracle.ManagedDataAccess.Core包
   - 运行 `dotnet restore` 恢复包

## 代码结构

- `Program.cs`: 主程序入口
- `OracleDbManager.cs`: 数据库管理类（包含在Program.cs中）
- `test.csproj`: 项目配置文件

## 依赖项

- Oracle.ManagedDataAccess.Core (23.9.1)

## 注意事项

- 此项目仅用于测试数据库连接
- 包含数据库连接信息，请注意安全
- 建议在生产环境中使用配置文件管理连接字符串
