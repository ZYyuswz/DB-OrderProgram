# DB-OrderProgram
同济大学数据库小学期项目
# 餐饮管理系统数据库设计文档

## 系统概述

本文档详细介绍餐饮管理系统的数据库设计，包括表结构、关键字段、主外键关系以及各表的业务功能。系统共包含19张表，涵盖订单管理、菜品管理、客户管理、员工管理、库存管理和财务管理等核心业务模块。

## 数据库表结构

### 1. 订单管理模块

#### Orders（订单主表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| OrderID | NUMBER(10) | PK | 订单ID |
| OrderTime | DATE | DEFAULT SYSDATE | 下单时间 |
| TableID | NUMBER(10) | FK | 关联桌台 |
| CustomerID | NUMBER(10) | FK | 关联客户 |
| TotalPrice | NUMBER(12,2) | DEFAULT 0 | 订单总价 |
| OrderStatus | VARCHAR2(20) | CHECK | 订单状态：待处理/制作中/已完成/已结账 |
| StaffID | NUMBER(10) | FK | 服务员ID |
| CreateTime | DATE | DEFAULT SYSDATE | 创建时间 |
| UpdateTime | DATE | DEFAULT SYSDATE | 更新时间 |

**功能**：记录客户订单的主要信息，包括下单时间、服务桌台、客户信息、订单状态等。作为订单详情表的主表，提供订单跟踪和管理功能。

#### OrderDetail（订单详情表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| OrderDetailID | NUMBER(10) | PK | 订单详情ID |
| OrderID | NUMBER(10) | FK | 关联订单 |
| DishID | NUMBER(10) | FK | 关联菜品 |
| Quantity | NUMBER(5) | NOT NULL | 数量 |
| UnitPrice | NUMBER(10,2) | NOT NULL | 单价 |
| Subtotal | NUMBER(12,2) | NOT NULL | 小计 |
| SpecialRequests | VARCHAR2(500) | | 特殊要求 |

**功能**：记录订单中的具体菜品信息，解决订单与菜品的多对多关系。保存下单时的价格，避免菜品价格变动影响历史订单。

#### TableInfo（桌台信息表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| TableID | NUMBER(10) | PK | 桌台ID |
| TableNumber | VARCHAR2(20) | NOT NULL, UNIQUE | 桌号 |
| Area | VARCHAR2(100) | | 区域 |
| Capacity | NUMBER(3) | | 容量 |
| Status | VARCHAR2(20) | CHECK | 状态：空闲/占用/清洁中/维修中 |
| IsReserved | CHAR(1) | CHECK | 是否预订：Y/N |
| ReservationTime | DATE | | 预订时间 |
| ReservationCustomer | VARCHAR2(100) | | 预订客户 |

**功能**：管理餐厅桌台资源，跟踪桌台状态和预订情况，支持桌台分区管理和预订功能。

### 2. 菜品管理模块

#### Dish（菜品表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| DishID | NUMBER(10) | PK | 菜品ID |
| DishName | VARCHAR2(200) | NOT NULL, UNIQUE | 菜品名称 |
| Price | NUMBER(10,2) | NOT NULL | 价格 |
| CategoryID | NUMBER(10) | FK | 分类ID |
| IsAvailable | CHAR(1) | CHECK | 是否可售：Y/N |
| Description | VARCHAR2(1000) | | 描述 |
| ImageURL | VARCHAR2(500) | | 图片链接 |
| EstimatedTime | NUMBER(5) | | 预计制作时间(分钟) |
| CreateTime | DATE | DEFAULT SYSDATE | 创建时间 |
| UpdateTime | DATE | DEFAULT SYSDATE | 更新时间 |

**功能**：存储菜品的基本信息，包括名称、价格、分类、状态等，是系统中的核心表之一。

#### Category（菜品分类表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| CategoryID | NUMBER(10) | PK | 分类ID |
| CategoryName | VARCHAR2(100) | NOT NULL, UNIQUE | 分类名称 |
| SortOrder | NUMBER(5) | DEFAULT 0 | 排序号 |
| IsActive | CHAR(1) | CHECK | 是否启用：Y/N |

**功能**：管理菜品分类，便于菜品的组织和展示，支持菜单的分类浏览。

#### Recipe（菜品配方表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| RecipeID | NUMBER(10) | PK | 配方ID |
| DishID | NUMBER(10) | FK | 菜品ID |
| MaterialID | NUMBER(10) | FK | 原材料ID |
| RequiredQuantity | NUMBER(12,2) | NOT NULL | 需要数量 |
| Unit | VARCHAR2(20) | | 单位 |
| CostPerUnit | NUMBER(10,2) | | 单位成本 |
| Notes | VARCHAR2(500) | | 备注 |

**功能**：记录每道菜品的原材料配方，解决菜品与原材料的多对多关系，支持成本核算和库存管理。

#### Promotion（促销活动表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| PromotionID | NUMBER(10) | PK | 促销ID |
| PromotionName | VARCHAR2(200) | NOT NULL | 促销名称 |
| DiscountType | VARCHAR2(20) | CHECK | 折扣类型：百分比/固定金额 |
| DiscountValue | NUMBER(10,2) | | 折扣值 |
| BeginTime | DATE | | 开始时间 |
| EndTime | DATE | | 结束时间 |
| IsActive | CHAR(1) | CHECK | 是否激活：Y/N |
| ApplicableCategories | VARCHAR2(500) | | 适用分类 |
| MinOrderAmount | NUMBER(10,2) | DEFAULT 0 | 最小订单金额 |

**功能**：管理促销活动，支持不同类型的折扣策略，可限定适用范围和时间。

#### DishPromotion（菜品促销关联表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| DishPromotionID | NUMBER(10) | PK | 关联ID |
| DishID | NUMBER(10) | FK | 菜品ID |
| PromotionID | NUMBER(10) | FK | 促销ID |

**功能**：解决菜品与促销活动的多对多关系，支持针对特定菜品的促销活动管理。

### 3. 客户管理模块

#### Customer（客户表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| CustomerID | NUMBER(10) | PK | 客户ID |
| CustomerName | VARCHAR2(100) | NOT NULL | 客户名称 |
| Phone | VARCHAR2(20) | | 电话 |
| Email | VARCHAR2(100) | | 邮箱 |
| Birthday | DATE | | 生日 |
| Gender | CHAR(1) | CHECK | 性别：M/F |
| RegisterTime | DATE | DEFAULT SYSDATE | 注册时间 |
| LastVisitTime | DATE | | 最后访问时间 |
| TotalConsumption | NUMBER(12,2) | DEFAULT 0 | 累计消费 |
| VIPLevel | NUMBER(10) | FK | 会员等级 |
| VIPPoints | NUMBER(10) | DEFAULT 0 | 会员积分 |
| Status | VARCHAR2(20) | CHECK | 状态：正常/黑名单 |

**功能**：管理客户信息，包括基本资料、消费记录、会员等级和积分等，支持客户关系管理。

#### VIPLevel（会员等级表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| LevelID | NUMBER(10) | PK | 等级ID |
| LevelName | VARCHAR2(50) | NOT NULL, UNIQUE | 等级名称 |
| MinConsumption | NUMBER(10,2) | DEFAULT 0 | 最低消费要求 |
| DiscountRate | NUMBER(5,2) | DEFAULT 100 | 折扣率 |
| PointsRate | NUMBER(5,2) | DEFAULT 1 | 积分倍率 |
| Benefits | VARCHAR2(1000) | | 权益描述 |

**功能**：定义会员等级体系，包括各等级的消费要求、折扣和积分政策，支持会员等级管理。

#### PointsRecord（积分记录表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| RecordID | NUMBER(10) | PK | 记录ID |
| CustomerID | NUMBER(10) | FK | 客户ID |
| OrderID | NUMBER(10) | FK | 订单ID |
| PointsChange | NUMBER(10) | | 积分变化 |
| RecordType | VARCHAR2(20) | CHECK | 记录类型：消费获得/兑换消费/过期扣除 |
| RecordTime | DATE | DEFAULT SYSDATE | 记录时间 |
| Description | VARCHAR2(500) | | 描述 |

**功能**：记录客户积分的变动情况，包括获取、消费和过期，支持积分管理和积分历史追踪。

#### CustomerLoginLog（客户登录日志表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| LogID | NUMBER(10) | PK | 日志ID |
| CustomerID | NUMBER(10) | FK | 客户ID |
| LoginTime | DATE | DEFAULT SYSDATE | 登录时间 |
| LoginIP | VARCHAR2(50) | | 登录IP |
| LoginDevice | VARCHAR2(100) | | 登录设备 |
| LoginLocation | VARCHAR2(200) | | 登录地点 |
| LogoutTime | DATE | | 登出时间 |

**功能**：记录客户登录系统的情况，包括时间、设备和位置等，支持安全监控和用户行为分析。

### 4. 员工管理模块

#### Staff（员工表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| StaffID | NUMBER(10) | PK | 员工ID |
| StaffName | VARCHAR2(100) | NOT NULL | 员工姓名 |
| Gender | CHAR(1) | CHECK | 性别：M/F |
| Position | VARCHAR2(100) | | 职位 |
| Phone | VARCHAR2(20) | | 电话 |
| Email | VARCHAR2(100) | | 邮箱 |
| HireDate | DATE | DEFAULT SYSDATE | 入职日期 |
| Salary | NUMBER(10,2) | | 薪资 |
| DepartmentID | NUMBER(10) | FK | 部门ID |
| Status | VARCHAR2(20) | CHECK | 状态：在职/离职/休假 |
| WorkSchedule | VARCHAR2(500) | | 工作时间安排 |
| CreateTime | DATE | DEFAULT SYSDATE | 创建时间 |
| UpdateTime | DATE | DEFAULT SYSDATE | 更新时间 |

**功能**：管理员工基本信息，包括个人资料、职位、薪资和状态等，支持人力资源管理。

#### Department（部门表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| DepartmentID | NUMBER(10) | PK | 部门ID |
| DepartmentName | VARCHAR2(100) | NOT NULL, UNIQUE | 部门名称 |
| Description | VARCHAR2(500) | | 描述 |
| ManagerID | NUMBER(10) | FK | 部门经理ID |

**功能**：管理餐厅部门结构，支持部门划分和管理层次设置。

#### Attendance（员工考勤表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| AttendanceID | NUMBER(10) | PK | 考勤ID |
| StaffID | NUMBER(10) | FK | 员工ID |
| WorkDate | DATE | NOT NULL | 工作日期 |
| CheckInTime | DATE | | 签到时间 |
| CheckOutTime | DATE | | 签退时间 |
| ActualWorkHours | NUMBER(5,2) | | 实际工作小时 |
| Status | VARCHAR2(20) | CHECK | 状态：正常/迟到/早退/请假 |

**功能**：记录员工的出勤情况，包括签到签退时间和工作时长，支持考勤管理和绩效考核。

### 5. 库存管理模块

#### RawMaterial（原材料表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| MaterialID | NUMBER(10) | PK | 原材料ID |
| MaterialName | VARCHAR2(200) | NOT NULL, UNIQUE | 原材料名称 |
| CurrentStock | NUMBER(12,2) | DEFAULT 0 | 当前库存 |
| Unit | VARCHAR2(20) | | 单位 |
| UnitPrice | NUMBER(10,2) | | 单价 |
| MinStock | NUMBER(12,2) | DEFAULT 0 | 最低库存预警线 |
| MaxStock | NUMBER(12,2) | | 最高库存线 |
| SupplierID | NUMBER(10) | FK | 供应商ID |
| LastInTime | DATE | | 最后入库时间 |
| LastInQuantity | NUMBER(12,2) | | 最后入库数量 |
| StaffID | NUMBER(10) | FK | 负责人ID |
| Status | VARCHAR2(20) | CHECK | 状态：正常/停用/缺货 |
| StorageLocation | VARCHAR2(200) | | 存储位置 |
| ExpiryDate | DATE | | 保质期 |

**功能**：管理原材料库存，包括库存量、价格、供应商和保质期等信息，支持库存预警和成本核算。

#### Supplier（供应商表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| SupplierID | NUMBER(10) | PK | 供应商ID |
| SupplierName | VARCHAR2(200) | NOT NULL, UNIQUE | 供应商名称 |
| Phone | VARCHAR2(20) | | 电话 |
| Email | VARCHAR2(100) | | 邮箱 |
| ContactPerson | VARCHAR2(100) | | 联系人 |
| Address | VARCHAR2(500) | | 地址 |
| MainProducts | VARCHAR2(1000) | | 主要产品 |
| CooperationStartDate | DATE | DEFAULT SYSDATE | 合作开始日期 |
| CreditRating | VARCHAR2(20) | CHECK | 信用评级：优秀/良好/一般/较差 |
| Status | VARCHAR2(20) | CHECK | 状态：合作中/暂停/终止 |
| PaymentTerm | VARCHAR2(200) | | 付款条件 |
| CreateTime | DATE | DEFAULT SYSDATE | 创建时间 |
| UpdateTime | DATE | DEFAULT SYSDATE | 更新时间 |

**功能**：管理供应商信息，包括联系方式、产品和合作状态等，支持供应商评估和选择。

#### PurchaseRecord（采购记录表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| PurchaseID | NUMBER(10) | PK | 采购ID |
| SupplierID | NUMBER(10) | FK | 供应商ID |
| PurchaseDate | DATE | DEFAULT SYSDATE | 采购日期 |
| TotalAmount | NUMBER(12,2) | DEFAULT 0 | 总金额 |
| StaffID | NUMBER(10) | FK | 采购员ID |
| Status | VARCHAR2(20) | CHECK | 状态：待收货/已收货/已付款 |
| Notes | VARCHAR2(1000) | | 备注 |

**功能**：记录采购订单的基本信息，包括供应商、金额和状态等，作为采购明细的主表。

#### PurchaseDetail（采购明细表）
| 字段名 | 类型 | 约束 | 描述 |
|--------|------|------|------|
| PurchaseDetailID | NUMBER(10) | PK | 明细ID |
| PurchaseID | NUMBER(10) | FK | 采购ID |
| MaterialID | NUMBER(10) | FK | 原材料ID |
| Quantity | NUMBER(12,2) | NOT NULL | 数量 |
| UnitPrice | NUMBER(10,2) | NOT NULL | 单价 |
| Subtotal | NUMBER(12,2) | NOT NULL | 小计 |
| ExpiryDate | DATE | | 保质期 |

**功能**：记录采购订单中的具体原材料信息，解决采购与原材料的多对多关系，支持入库和成本核算。

## 系统关系图

```
+-------------+      +------------+      +------------+
| Department  |<-----| Staff      |----->| Attendance |
+-------------+      +------------+      +------------+
                          |
                          |
+-------------+      +------------+      +------------+
| VIPLevel    |<-----| Customer   |----->| PointsRecord|
+-------------+      +------------+      +------------+
                          |                    |
                          |                    |
+-------------+      +------------+      +------------+
| TableInfo   |<-----| Orders     |<-----|OrderDetail |
+-------------+      +------------+      +------------+
                                               |
                                               |
+-------------+      +------------+      +------------+
| Category    |<-----| Dish       |<-----|Recipe      |
+-------------+      +------------+      +------------+
                          |                    |
                          |                    |
+-------------+      +------------+      +------------+
| Promotion   |<-----|DishPromotion|      |RawMaterial |
+-------------+      +------------+      +------------+
                                               |
                                               |
                                         +------------+
                                         | Supplier   |
                                         +------------+
                                               |
                                               |
                                         +------------+      +------------+
                                         |PurchaseRecord|--->|PurchaseDetail|
                                         +------------+      +------------+
```

## 系统功能模块

1. **订单管理**：处理点餐、结账和订单跟踪
2. **菜品管理**：维护菜单、菜品分类和价格
3. **客户管理**：会员服务、积分管理和客户关系维护
4. **员工管理**：人力资源、考勤和绩效管理
5. **库存管理**：原材料库存、采购和供应商管理
6. **桌台管理**：餐厅桌台状态和预订管理
7. **促销管理**：折扣活动和营销策略
8. **成本核算**：菜品成本和利润分析

## 索引设计

系统针对常用查询字段设计了以下索引：

1. 订单相关索引：订单时间、客户、桌台和状态
2. 订单详情索引：订单ID和菜品ID
3. 客户相关索引：电话和会员等级
4. 员工相关索引：部门和职位
5. 原材料相关索引：供应商和状态
6. 配方相关索引：菜品ID和原材料ID

## 总结

本餐饮管理系统数据库设计采用规范化的关系模型，通过合理的表结构设计和关系定义，解决了多对多关系和多值依赖问题。系统支持从点餐、结账到库存管理、成本核算的完整业务流程，适合中小型餐饮企业使用。
