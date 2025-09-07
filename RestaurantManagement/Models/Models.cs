using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantManagement.Models
{
    // 更新状态请求模型
    public class UpdateStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    // 打卡请求模型
    public class CheckInRequest
    {
        public int StaffId { get; set; }
    }

    // 开台请求模型
    public class OpenTableRequest
    {
        public int? StaffId { get; set; }
        public string? Notes { get; set; }
    }

    // 桌台信息
    [Table("TABLEINFO")]
    public class TableInfo
    {
        [Column("TABLEID")]
        public int TableID { get; set; }
        
        [Column("TABLENUMBER")]
        public string TableNumber { get; set; } = string.Empty;
        
        [Column("CAPACITY")]
        public int Capacity { get; set; }
        
        [Column("AREA")]
        public string Area { get; set; } = string.Empty;
        
        [Column("STATUS")]
        public string Status { get; set; } = string.Empty; // 空闲、占用、预订、清洁中
        
        [Column("LASTCLEANTIME")]
        public DateTime? LastCleanTime { get; set; }
    }

    // 订单
    public class Order
    {
        public int OrderID { get; set; }
        public int? TableID { get; set; }
        public int? CustomerID { get; set; }
        public int? StaffID { get; set; }
        public DateTime OrderTime { get; set; }
        public decimal? TotalPrice { get; set; }
        public string OrderStatus { get; set; } = string.Empty; // 待处理、制作中、已完成、已结账、已取消
        public int? StoreID { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string? Notes { get; set; }
        
        // 兼容性属性（前端可能使用）
        public decimal TotalAmount 
        { 
            get => TotalPrice ?? 0; 
            set => TotalPrice = value; 
        }
    }

    // 订单详情
    public class OrderDetail
    {
        public int OrderDetailID { get; set; }
        public int OrderID { get; set; }
        public int DishID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        public string? SpecialRequests { get; set; }
    }

    // 订单详情DTO（包含菜品信息）
    public class OrderDetailDto
    {
        public int OrderDetailID { get; set; }
        public int OrderID { get; set; }
        public int DishID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        public string? SpecialRequests { get; set; }
        public string DishName { get; set; } = string.Empty;
        public string? DishDescription { get; set; }
        public string? DishImageURL { get; set; }
        public string? CategoryName { get; set; }
        public int? CategoryID { get; set; }
    }

    // 菜品
    public class Dish
    {
        public int DishID { get; set; }
        public string DishName { get; set; } = string.Empty;
        public int CategoryID { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public string? ImageURL { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreateTime { get; set; }
    }

    // 菜品分类
    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int SortOrder { get; set; }
    }

    // 员工
    [Table("STAFF")]
    public class Staff
    {
        [Column("STAFFID")]
        public int StaffID { get; set; }
        
        [Column("STAFFNAME")]
        public string StaffName { get; set; } = string.Empty;
        
        [Column("GENDER")]
        public string Gender { get; set; } = string.Empty; // "M" 或 "F"
        
        [Column("POSITION")]
        public string? Position { get; set; }
        
        [Column("PHONE")]
        public string? Phone { get; set; }
        
        [Column("EMAIL")]
        public string? Email { get; set; }
        
        [Column("HIREDATE")]
        public DateTime HireDate { get; set; }
        
        [Column("SALARY")]
        public decimal? Salary { get; set; }
        
        [Column("DEPARTMENTID")]
        public int DepartmentID { get; set; }
        
        [Column("STOREID")]
        public int StoreID { get; set; }
        
        [Column("STATUS")]
        public string Status { get; set; } = string.Empty; // "在职"/"离职"/"休假"
        
        [Column("CREATETIME")]
        public DateTime CreateTime { get; set; }
        
        [Column("UPDATETIME")]
        public DateTime UpdateTime { get; set; }
    }

    // 考勤记录
    public class Attendance
    {
        public int AttendanceID { get; set; }
        public int StaffID { get; set; }
        public DateTime WorkDate { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public decimal? ActualWorkHours { get; set; }
        public string Status { get; set; } = string.Empty; // 正常、迟到、早退、缺勤
        public int? StoreID { get; set; }
        
        // 兼容性属性
        public decimal? WorkHours 
        { 
            get => ActualWorkHours; 
            set => ActualWorkHours = value; 
        }
    }

    // 原材料
    public class RawMaterial
    {
        public int MaterialID { get; set; }
        public string MaterialName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal CurrentStock { get; set; }
        public decimal MinStock { get; set; }
        public decimal MaxStock { get; set; }
        public decimal UnitPrice { get; set; }
        public int? SupplierID { get; set; }
        public int? StaffID { get; set; }
        public int? StoreID { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? StorageLocation { get; set; }
        public DateTime? LastInTime { get; set; }
        public decimal? LastInQuantity { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    // 供应商
    public class Supplier
    {
        public int SupplierID { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Email { get; set; }
    }

    // 采购记录
    public class PurchaseRecord
    {
        public int PurchaseID { get; set; }
        public int SupplierID { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty; // 待入库、已入库、已取消
        public string? Notes { get; set; }
    }

    // 菜谱（菜品与原材料关系）
    public class Recipe
    {
        public int RecipeID { get; set; }
        public int DishID { get; set; }
        public int MaterialID { get; set; }
        public decimal RequiredQuantity { get; set; }
    }

    // 排班记录
    [Table("STAFFSCHEDULE")]
    public class Schedule
    {
        [Column("SCHEDULEID")]
        public int ScheduleID { get; set; }

        [Column("STAFFID")]
        public int StaffID { get; set; }

        [Column("WORKDATE")]
        public int WorkDate { get; set; } // 1-7，表示星期几

        [Column("STARTTIME")]
        public TimeSpan StartTime { get; set; } // INTERVAL DAY TO SECOND

        [Column("ENDTIME")]
        public TimeSpan EndTime { get; set; } // INTERVAL DAY TO SECOND

        [Column("STOREID")]
        public int? StoreID { get; set; }

        [Column("NOTES")]
        public string? Notes { get; set; }
    }
}
