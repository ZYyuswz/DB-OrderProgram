using Dapper;
using RestaurantManagement.Models;
using Oracle.ManagedDataAccess.Client;
using System.Data;

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
            var sql = @"SELECT 
                STAFFID as StaffID,
                STAFFNAME as StaffName, 
                GENDER as Gender,
                POSITION as Position,
                PHONE as Phone,
                EMAIL as Email,
                HIREDATE as HireDate,
                SALARY as Salary,
                DEPARTMENTID as DepartmentID,
                STOREID as StoreID,
                STATUS as Status,
                WORKSCHEDULE as WorkSchedule,
                CREATETIME as CreateTime,
                UPDATETIME as UpdateTime
                FROM PUB.STAFF ORDER BY STAFFNAME";
            return await connection.QueryAsync<Staff>(sql);
        }

        // 根据ID获取员工
        public async Task<Staff?> GetStaffByIdAsync(int staffId)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"SELECT 
                STAFFID as StaffID,
                STAFFNAME as StaffName, 
                GENDER as Gender,
                POSITION as Position,
                PHONE as Phone,
                EMAIL as Email,
                HIREDATE as HireDate,
                SALARY as Salary,
                DEPARTMENTID as DepartmentID,
                STOREID as StoreID,
                STATUS as Status,
                WORKSCHEDULE as WorkSchedule,
                CREATETIME as CreateTime,
                UPDATETIME as UpdateTime
                FROM PUB.STAFF WHERE STAFFID = :StaffId";
            return await connection.QueryFirstOrDefaultAsync<Staff>(sql, new { StaffId = staffId });
        }

        // 添加员工（使用Oracle序列自动生成ID）
        public async Task<bool> AddStaffAsync(Staff staff)
        {
            using var connection = _dbService.CreateConnection();
            
            var maxId = await connection.ExecuteScalarAsync<int>(
                "SELECT MAX(STAFFID) FROM PUB.STAFF"
            );
            staff.StaffID = maxId + 1;

            // 2. 设置默认值
            staff.HireDate = DateTime.Now;
            staff.CreateTime = DateTime.Now;
            staff.UpdateTime = DateTime.Now;
            if (string.IsNullOrEmpty(staff.Status))
                staff.Status = "在职";

            // 3. 外键有效性简单校验
            if (staff.DepartmentID == 0 || staff.StoreID == 0)
                throw new ArgumentException("DepartmentID 和 StoreID 必须有效且存在。");

            // 4. 使用Oracle序列插入数据
            var sql = @"
                INSERT INTO PUB.STAFF 
                (STAFFID, STAFFNAME, GENDER, POSITION, PHONE, EMAIL, HIREDATE, SALARY, DEPARTMENTID, STOREID, STATUS, WORKSCHEDULE, CREATETIME, UPDATETIME)
                VALUES
                (:STAFFID, :STAFFNAME, :GENDER, :POSITION, :PHONE, :EMAIL, :HIREDATE, :SALARY, :DEPARTMENTID, :STOREID, :STATUS, :WORKSCHEDULE, :CREATETIME, :UPDATETIME)";

            var result = await connection.ExecuteAsync(sql, new {
                STAFFID = staff.StaffID,
                STAFFNAME = staff.StaffName,
                GENDER = staff.Gender,
                POSITION = staff.Position,
                PHONE = staff.Phone,
                EMAIL = staff.Email,
                HIREDATE = staff.HireDate,
                SALARY = staff.Salary,
                DEPARTMENTID = staff.DepartmentID,
                STOREID = staff.StoreID,
                STATUS = staff.Status,
                WORKSCHEDULE = staff.WorkSchedule,
                CREATETIME = staff.CreateTime,
                UPDATETIME = staff.UpdateTime
            });
            return result > 0;
        }



        // 更新员工信息（字段补全，增加外键有效性校验）
        public async Task<bool> UpdateStaffAsync(Staff staff)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                UPDATE PUB.STAFF 
                SET STAFFNAME = :StaffName, GENDER = :Gender, POSITION = :Position, PHONE = :Phone, 
                    EMAIL = :Email, HIREDATE = :HireDate, SALARY = :Salary, DEPARTMENTID = :DepartmentID, 
                    STOREID = :StoreID, STATUS = :Status, WORKSCHEDULE = :WorkSchedule, UPDATETIME = :UpdateTime
                WHERE STAFFID = :StaffID";
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
            var sql = "DELETE FROM PUB.STAFF WHERE STAFFID = :StaffId";
            var result = await connection.ExecuteAsync(sql, new { StaffId = staffId });
            return result > 0;
        }

        // 按条件搜索员工
        public async Task<IEnumerable<Staff>> SearchStaffAsync(string? name = null, int? departmentId = null, string? position = null)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"SELECT 
                STAFFID as StaffID,
                STAFFNAME as StaffName, 
                GENDER as Gender,
                POSITION as Position,
                PHONE as Phone,
                EMAIL as Email,
                HIREDATE as HireDate,
                SALARY as Salary,
                DEPARTMENTID as DepartmentID,
                STOREID as StoreID,
                STATUS as Status,
                WORKSCHEDULE as WorkSchedule,
                CREATETIME as CreateTime,
                UPDATETIME as UpdateTime
                FROM PUB.STAFF WHERE 1=1";
            var parameters = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(name))
            {
                sql += " AND STAFFNAME LIKE :Name";
                parameters.Add("Name", $"%{name}%");
            }
            if (departmentId.HasValue)
            {
                sql += " AND DEPARTMENTID = :DepartmentId";
                parameters.Add("DepartmentId", departmentId.Value);
            }
            if (!string.IsNullOrWhiteSpace(position))
            {
                sql += " AND POSITION LIKE :Position";
                parameters.Add("Position", $"%{position}%");
            }
            sql += " ORDER BY STAFFNAME";
            return await connection.QueryAsync<Staff>(sql, parameters);
        }

        // 更新员工状态
        public async Task<bool> UpdateStaffStatusAsync(int staffId, string status)
        {
            using var connection = _dbService.CreateConnection();
            var sql = "UPDATE PUB.STAFF SET STATUS = :Status, UPDATETIME = :UpdateTime WHERE STAFFID = :StaffId";
            var result = await connection.ExecuteAsync(sql, new { 
                Status = status, 
                StaffId = staffId,
                UpdateTime = DateTime.Now
            });
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