namespace ConsoleApp1.Models
{
    /// <summary>
    /// 员工表实体类
    /// </summary>
    public class Staff
    {
        public int StaffID { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public char? Gender { get; set; }
        public string? Position { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateTime HireDate { get; set; }
        public decimal? Salary { get; set; }
        public int? DepartmentID { get; set; }
        public int? StoreID { get; set; }
        public string Status { get; set; } = "在职";
        public string? WorkSchedule { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}