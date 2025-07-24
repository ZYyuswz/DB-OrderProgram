# 餐饮门店智能管理系统

## 项目概述

这是一个完整的餐饮门店智能管理系统，包含数据库设计和商家端管理系统的实现。

## 项目结构

```
DB-OrderProgram/
├── Details/                     # 项目文档和设计资料
│   ├── 分工.md                  # 开发分工文档
│   ├── 表.md                    # 数据库表结构说明
│   ├── 功能点.md                # 功能需求说明
│   └── *.xlsx                   # 数据库设计文档
├── RestaurantManagement/        # 商家端管理系统
│   ├── Controllers/             # API控制器
│   ├── Models/                  # 数据模型
│   ├── Services/                # 业务服务层
│   ├── wwwroot/                 # 前端静态文件
│   │   ├── css/
│   │   ├── js/
│   │   ├── index.html           # 主页
│   │   ├── tables.html          # 桌台管理页面
│   │   └── 其他管理页面...
│   ├── appsettings.json         # 应用配置
│   ├── Program.cs               # 应用入口
│   └── README.md                # 系统部署说明
└── README.md                    # 项目总览（本文件）
```

## 功能模块

根据分工文档，系统分为三个主要模块：

### 成员A：订单与桌台管理
- 桌台状态实时监控
- 开台、清台操作管理
- 订单详情查看和处理
- 加菜、结账功能

### 成员B：员工与考勤管理
- 员工信息管理
- 排班管理
- 考勤打卡和记录查询

### 成员C：库存与菜单管理
- 菜单分类和菜品管理
- 原材料库存监控
- 采购管理和入库操作
- 供应商信息维护

## 技术栈

- **后端**: ASP.NET Core 8.0 + C#
- **数据库**: SQL Server + Dapper ORM
- **前端**: 原生HTML + CSS + JavaScript
- **架构**: 三层架构（表示层、业务层、数据访问层）

## 快速开始

1. **配置数据库**
   ```bash
   # 修改 RestaurantManagement/appsettings.json 中的连接字符串
   "DefaultConnection": "Server=YOUR_SERVER_IP;Database=RestaurantDB;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=true;"
   ```

2. **安装依赖并运行**
   ```bash
   cd RestaurantManagement
   dotnet restore
   dotnet build
   dotnet run
   ```

3. **访问系统**
   - 浏览器打开: http://localhost:5000
   - 或: https://localhost:5001

详细的部署说明请参考 `RestaurantManagement/README.md`

## 开发进度

- [x] 数据库设计和表结构
- [x] 项目架构搭建
- [x] 后端API开发（完整）
- [x] 前端主页和桌台管理页面
- [ ] 其他前端管理页面
- [ ] 促销活动功能
- [ ] 数据报表功能
- [ ] 用户权限管理

## API接口

系统提供完整的RESTful API接口，主要包括：

- `/api/tables` - 桌台管理API
- `/api/orders` - 订单管理API  
- `/api/menu` - 菜单管理API
- `/api/staff` - 员工管理API
- `/api/inventory` - 库存管理API

详细的API文档请参考各Controller文件的注释。

## 贡献指南

1. 查看 `Details/分工.md` 了解模块分工
2. 根据分工认领相应的功能模块
3. 遵循现有的代码结构和命名规范
4. 提交前确保代码通过编译和基本测试

## 许可证

本项目仅用于学习和演示目的。

---

## 原始数据库设计文档

# DB-OrderProgram
同济大学数据库小学期项目
# 餐饮连锁管理系统数据库设计文档

## 系统概述

本系统是一个全面的餐饮连锁管理系统，旨在帮助餐饮企业实现多门店统一管理、标准化运营和数据分析。系统包含28个表，涵盖订单管理、菜品管理、客户管理、员工管理、库存管理和连锁管理等核心业务模块。

## 数据库表结构

### 1. 订单管理模块

#### Orders（订单主表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| OrderID | NUMBER(10) | PK | 订单的唯一标识符，使用序列自动生成 |
| OrderTime | DATE | DEFAULT SYSDATE | 客户下单的时间，默认为系统当前时间 |
| TableID | NUMBER(10) | FK | 关联桌台表的外键，标识订单所在的桌台 |
| CustomerID | NUMBER(10) | FK | 关联客户表的外键，标识下单的客户 |
| TotalPrice | NUMBER(12,2) | DEFAULT 0 | 订单总价，保留两位小数，默认为0 |
| OrderStatus | VARCHAR2(20) | CHECK | 订单状态，限定为：待处理/制作中/已完成/已结账 |
| StaffID | NUMBER(10) | FK | 关联员工表的外键，标识处理订单的服务员 |
| StoreID | NUMBER(10) | FK | 关联门店表的外键，标识订单所在的门店 |
| CreateTime | DATE | DEFAULT SYSDATE | 订单记录创建时间，默认为系统当前时间 |
| UpdateTime | DATE | DEFAULT SYSDATE | 订单记录最后更新时间，默认为系统当前时间 |

#### OrderDetail（订单详情表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| OrderDetailID | NUMBER(10) | PK | 订单详情的唯一标识符，使用序列自动生成 |
| OrderID | NUMBER(10) | FK | 关联订单表的外键，标识所属订单 |
| DishID | NUMBER(10) | FK | 关联菜品表的外键，标识所点的菜品 |
| Quantity | NUMBER(5) | NOT NULL | 菜品数量，不能为空 |
| UnitPrice | NUMBER(10,2) | NOT NULL | 下单时的菜品单价，保留两位小数，不能为空 |
| Subtotal | NUMBER(12,2) | NOT NULL | 小计金额（单价×数量），保留两位小数，不能为空 |
| SpecialRequests | VARCHAR2(500) | | 特殊要求或备注，如口味偏好、烹饪方式等 |

#### TableInfo（桌台信息表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| TableID | NUMBER(10) | PK | 桌台的唯一标识符，使用序列自动生成 |
| TableNumber | VARCHAR2(20) | NOT NULL, UNIQUE | 桌号，如A-01，不能为空且在同一门店内唯一 |
| Area | VARCHAR2(100) | | 桌台所在区域，如大厅、包间、露台等 |
| Capacity | NUMBER(3) | | 桌台可容纳的人数 |
| Status | VARCHAR2(20) | CHECK | 桌台状态，限定为：空闲/占用/清洁中/维修中 |
| StoreID | NUMBER(10) | FK | 关联门店表的外键，标识桌台所属的门店 |

#### TableReservation（桌台预约表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| ReservationID | NUMBER(10) | PK | 预约的唯一标识符，使用序列自动生成 |
| TableID | NUMBER(10) | FK | 关联桌台表的外键，标识被预约的桌台 |
| CustomerID | NUMBER(10) | FK | 关联客户表的外键，标识预约的客户 |
| CustomerName | VARCHAR2(100) | | 预约客户姓名，便于非会员预约 |
| ContactPhone | VARCHAR2(20) | | 联系电话，用于预约确认和提醒 |
| PartySize | NUMBER(3) | | 预约人数，用于安排合适大小的桌台 |
| ReservationTime | DATE | | 预约的用餐时间 |
| ExpectedDuration | NUMBER(3) | | 预计用餐时长(分钟)，用于桌台周转安排 |
| Status | VARCHAR2(20) | CHECK | 预约状态，限定为：已预约/已到店/已取消/未到店 |
| Notes | VARCHAR2(500) | | 预约备注，如特殊要求、庆祝活动等 |
| CreateTime | DATE | DEFAULT SYSDATE | 预约记录创建时间，默认为系统当前时间 |

### 2. 菜品管理模块

#### Dish（菜品表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| DishID | NUMBER(10) | PK | 菜品的唯一标识符，使用序列自动生成 |
| DishName | VARCHAR2(200) | NOT NULL, UNIQUE | 菜品名称，不能为空且唯一 |
| Price | NUMBER(10,2) | NOT NULL | 菜品价格，保留两位小数，不能为空 |
| CategoryID | NUMBER(10) | FK | 关联分类表的外键，标识菜品所属分类 |
| IsAvailable | CHAR(1) | CHECK | 是否可售，限定为Y(是)/N(否) |
| Description | VARCHAR2(1000) | | 菜品描述，包括原料、做法、特色等 |
| ImageURL | VARCHAR2(500) | | 菜品图片的URL链接 |
| EstimatedTime | NUMBER(5) | | 预计制作时间(分钟)，用于厨房排单和客户等待提示 |
| CreateTime | DATE | DEFAULT SYSDATE | 菜品记录创建时间，默认为系统当前时间 |
| UpdateTime | DATE | DEFAULT SYSDATE | 菜品记录最后更新时间，默认为系统当前时间 |

#### Category（菜品分类表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| CategoryID | NUMBER(10) | PK | 分类的唯一标识符，使用序列自动生成 |
| CategoryName | VARCHAR2(100) | NOT NULL, UNIQUE | 分类名称，如热菜、凉菜、主食等，不能为空且唯一 |
| SortOrder | NUMBER(5) | DEFAULT 0 | 分类在菜单中的排序号，默认为0 |
| IsActive | CHAR(1) | CHECK | 分类是否启用，限定为Y(是)/N(否)，默认为Y |

#### Recipe（菜品配方表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| RecipeID | NUMBER(10) | PK | 配方的唯一标识符，使用序列自动生成 |
| DishID | NUMBER(10) | FK | 关联菜品表的外键，标识配方所属的菜品 |
| MaterialID | NUMBER(10) | FK | 关联原材料表的外键，标识配方中使用的原材料 |
| RequiredQuantity | NUMBER(12,2) | NOT NULL | 需要的原材料数量，不能为空 |
| Unit | VARCHAR2(20) | | 计量单位，如克、个、瓶等 |
| CostPerUnit | NUMBER(10,2) | | 原材料单位成本，用于菜品成本核算 |
| Notes | VARCHAR2(500) | | 配方备注，如特殊处理方法、替代品等 |

#### Promotion（促销活动表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| PromotionID | NUMBER(10) | PK | 促销活动的唯一标识符，使用序列自动生成 |
| PromotionName | VARCHAR2(200) | NOT NULL | 促销活动名称，如"周年庆"、"节日特惠"等，不能为空 |
| DiscountType | VARCHAR2(20) | CHECK | 折扣类型，限定为：百分比/固定金额 |
| DiscountValue | NUMBER(10,2) | | 折扣值，百分比类型为折扣率，固定金额类型为减免金额 |
| BeginTime | DATE | | 促销开始时间 |
| EndTime | DATE | | 促销结束时间 |
| IsActive | CHAR(1) | CHECK | 是否激活，限定为Y(是)/N(否)，默认为Y |
| ApplicableCategories | VARCHAR2(500) | | 适用的菜品分类，多个分类用逗号分隔 |
| MinOrderAmount | NUMBER(10,2) | DEFAULT 0 | 最小订单金额要求，默认为0表示无限制 |

#### DishPromotion（菜品促销关联表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| DishPromotionID | NUMBER(10) | PK | 关联记录的唯一标识符，使用序列自动生成 |
| DishID | NUMBER(10) | FK | 关联菜品表的外键，标识参与促销的菜品 |
| PromotionID | NUMBER(10) | FK | 关联促销活动表的外键，标识菜品参与的促销活动 |

#### CorporateDish（总部标准菜单表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| CorporateDishID | NUMBER(10) | PK | 总部菜品的唯一标识符，使用序列自动生成 |
| DishName | VARCHAR2(200) | NOT NULL, UNIQUE | 菜品名称，不能为空且唯一 |
| StandardPrice | NUMBER(10,2) | NOT NULL | 总部制定的标准价格，保留两位小数，不能为空 |
| CategoryID | NUMBER(10) | FK | 关联分类表的外键，标识菜品所属分类 |
| Description | VARCHAR2(1000) | | 菜品描述，包括原料、做法、特色等 |
| ImageURL | VARCHAR2(500) | | 菜品标准图片的URL链接 |
| EstimatedTime | NUMBER(5) | | 标准制作时间(分钟) |
| IsActive | CHAR(1) | CHECK | 是否启用，限定为Y(是)/N(否)，默认为Y |
| CreateTime | DATE | DEFAULT SYSDATE | 记录创建时间，默认为系统当前时间 |
| UpdateTime | DATE | DEFAULT SYSDATE | 记录最后更新时间，默认为系统当前时间 |

#### StoreDishMapping（门店菜单关联表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| MappingID | NUMBER(10) | PK | 映射记录的唯一标识符，使用序列自动生成 |
| StoreID | NUMBER(10) | FK | 关联门店表的外键，标识映射所属的门店 |
| CorporateDishID | NUMBER(10) | FK | 关联总部菜单表的外键，标识总部标准菜品 |
| LocalDishID | NUMBER(10) | FK | 关联菜品表的外键，标识门店实际菜品 |
| LocalPrice | NUMBER(10,2) | | 门店实际定价，可根据当地市场情况调整 |
| IsAvailable | CHAR(1) | CHECK | 门店是否提供此菜品，限定为Y(是)/N(否)，默认为Y |
| CreateTime | DATE | DEFAULT SYSDATE | 映射记录创建时间，默认为系统当前时间 |
| UpdateTime | DATE | DEFAULT SYSDATE | 映射记录最后更新时间，默认为系统当前时间 |

### 3. 客户管理模块

#### Customer（客户表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| CustomerID | NUMBER(10) | PK | 客户的唯一标识符，使用序列自动生成 |
| CustomerName | VARCHAR2(100) | NOT NULL | 客户姓名，不能为空 |
| Phone | VARCHAR2(20) | | 客户电话，用于联系和作为登录账号 |
| Email | VARCHAR2(100) | | 客户邮箱，用于营销和通知 |
| Birthday | DATE | | 客户生日，用于生日优惠和祝福 |
| Gender | CHAR(1) | CHECK | 性别，限定为M(男)/F(女) |
| RegisterTime | DATE | DEFAULT SYSDATE | 注册时间，默认为系统当前时间 |
| LastVisitTime | DATE | | 最后一次光顾时间，用于客户活跃度分析 |
| TotalConsumption | NUMBER(12,2) | DEFAULT 0 | 累计消费金额，用于会员等级评定，默认为0 |
| VIPLevel | NUMBER(10) | FK | 关联会员等级表的外键，标识客户的会员等级 |
| VIPPoints | NUMBER(10) | DEFAULT 0 | 会员积分，用于兑换礼品或折扣，默认为0 |
| Status | VARCHAR2(20) | CHECK | 客户状态，限定为：正常/黑名单，默认为正常 |
| PreferredStoreID | NUMBER(10) | FK | 关联门店表的外键，标识客户常去的门店 |

#### VIPLevel（会员等级表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| LevelID | NUMBER(10) | PK | 等级的唯一标识符，使用序列自动生成 |
| LevelName | VARCHAR2(50) | NOT NULL, UNIQUE | 等级名称，如普通会员、黄金会员等，不能为空且唯一 |
| MinConsumption | NUMBER(10,2) | DEFAULT 0 | 达到该等级的最低累计消费金额，默认为0 |
| DiscountRate | NUMBER(5,2) | DEFAULT 100 | 该等级享受的折扣率，如90表示9折，默认为100(不打折) |
| PointsRate | NUMBER(5,2) | DEFAULT 1 | 该等级的积分倍率，如1.5表示消费同样金额获得1.5倍积分，默认为1 |
| Benefits | VARCHAR2(1000) | | 该等级的其他特权描述，如免费停车、生日双倍积分等 |

#### PointsRecord（积分记录表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| RecordID | NUMBER(10) | PK | 记录的唯一标识符，使用序列自动生成 |
| CustomerID | NUMBER(10) | FK | 关联客户表的外键，标识积分所属的客户 |
| OrderID | NUMBER(10) | FK | 关联订单表的外键，标识产生积分变动的订单 |
| PointsChange | NUMBER(10) | | 积分变动值，正数表示增加，负数表示减少 |
| RecordType | VARCHAR2(20) | CHECK | 记录类型，限定为：消费获得/兑换消费/过期扣除 |
| RecordTime | DATE | DEFAULT SYSDATE | 记录时间，默认为系统当前时间 |
| Description | VARCHAR2(500) | | 记录描述，说明积分变动的具体原因 |

#### CustomerLoginLog（客户登录日志表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| LogID | NUMBER(10) | PK | 日志的唯一标识符，使用序列自动生成 |
| CustomerID | NUMBER(10) | FK | 关联客户表的外键，标识登录的客户 |
| LoginTime | DATE | DEFAULT SYSDATE | 登录时间，默认为系统当前时间 |
| LoginIP | VARCHAR2(50) | | 登录的IP地址，用于安全监控 |
| LoginDevice | VARCHAR2(100) | | 登录设备信息，如手机型号、浏览器类型等 |
| LoginLocation | VARCHAR2(200) | | 登录地点，通过IP解析得到的地理位置 |
| LogoutTime | DATE | | 登出时间，用于计算会话时长 |

### 4. 员工管理模块

#### Staff（员工表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| StaffID | NUMBER(10) | PK | 员工的唯一标识符，使用序列自动生成 |
| StaffName | VARCHAR2(100) | NOT NULL | 员工姓名，不能为空 |
| Gender | CHAR(1) | CHECK | 性别，限定为M(男)/F(女) |
| Position | VARCHAR2(100) | | 职位，如服务员、厨师、经理等 |
| Phone | VARCHAR2(20) | | 联系电话，用于工作联系和排班通知 |
| Email | VARCHAR2(100) | | 电子邮箱，用于工作通知和文件发送 |
| HireDate | DATE | DEFAULT SYSDATE | 入职日期，默认为系统当前时间 |
| Salary | NUMBER(10,2) | | 基本薪资，保留两位小数 |
| DepartmentID | NUMBER(10) | FK | 关联部门表的外键，标识员工所属部门 |
| StoreID | NUMBER(10) | FK | 关联门店表的外键，标识员工所在门店 |
| Status | VARCHAR2(20) | CHECK | 员工状态，限定为：在职/离职/休假 |
| WorkSchedule | VARCHAR2(500) | | 工作时间安排，记录员工的排班信息 |
| CreateTime | DATE | DEFAULT SYSDATE | 记录创建时间，默认为系统当前时间 |
| UpdateTime | DATE | DEFAULT SYSDATE | 记录最后更新时间，默认为系统当前时间 |

#### Department（部门表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| DepartmentID | NUMBER(10) | PK | 部门的唯一标识符，使用序列自动生成 |
| DepartmentName | VARCHAR2(100) | NOT NULL, UNIQUE | 部门名称，如前厅部、厨房部等，不能为空且唯一 |
| Description | VARCHAR2(500) | | 部门职责描述 |
| ManagerID | NUMBER(10) | FK | 关联员工表的外键，标识部门负责人 |

#### Attendance（员工考勤表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| AttendanceID | NUMBER(10) | PK | 考勤记录的唯一标识符，使用序列自动生成 |
| StaffID | NUMBER(10) | FK | 关联员工表的外键，标识考勤所属的员工 |
| WorkDate | DATE | NOT NULL | 工作日期，不能为空 |
| CheckInTime | DATE | | 签到时间，记录员工上班打卡时间 |
| CheckOutTime | DATE | | 签退时间，记录员工下班打卡时间 |
| ActualWorkHours | NUMBER(5,2) | | 实际工作小时数，用于工时统计和薪资计算 |
| Status | VARCHAR2(20) | CHECK | 考勤状态，限定为：正常/迟到/早退/请假 |
| StoreID | NUMBER(10) | FK | 关联门店表的外键，标识考勤所在的门店 |

### 5. 库存管理模块

#### RawMaterial（原材料表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| MaterialID | NUMBER(10) | PK | 原材料的唯一标识符，使用序列自动生成 |
| MaterialName | VARCHAR2(200) | NOT NULL, UNIQUE | 原材料名称，如牛肉、大米等，不能为空且在同一门店内唯一 |
| CurrentStock | NUMBER(12,2) | DEFAULT 0 | 当前库存量，默认为0 |
| Unit | VARCHAR2(20) | | 计量单位，如千克、个、箱等 |
| UnitPrice | NUMBER(10,2) | | 单价，保留两位小数 |
| MinStock | NUMBER(12,2) | DEFAULT 0 | 最低库存预警线，低于此值时系统提醒采购，默认为0 |
| MaxStock | NUMBER(12,2) | | 最高库存线，用于控制过度采购 |
| SupplierID | NUMBER(10) | FK | 关联供应商表的外键，标识主要供应商 |
| LastInTime | DATE | | 最后入库时间，记录最近一次进货时间 |
| LastInQuantity | NUMBER(12,2) | | 最后入库数量，记录最近一次进货数量 |
| StaffID | NUMBER(10) | FK | 关联员工表的外键，标识负责此原材料的员工 |
| StoreID | NUMBER(10) | FK | 关联门店表的外键，标识原材料所属的门店 |
| Status | VARCHAR2(20) | CHECK | 状态，限定为：正常/停用/缺货 |
| StorageLocation | VARCHAR2(200) | | 存储位置，如仓库编号、冰箱位置等 |
| ExpiryDate | DATE | | 保质期截止日期，用于过期提醒 |

#### Supplier（供应商表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| SupplierID | NUMBER(10) | PK | 供应商的唯一标识符，使用序列自动生成 |
| SupplierName | VARCHAR2(200) | NOT NULL, UNIQUE | 供应商名称，不能为空且唯一 |
| Phone | VARCHAR2(20) | | 联系电话，用于采购联系 |
| Email | VARCHAR2(100) | | 电子邮箱，用于发送订单和询价 |
| ContactPerson | VARCHAR2(100) | | 联系人姓名，供应商的对接人员 |
| Address | VARCHAR2(500) | | 供应商地址，用于物流和送货 |
| MainProducts | VARCHAR2(1000) | | 主要产品描述，记录供应商能提供的主要原材料 |
| CooperationStartDate | DATE | DEFAULT SYSDATE | 合作开始日期，默认为系统当前时间 |
| CreditRating | VARCHAR2(20) | CHECK | 信用评级，限定为：优秀/良好/一般/较差 |
| Status | VARCHAR2(20) | CHECK | 合作状态，限定为：合作中/暂停/终止 |
| PaymentTerm | VARCHAR2(200) | | 付款条件，如月结30天、预付款等 |
| CreateTime | DATE | DEFAULT SYSDATE | 记录创建时间，默认为系统当前时间 |
| UpdateTime | DATE | DEFAULT SYSDATE | 记录最后更新时间，默认为系统当前时间 |

#### PurchaseRecord（采购记录表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| PurchaseID | NUMBER(10) | PK | 采购记录的唯一标识符，使用序列自动生成 |
| SupplierID | NUMBER(10) | FK | 关联供应商表的外键，标识采购的供应商 |
| PurchaseDate | DATE | DEFAULT SYSDATE | 采购日期，默认为系统当前时间 |
| TotalAmount | NUMBER(12,2) | DEFAULT 0 | 采购总金额，保留两位小数，默认为0 |
| StaffID | NUMBER(10) | FK | 关联员工表的外键，标识负责采购的员工 |
| StoreID | NUMBER(10) | FK | 关联门店表的外键，标识采购所属的门店 |
| Status | VARCHAR2(20) | CHECK | 采购状态，限定为：待收货/已收货/已付款 |
| Notes | VARCHAR2(1000) | | 采购备注，记录特殊要求或情况 |

#### PurchaseDetail（采购明细表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| PurchaseDetailID | NUMBER(10) | PK | 采购明细的唯一标识符，使用序列自动生成 |
| PurchaseID | NUMBER(10) | FK | 关联采购记录表的外键，标识所属的采购记录 |
| MaterialID | NUMBER(10) | FK | 关联原材料表的外键，标识采购的原材料 |
| Quantity | NUMBER(12,2) | NOT NULL | 采购数量，不能为空 |
| UnitPrice | NUMBER(10,2) | NOT NULL | 采购单价，保留两位小数，不能为空 |
| Subtotal | NUMBER(12,2) | NOT NULL | 小计金额（单价×数量），保留两位小数，不能为空 |
| ExpiryDate | DATE | | 原材料的保质期截止日期，用于库存管理 |

#### CorporateStandard（总部库存标准表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| StandardID | NUMBER(10) | PK | 标准的唯一标识符，使用序列自动生成 |
| MaterialID | NUMBER(10) | FK | 关联原材料表的外键，标识标准适用的原材料 |
| MinStockRatio | NUMBER(5,2) | | 最低库存比率，根据门店规模计算最低库存量 |
| MaxStockRatio | NUMBER(5,2) | | 最高库存比率，根据门店规模计算最高库存量 |
| StandardUnitPrice | NUMBER(10,2) | | 总部制定的标准采购单价，用于成本控制 |
| RecommendedSupplierID | NUMBER(10) | FK | 关联供应商表的外键，标识总部推荐的供应商 |
| CreateTime | DATE | DEFAULT SYSDATE | 记录创建时间，默认为系统当前时间 |
| UpdateTime | DATE | DEFAULT SYSDATE | 记录最后更新时间，默认为系统当前时间 |

### 6. 连锁管理模块

#### Store（门店表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| StoreID | NUMBER(10) | PK | 门店的唯一标识符，使用序列自动生成 |
| StoreName | VARCHAR2(200) | NOT NULL, UNIQUE | 门店名称，不能为空且唯一 |
| Address | VARCHAR2(500) | | 门店详细地址，用于导航和配送 |
| Phone | VARCHAR2(20) | | 门店联系电话，用于客户咨询和预订 |
| ManagerID | NUMBER(10) | FK | 关联员工表的外键，标识门店的店长 |
| OpeningHours | VARCHAR2(200) | | 营业时间，如"10:00-22:00" |
| Status | VARCHAR2(20) | CHECK | 门店状态，限定为：营业中/装修中/已关闭 |
| OpenDate | DATE | | 开业日期，记录门店正式营业的时间 |
| RegionID | NUMBER(10) | FK | 关联区域表的外键，标识门店所属的区域 |
| StoreSize | NUMBER(10,2) | | 门店面积(平方米)，用于规模评估和资源分配 |
| MonthlyRent | NUMBER(12,2) | | 月租金，用于成本核算 |
| CreateTime | DATE | DEFAULT SYSDATE | 记录创建时间，默认为系统当前时间 |
| UpdateTime | DATE | DEFAULT SYSDATE | 记录最后更新时间，默认为系统当前时间 |

#### Region（区域表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| RegionID | NUMBER(10) | PK | 区域的唯一标识符，使用序列自动生成 |
| RegionName | VARCHAR2(100) | NOT NULL, UNIQUE | 区域名称，如华东区、华南区等，不能为空且唯一 |
| RegionalManagerID | NUMBER(10) | FK | 关联员工表的外键，标识区域的负责人 |
| Description | VARCHAR2(500) | | 区域描述，包括覆盖范围、特点等 |
| CreateTime | DATE | DEFAULT SYSDATE | 记录创建时间，默认为系统当前时间 |
| UpdateTime | DATE | DEFAULT SYSDATE | 记录最后更新时间，默认为系统当前时间 |

#### ChainPerformance（连锁业绩统计表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| PerformanceID | NUMBER(10) | PK | 业绩记录的唯一标识符，使用序列自动生成 |
| StoreID | NUMBER(10) | FK | 关联门店表的外键，标识业绩所属的门店 |
| StatDate | DATE | | 统计日期，记录业绩的时间点 |
| TotalSales | NUMBER(12,2) | | 总销售额，当日或当期的销售总金额 |
| TotalOrders | NUMBER(10) | | 总订单数，当日或当期的订单总数量 |
| AverageOrderValue | NUMBER(10,2) | | 平均订单金额，总销售额除以总订单数 |
| CustomerCount | NUMBER(10) | | 客户数量，当日或当期的客户总数 |
| NewCustomerCount | NUMBER(10) | | 新客户数量，当日或当期新增的客户数 |
| TopSellingDish | NUMBER(10) | FK | 关联菜品表的外键，标识销售最好的菜品 |
| TopSellingQuantity | NUMBER(10) | | 销售最好的菜品的销售数量 |
| GrossProfitMargin | NUMBER(5,2) | | 毛利率，(销售额-成本)/销售额×100% |
| CreateTime | DATE | DEFAULT SYSDATE | 记录创建时间，默认为系统当前时间 |

#### StoreTransfer（门店间调货记录表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| TransferID | NUMBER(10) | PK | 调货记录的唯一标识符，使用序列自动生成 |
| FromStoreID | NUMBER(10) | FK | 关联门店表的外键，标识调出原材料的门店 |
| ToStoreID | NUMBER(10) | FK | 关联门店表的外键，标识接收原材料的门店 |
| TransferDate | DATE | DEFAULT SYSDATE | 调货日期，默认为系统当前时间 |
| Status | VARCHAR2(20) | CHECK | 调货状态，限定为：待处理/运输中/已完成/已取消 |
| ApproverID | NUMBER(10) | FK | 关联员工表的外键，标识批准调货的管理人员 |
| Notes | VARCHAR2(500) | | 调货备注，记录调货原因或特殊情况 |
| CreateTime | DATE | DEFAULT SYSDATE | 记录创建时间，默认为系统当前时间 |
| UpdateTime | DATE | DEFAULT SYSDATE | 记录最后更新时间，默认为系统当前时间 |

#### TransferDetail（调货明细表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| DetailID | NUMBER(10) | PK | 调货明细的唯一标识符，使用序列自动生成 |
| TransferID | NUMBER(10) | FK | 关联门店间调货记录表的外键，标识所属的调货记录 |
| MaterialID | NUMBER(10) | FK | 关联原材料表的外键，标识被调货的原材料 |
| Quantity | NUMBER(12,2) | | 调货数量，记录原材料的调拨量 |
| UnitPrice | NUMBER(10,2) | | 单价，用于内部结算，保留两位小数 |
| Subtotal | NUMBER(12,2) | | 小计金额（单价×数量），用于内部结算，保留两位小数 |
| Notes | VARCHAR2(500) | | 明细备注，记录特定原材料的调货说明 |

## 系统关系图

```
                                   +-------------+
                                   | Region      |
                                   +-------------+
                                         |
                                         |
+-------------+      +------------+      +------------+      +----------------+
| Department  |<-----| Staff      |<---->| Store      |----->| ChainPerformance|
+-------------+      +------------+      +------------+      +----------------+
                          |                    |
                          |                    |
+-------------+      +------------+      +------------+      +----------------+
| Attendance  |<-----| Staff      |      | Store      |----->| StoreTransfer  |
+-------------+      +------------+      +------------+      +----------------+
                          |                    |                    |
                          |                    |                    |
+-------------+      +------------+      +------------+      +----------------+
| VIPLevel    |<-----| Customer   |----->| TableInfo   |      | TransferDetail |
+-------------+      +------------+      +------------+      +----------------+
                          |                    |
                          |                    |
+-------------+      +------------+      +------------+      +----------------+
| PointsRecord|<-----| Customer   |      | TableInfo   |---->| TableReservation|
+-------------+      +------------+      +------------+      +----------------+
                          |                                        |
                          |                                        |
+----------------+   +------------+      +------------+      +----------------+
|CustomerLoginLog|<--|Customer    |<-----|Orders      |----->|OrderDetail     |
+----------------+   +------------+      +------------+      +----------------+
                                               |                    |
                                               |                    |
                                         +------------+      +----------------+
                                         | Dish       |<-----|OrderDetail     |
                                         +------------+      +----------------+
                                               |
                                               |
+----------------+   +------------+      +------------+      +----------------+
| Category       |<--|CorporateDish|----->|StoreDishMapping|--|Dish           |
+----------------+   +------------+      +------------+      +----------------+
                                               |
                                               |
+----------------+   +------------+      +------------+      +----------------+
| Promotion      |<--|DishPromotion|---->| Dish       |----->|Recipe          |
+----------------+   +------------+      +------------+      +----------------+
                                               |                    |
                                               |                    |
                                         +------------+      +----------------+
                                         |RawMaterial |<-----|Recipe          |
                                         +------------+      +----------------+
                                               |
                                               |
+----------------+   +------------+      +------------+      +----------------+
|CorporateStandard|--|RawMaterial |<-----|Supplier    |----->|PurchaseRecord  |
+----------------+   +------------+      +------------+      +----------------+
                                                                    |
                                                                    |
                                                              +----------------+
                                                              |PurchaseDetail  |
                                                              +----------------+
```

## 序列设计

系统为每个表设计了对应的自增序列，用于生成主键值：

| 序列名 | 起始值 | 步长 | 用途 |
|--------|--------|------|------|
| seq_order_id | 1 | 1 | Orders表主键 |
| seq_order_detail_id | 1 | 1 | OrderDetail表主键 |
| seq_table_id | 1 | 1 | TableInfo表主键 |
| seq_reservation_id | 1 | 1 | TableReservation表主键 |
| seq_dish_id | 1 | 1 | Dish表主键 |
| seq_category_id | 1 | 1 | Category表主键 |
| seq_recipe_id | 1 | 1 | Recipe表主键 |
| seq_promotion_id | 1 | 1 | Promotion表主键 |
| seq_dish_promotion_id | 1 | 1 | DishPromotion表主键 |
| seq_customer_id | 1 | 1 | Customer表主键 |
| seq_level_id | 1 | 1 | VIPLevel表主键 |
| seq_points_record_id | 1 | 1 | PointsRecord表主键 |
| seq_login_log_id | 1 | 1 | CustomerLoginLog表主键 |
| seq_staff_id | 1 | 1 | Staff表主键 |
| seq_department_id | 1 | 1 | Department表主键 |
| seq_attendance_id | 1 | 1 | Attendance表主键 |
| seq_material_id | 1 | 1 | RawMaterial表主键 |
| seq_supplier_id | 1 | 1 | Supplier表主键 |
| seq_purchase_id | 1 | 1 | PurchaseRecord表主键 |
| seq_purchase_detail_id | 1 | 1 | PurchaseDetail表主键 |
| seq_store_id | 1 | 1 | Store表主键 |
| seq_region_id | 1 | 1 | Region表主键 |
| seq_corporate_dish_id | 1 | 1 | CorporateDish表主键 |
| seq_store_dish_mapping_id | 1 | 1 | StoreDishMapping表主键 |
| seq_corporate_standard_id | 1 | 1 | CorporateStandard表主键 |
| seq_chain_performance_id | 1 | 1 | ChainPerformance表主键 |
| seq_store_transfer_id | 1 | 1 | StoreTransfer表主键 |
| seq_transfer_detail_id | 1 | 1 | TransferDetail表主键 |

## 索引设计

系统针对常用查询字段设计了以下索引：

| 索引名 | 表名 | 字段 | 用途 |
|--------|------|------|------|
| idx_orders_time | Orders | OrderTime | 加速按时间查询订单 |
| idx_orders_customer | Orders | CustomerID | 加速查询客户的订单 |
| idx_orders_store | Orders | StoreID | 加速查询门店的订单 |
| idx_orders_status | Orders | OrderStatus | 加速按状态筛选订单 |
| idx_orderdetail_order | OrderDetail | OrderID | 加速查询订单的详情 |
| idx_orderdetail_dish | OrderDetail | DishID | 加速查询菜品的销售情况 |
| idx_customer_phone | Customer | Phone | 加速通过电话查找客户 |
| idx_customer_level | Customer | VIPLevel | 加速按会员等级筛选客户 |
| idx_customer_store | Customer | PreferredStoreID | 加速查询门店的常客 |
| idx_staff_department | Staff | DepartmentID | 加速查询部门的员工 |
| idx_staff_store | Staff | StoreID | 加速查询门店的员工 |
| idx_staff_position | Staff | Position | 加速按职位筛选员工 |
| idx_material_supplier | RawMaterial | SupplierID | 加速查询供应商的原材料 |
| idx_material_store | RawMaterial | StoreID | 加速查询门店的库存 |
| idx_material_status | RawMaterial | Status | 加速按状态筛选原材料 |
| idx_recipe_dish | Recipe | DishID | 加速查询菜品的配方 |
| idx_recipe_material | Recipe | MaterialID | 加速查询原材料的使用情况 |
| idx_reservation_table | TableReservation | TableID | 加速查询桌台的预约 |
| idx_reservation_time | TableReservation | ReservationTime | 加速按时间查询预约 |
| idx_reservation_status | TableReservation | Status | 加速按状态筛选预约 |
| idx_store_region | Store | RegionID | 加速查询区域的门店 |
| idx_store_status | Store | Status | 加速按状态筛选门店 |
| idx_store_open_date | Store | OpenDate | 加速按开业日期查询门店 |
| idx_performance_store | ChainPerformance | StoreID | 加速查询门店的业绩 |
| idx_performance_date | ChainPerformance | StatDate | 加速按日期查询业绩 |
| idx_performance_sales | ChainPerformance | TotalSales | 加速按销售额排序业绩 |
| idx_transfer_from | StoreTransfer | FromStoreID | 加速查询门店的调出记录 |
| idx_transfer_to | StoreTransfer | ToStoreID | 加速查询门店的调入记录 |
| idx_transfer_status | StoreTransfer | Status | 加速按状态筛选调货记录 |
| idx_dish_category | Dish | CategoryID | 加速查询分类的菜品 |
| idx_dish_available | Dish | IsAvailable | 加速筛选可售菜品 |
| idx_table_store | TableInfo | StoreID | 加速查询门店的桌台 |
| idx_table_status | TableInfo | Status | 加速按状态筛选桌台 |
| idx_attendance_staff | Attendance | StaffID | 加速查询员工的考勤 |
| idx_attendance_date | Attendance | WorkDate | 加速按日期查询考勤 |
| idx_attendance_store | Attendance | StoreID | 加速查询门店的考勤 |
| idx_points_customer | PointsRecord | CustomerID | 加速查询客户的积分记录 |
| idx_points_order | PointsRecord | OrderID | 加速查询订单的积分记录 |
| idx_points_type | PointsRecord | RecordType | 加速按类型筛选积分记录 |
| idx_purchase_supplier | PurchaseRecord | SupplierID | 加速查询供应商的采购记录 |
| idx_purchase_store | PurchaseRecord | StoreID | 加速查询门店的采购记录 |
| idx_purchase_date | PurchaseRecord | PurchaseDate | 加速按日期查询采购记录 |
| idx_purchase_status | PurchaseRecord | Status | 加速按状态筛选采购记录 |
| idx_mapping_store | StoreDishMapping | StoreID | 加速查询门店的菜单映射 |
| idx_mapping_corporate | StoreDishMapping | CorporateDishID | 加速查询总部菜品的门店映射 |
| idx_mapping_local | StoreDishMapping | LocalDishID | 加速查询本地菜品的总部映射 |

## 系统功能模块

1. **订单管理**：处理点餐、结账和订单跟踪，支持多种点餐方式和支付方式
2. **菜品管理**：维护菜单、菜品分类和价格，支持菜品上下架和促销设置
3. **客户管理**：会员服务、积分管理和客户关系维护，支持客户画像和精准营销
4. **员工管理**：人力资源、考勤和绩效管理，支持排班和薪资核算
5. **库存管理**：原材料库存、采购和供应商管理，支持库存预警和自动采购
6. **桌台管理**：餐厅桌台状态和预订管理，支持桌台分区和动态调整
7. **促销管理**：折扣活动和营销策略，支持多种促销方式和会员特权
8. **成本核算**：菜品成本和利润分析，支持成本控制和定价策略
9. **连锁管理**：门店管理、区域管理和业绩分析，支持连锁扩张和品牌统一
10. **标准化管理**：菜单标准化和库存标准化，确保连锁店品质一致
11. **调货管理**：门店间物料调拨和资源共享，优化集团资源配置

## 使用指南

1. **系统初始化**：执行建表脚本创建数据库结构，并导入基础数据
2. **用户权限**：根据角色分配适当的表访问权限
3. **数据维护**：定期维护基础数据，如菜品、供应商、会员等级等
4. **数据备份**：按计划执行数据备份，确保数据安全
5. **性能优化**：定期检查索引使用情况，优化查询性能
6. **系统升级**：根据业务需求，适时调整表结构和功能

