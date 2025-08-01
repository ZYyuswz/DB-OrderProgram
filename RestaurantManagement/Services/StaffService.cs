using Dapper;
using RestaurantManagement.Models;

namespace RestaurantManagement.Services
{
    public class StaffService
    {
        private readonly DatabaseService _dbService;

        public StaffService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        // 获取所有员工
        public async Task<IEnumerable<Staff>> GetAllStaffAsync()
        {
            using var connection = _dbService.CreateConnection();
            var sql = "SELECT * FROM PUB.Staff ORDER BY StaffName";
            return await connection.QueryAsync<Staff>(sql);
        }

        // 根据ID获取员工
        public async Task<Staff?> GetStaffByIdAsync(int staffId)
        {
            using var connection = _dbService.CreateConnection();
            var sql = "SELECT * FROM PUB.Staff WHERE StaffID = :StaffId";
            return await connection.QueryFirstOrDefaultAsync<Staff>(sql, new { StaffId = staffId });
        }

        // 添加员工（去掉StaffID，确保外键有效）
        public async Task<bool> AddStaffAsync(Staff staff)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                INSERT INTO PUB.Staff 
                (StaffName, Gender, Position, Phone, Email, HireDate, Salary, DepartmentID, StoreID, Status, WorkSchedule, CreateTime, UpdateTime)
                VALUES
                (:StaffName, :Gender, :Position, :Phone, :Email, :HireDate, :Salary, :DepartmentID, :StoreID, :Status, :WorkSchedule, :CreateTime, :UpdateTime)";
            staff.CreateTime = DateTime.Now;
            staff.UpdateTime = DateTime.Now;

            // 外键有效性简单校验
            if (staff.DepartmentID == 0 || staff.StoreID == 0)
                throw new ArgumentException("DepartmentID 和 StoreID 必须有效且存在。");

            var result = await connection.ExecuteAsync(sql, staff);
            return result > 0;
        }

        // 更新员工信息（字段补全，增加外键有效性校验）
        public async Task<bool> UpdateStaffAsync(Staff staff)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                UPDATE PUB.Staff 
                SET StaffName = :StaffName, Gender = :Gender, Position = :Position, Phone = :Phone, 
                    Email = :Email, HireDate = :HireDate, Salary = :Salary, DepartmentID = :DepartmentID, 
                    StoreID = :StoreID, Status = :Status, WorkSchedule = :WorkSchedule, UpdateTime = :UpdateTime
                WHERE StaffID = :StaffID";
            staff.UpdateTime = DateTime.Now;

            if (staff.DepartmentID == 0 || staff.StoreID == 0)
                throw new ArgumentException("DepartmentID 和 StoreID 必须有效且存在。");

            var result = await connection.ExecuteAsync(sql, staff);
            return result > 0;
        }

        // 删除员工
        public async Task<bool> DeleteStaffAsync(int staffId)
        {
            using var connection = _dbService.CreateConnection();
            var sql = "DELETE FROM PUB.Staff WHERE StaffID = :StaffId";
            var result = await connection.ExecuteAsync(sql, new { StaffId = staffId });
            return result > 0;
        }

        // 按条件搜索员工
        public async Task<IEnumerable<Staff>> SearchStaffAsync(string? name = null, int? departmentId = null, string? position = null)
        {
            using var connection = _dbService.CreateConnection();
            var sql = "SELECT * FROM PUB.Staff WHERE 1=1";
            var parameters = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(name))
            {
                sql += " AND StaffName LIKE :Name";
                parameters.Add("Name", $"%{name}%");
            }
            if (departmentId.HasValue)
            {
                sql += " AND DepartmentID = :DepartmentId";
                parameters.Add("DepartmentId", departmentId.Value);
            }
            if (!string.IsNullOrWhiteSpace(position))
            {
                sql += " AND Position LIKE :Position";
                parameters.Add("Position", $"%{position}%");
            }
            sql += " ORDER BY StaffName";
            return await connection.QueryAsync<Staff>(sql, parameters);
        }

        // 更新员工状态
        public async Task<bool> UpdateStaffStatusAsync(int staffId, string status)
        {
            using var connection = _dbService.CreateConnection();
            var sql = "UPDATE PUB.Staff SET Status = :Status WHERE StaffID = :StaffId";
            var result = await connection.ExecuteAsync(sql, new { Status = status, StaffId = staffId });
            return result > 0;
        }

        // 获取考勤记录
        public async Task<IEnumerable<dynamic>> GetAttendanceRecordsAsync(DateTime? startDate = null, DateTime? endDate = null, int? staffId = null)
        {
            using var connection = _dbService.CreateConnection();
            var whereClause = "WHERE 1=1";
            var parameters = new DynamicParameters();

            if (startDate.HasValue)
            {
                whereClause += " AND a.WorkDate >= :StartDate";
                parameters.Add("StartDate", startDate.Value);
            }

            if (endDate.HasValue)
            {
                whereClause += " AND a.WorkDate <= :EndDate";
                parameters.Add("EndDate", endDate.Value);
            }

            if (staffId.HasValue)
            {
                whereClause += " AND a.StaffID = :StaffId";
                parameters.Add("StaffId", staffId.Value);
            }

            var sql = $@"
                SELECT a.*, s.StaffName 
                FROM PUB.Attendance a
                INNER JOIN PUB.Staff s ON a.StaffID = s.StaffID
                {whereClause}
                ORDER BY a.WorkDate DESC, s.StaffName";

            return await connection.QueryAsync(sql, parameters);
        }

        // 员工打卡（补全StoreID）
        public async Task<bool> CheckInAsync(int staffId)
        {
            using var connection = _dbService.CreateConnection();
            var today = DateTime.Today;

            // 查询员工信息，获取StoreID
            var staff = await GetStaffByIdAsync(staffId);
            if (staff == null) return false;

            // 检查今天是否已经有考勤记录
            var existingRecord = await connection.QueryFirstOrDefaultAsync<Attendance>(
                "SELECT * FROM PUB.Attendance WHERE StaffID = :StaffId AND WorkDate = :WorkDate",
                new { StaffId = staffId, WorkDate = today });

            if (existingRecord != null)
            {
                // 更新打卡时间
                var sql = "UPDATE PUB.Attendance SET CheckInTime = :CheckInTime WHERE AttendanceID = :AttendanceId";
                var result = await connection.ExecuteAsync(sql, new
                {
                    CheckInTime = DateTime.Now,
                    AttendanceId = existingRecord.AttendanceID
                });
                return result > 0;
            }
            else
            {
                // 创建新的考勤记录（补全StoreID）
                var sql = @"
                    INSERT INTO PUB.Attendance (StaffID, WorkDate, CheckInTime, Status, StoreID)
                    VALUES (:StaffId, :WorkDate, :CheckInTime, :Status, :StoreID)";
                var result = await connection.ExecuteAsync(sql, new
                {
                    StaffId = staffId,
                    WorkDate = today,
                    CheckInTime = DateTime.Now,
                    Status = "正常",
                    StoreID = staff.StoreID
                });
                return result > 0;
            }
        }

        // 员工签退（修正字段名为ActualWorkHours）
        public async Task<bool> CheckOutAsync(int staffId)
        {
            using var connection = _dbService.CreateConnection();
            var today = DateTime.Today;

            // 获取CheckInTime
            var attendance = await connection.QueryFirstOrDefaultAsync<Attendance>(
                "SELECT * FROM PUB.Attendance WHERE StaffID = :StaffId AND WorkDate = :WorkDate",
                new { StaffId = staffId, WorkDate = today });

            if (attendance == null || attendance.CheckInTime == null)
                return false;

            var checkOutTime = DateTime.Now;
            var actualWorkHours = (decimal)((checkOutTime - attendance.CheckInTime.Value).TotalHours);

            var sql = @"
                UPDATE PUB.Attendance 
                SET CheckOutTime = :CheckOutTime,
                    ActualWorkHours = :ActualWorkHours
                WHERE StaffID = :StaffId AND WorkDate = :WorkDate";

            var result = await connection.ExecuteAsync(sql, new
            {
                CheckOutTime = checkOutTime,
                ActualWorkHours = actualWorkHours,
                StaffId = staffId,
                WorkDate = today
            });
            return result > 0;
        }
    }
}