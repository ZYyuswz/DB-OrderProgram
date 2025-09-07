using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            var sql = @"
                SELECT s.*, d.DEPARTMENTNAME 
                FROM PUB.STAFF s
                LEFT JOIN PUB.DEPARTMENT d ON s.DEPARTMENTID = d.DEPARTMENTID
                ORDER BY s.STAFFID";
            return await connection.QueryAsync<Staff>(sql);
        }

        // 根据ID获取员工
        public async Task<Staff?> GetStaffByIdAsync(int staffId)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                SELECT s.*, d.DEPARTMENTNAME 
                FROM PUB.STAFF s
                LEFT JOIN PUB.DEPARTMENT d ON s.DEPARTMENTID = d.DEPARTMENTID
                WHERE s.STAFFID = :StaffId";
            return await connection.QueryFirstOrDefaultAsync<Staff>(sql, new { StaffId = staffId });
        }

        // 员工登录验证
        public async Task<Staff?> ValidateLoginAsync(string username, string password)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                SELECT s.*, d.DEPARTMENTNAME 
                FROM PUB.STAFF s
                LEFT JOIN PUB.DEPARTMENT d ON s.DEPARTMENTID = d.DEPARTMENTID
                WHERE s.USERNAME = :Username AND s.PASSWORD = :Password AND s.STATUS = '在职'";
            return await connection.QueryFirstOrDefaultAsync<Staff>(sql, new { Username = username, Password = password });
        }

        // 添加员工
        public async Task<bool> AddStaffAsync(Staff staff)
        {
            using var connection = _dbService.CreateConnection();
            
            // 先查询所有员工ID，获取最大值
            var maxIdSql = "SELECT NVL(MAX(STAFFID), 0) FROM PUB.STAFF";
            var maxId = await connection.QueryFirstOrDefaultAsync<int>(maxIdSql);
            
            // 设置新员工ID为最大值+1
            staff.StaffID = maxId + 1;
            
            var sql = @"
                INSERT INTO PUB.STAFF (STAFFID, STAFFNAME, GENDER, POSITION, PHONE, EMAIL, 
                                      HIREDATE, SALARY, DEPARTMENTID, STOREID, STATUS, 
                                      CREATETIME, UPDATETIME)
                VALUES (:StaffID, :StaffName, :Gender, :Position, :Phone, :Email, 
                        :HireDate, :Salary, :DepartmentID, :StoreID, :Status,
                        SYSDATE, SYSDATE)";
            var result = await connection.ExecuteAsync(sql, staff);
            return result > 0;
        }

        // 更新员工信息
        public async Task<bool> UpdateStaffAsync(Staff staff)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                UPDATE PUB.STAFF 
                SET STAFFNAME = :StaffName,
                    GENDER = :Gender,
                    POSITION = :Position,
                    PHONE = :Phone,
                    EMAIL = :Email,
                    SALARY = :Salary,
                    DEPARTMENTID = :DepartmentId,
                    STOREID = :StoreId,
                    STATUS = :Status,
                    UPDATETIME = SYSDATE
                WHERE STAFFID = :StaffId";
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

        // 员工打卡（基于排班信息计算状态）
        public async Task<bool> CheckInAsync(int staffId)
        {
            using var connection = _dbService.CreateConnection();
            var today = DateTime.Today;
            var now = DateTime.Now;

            // 查询员工信息，获取StoreID
            var staff = await GetStaffByIdAsync(staffId);
            if (staff == null) return false;

            // 获取员工今天的排班信息（根据今天是星期几）
            var dayOfWeek = (int)today.DayOfWeek;
            if (dayOfWeek == 0) dayOfWeek = 7; // 星期日转为7

            var schedule = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
                SELECT STARTTIME, ENDTIME FROM PUB.STAFFSCHEDULE 
                WHERE STAFFID = :StaffId AND WORKDATE = :DayOfWeek",
                new { StaffId = staffId, DayOfWeek = dayOfWeek });

            // 检查今天是否已经有考勤记录
            var existingRecord = await connection.QueryFirstOrDefaultAsync<Attendance>(
                "SELECT * FROM PUB.ATTENDANCE WHERE STAFFID = :StaffId AND WORKDATE = :WorkDate",
                new { StaffId = staffId, WorkDate = today });

            // 计算状态
            string status = "正常";
            if (schedule != null)
            {
                try
                {
                    // 解析排班开始时间 - 更安全的方式处理动态对象
                    var startTimeObj = schedule.STARTTIME;
                    string? startTimeValue = null;
                    
                    if (startTimeObj != null)
                    {
                        startTimeValue = startTimeObj.ToString();
                    }
                    
                    var scheduledStartTime = ParseIntervalTime(startTimeValue);
                    if (scheduledStartTime.HasValue)
                    {
                        var currentTime = now.TimeOfDay;
                        // 如果迟到超过30分钟，标记为迟到
                        if (currentTime > scheduledStartTime.Value.Add(TimeSpan.FromMinutes(30)))
                        {
                            status = "迟到";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"解析排班时间失败: {ex.Message}");
                    // 如果解析失败，使用默认逻辑
                }
            }
            else
            {
                // 没有排班时使用默认标准时间（9:00）
                var standardStartTime = new TimeSpan(9, 0, 0);
                var currentTime = now.TimeOfDay;
                status = currentTime > standardStartTime.Add(TimeSpan.FromMinutes(30)) ? "迟到" : "正常";
            }

            if (existingRecord != null)
            {
                // 更新打卡时间和状态
                var sql = "UPDATE PUB.ATTENDANCE SET CHECKINTIME = :CheckInTime, STATUS = :Status WHERE ATTENDANCEID = :AttendanceId";
                var result = await connection.ExecuteAsync(sql, new
                {
                    CheckInTime = now,
                    Status = status,
                    AttendanceId = existingRecord.AttendanceID
                });
                return result > 0;
            }
            else
            {
                // 创建新的考勤记录
                var nextId = await connection.QueryFirstOrDefaultAsync<int>(
                    "SELECT NVL(MAX(ATTENDANCEID), 0) + 1 FROM PUB.ATTENDANCE");
                
                var sql = @"
                    INSERT INTO PUB.ATTENDANCE (ATTENDANCEID, STAFFID, WORKDATE, CHECKINTIME, STATUS, STOREID)
                    VALUES (:AttendanceId, :StaffId, :WorkDate, :CheckInTime, :Status, :StoreID)";
                var result = await connection.ExecuteAsync(sql, new
                {
                    AttendanceId = nextId,
                    StaffId = staffId,
                    WorkDate = today,
                    CheckInTime = now,
                    Status = status,
                    StoreID = staff.StoreID
                });
                return result > 0;
            }
        }

        // 员工签退（基于排班信息检测早退）
        public async Task<bool> CheckOutAsync(int staffId)
        {
            using var connection = _dbService.CreateConnection();
            var today = DateTime.Today;
            var now = DateTime.Now;

            // 获取员工今天的排班信息
            var dayOfWeek = (int)today.DayOfWeek;
            if (dayOfWeek == 0) dayOfWeek = 7; // 星期日转为7

            var schedule = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
                SELECT STARTTIME, ENDTIME FROM PUB.STAFFSCHEDULE 
                WHERE STAFFID = :StaffId AND WORKDATE = :DayOfWeek",
                new { StaffId = staffId, DayOfWeek = dayOfWeek });

            // 获取考勤记录
            var attendance = await connection.QueryFirstOrDefaultAsync<Attendance>(
                "SELECT * FROM PUB.ATTENDANCE WHERE STAFFID = :StaffId AND WORKDATE = :WorkDate",
                new { StaffId = staffId, WorkDate = today });

            if (attendance == null || attendance.CheckInTime == null)
                return false;

            var actualWorkHours = (decimal)((now - attendance.CheckInTime.Value).TotalHours);
            var currentStatus = attendance.Status;

            // 基于排班判断早退
            if (schedule != null)
            {
                try
                {
                    // 更安全的方式处理动态对象
                    var endTimeObj = schedule.ENDTIME;
                    var startTimeObj = schedule.STARTTIME;
                    
                    string? endTimeValue = null;
                    string? startTimeValue = null;
                    
                    if (endTimeObj != null) endTimeValue = endTimeObj.ToString();
                    if (startTimeObj != null) startTimeValue = startTimeObj.ToString();
                    
                    var scheduledEndTime = ParseIntervalTime(endTimeValue);
                    if (scheduledEndTime.HasValue)
                    {
                        var currentTime = now.TimeOfDay;
                        var scheduledStartTime = ParseIntervalTime(startTimeValue);
                        if (scheduledStartTime.HasValue)
                        {
                            var scheduledWorkHours = (decimal)(scheduledEndTime.Value - scheduledStartTime.Value).TotalHours;
                            if (currentTime < scheduledEndTime.Value.Subtract(TimeSpan.FromMinutes(30)) && 
                                actualWorkHours < scheduledWorkHours * 0.8m)
                            {
                                currentStatus = "早退";
                            }
                            else if (currentStatus != "迟到") // 保持迟到状态
                            {
                                currentStatus = "正常";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"解析排班时间失败: {ex.Message}");
                    // 如果解析失败，使用默认逻辑
                }
            }
            else
            {
                // 没有排班时使用默认逻辑
                var standardEndTime = new TimeSpan(18, 0, 0);
                var currentTime = now.TimeOfDay;
                if (currentTime < standardEndTime.Subtract(TimeSpan.FromMinutes(30)) && actualWorkHours < 7.5m)
                {
                    currentStatus = "早退";
                }
                else if (currentStatus != "迟到")
                {
                    currentStatus = "正常";
                }
            }

            var sql = @"
                UPDATE PUB.ATTENDANCE 
                SET CHECKOUTTIME = :CheckOutTime,
                    ACTUALWORKHOURS = :ActualWorkHours,
                    STATUS = :Status
                WHERE STAFFID = :StaffId AND WORKDATE = :WorkDate";

            Console.WriteLine($"签退更新SQL: StaffId={staffId}, WorkDate={today}, ActualWorkHours={actualWorkHours}, Status={currentStatus}");

            var result = await connection.ExecuteAsync(sql, new
            {
                CheckOutTime = now,
                ActualWorkHours = actualWorkHours,
                Status = currentStatus,
                StaffId = staffId,
                WorkDate = today
            });
            
            Console.WriteLine($"签退更新结果: {result} 行受影响");
            return result > 0;
        }

        // 解析Oracle INTERVAL DAY TO SECOND 时间格式
        private TimeSpan? ParseIntervalTime(string? intervalStr)
        {
            if (string.IsNullOrEmpty(intervalStr)) return null;
            
            try
            {
                // Oracle INTERVAL格式: "0 08:30:00.000000"
                if (intervalStr.Contains(' '))
                {
                    var parts = intervalStr.Split(' ');
                    if (parts.Length >= 2)
                    {
                        var timePart = parts[1];
                        if (TimeSpan.TryParse(timePart, out var result))
                        {
                            return result;
                        }
                    }
                }
                // 直接尝试解析时间格式
                else if (TimeSpan.TryParse(intervalStr, out var result))
                {
                    return result;
                }
            }
            catch
            {
                // 解析失败时返回null
            }
            
            return null;
        }

        // 获取考勤记录
        public async Task<IEnumerable<dynamic>> GetAttendanceRecordsAsync(DateTime? startDate = null, DateTime? endDate = null, int? staffId = null)
        {
            using var connection = _dbService.CreateConnection();
            var whereClause = "WHERE 1=1";
            var parameters = new DynamicParameters();

            if (startDate.HasValue)
            {
                whereClause += " AND a.WORKDATE >= :StartDate";
                parameters.Add("StartDate", startDate.Value);
            }

            if (endDate.HasValue)
            {
                whereClause += " AND a.WORKDATE <= :EndDate";
                parameters.Add("EndDate", endDate.Value);
            }

            if (staffId.HasValue)
            {
                whereClause += " AND a.STAFFID = :StaffId";
                parameters.Add("StaffId", staffId.Value);
            }

            var sql = $@"
                SELECT 
                    a.ATTENDANCEID as AttendanceID,
                    a.STAFFID as StaffID,
                    a.WORKDATE as WorkDate,
                    a.CHECKINTIME as CheckInTime,
                    a.CHECKOUTTIME as CheckOutTime,
                    CASE 
                        WHEN a.CHECKOUTTIME IS NOT NULL THEN a.ACTUALWORKHOURS
                        WHEN a.CHECKINTIME IS NOT NULL THEN 
                            ROUND((SYSDATE - a.CHECKINTIME) * 24, 2)
                        ELSE NULL 
                    END as ActualWorkHours,
                    a.STATUS as Status,
                    a.STOREID as StoreID,
                    s.STAFFNAME as StaffName,
                    d.DEPARTMENTNAME as DepartmentName
                FROM PUB.ATTENDANCE a
                LEFT JOIN PUB.STAFF s ON a.STAFFID = s.STAFFID
                LEFT JOIN PUB.DEPARTMENT d ON s.DEPARTMENTID = d.DEPARTMENTID
                {whereClause}
                ORDER BY a.WORKDATE DESC, a.STAFFID";

            return await connection.QueryAsync<dynamic>(sql, parameters);
        }

        // 获取考勤统计
        public async Task<IEnumerable<dynamic>> GetAttendanceStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null, int? departmentId = null)
        {
            using var connection = _dbService.CreateConnection();
            var whereClause = "WHERE 1=1";
            var parameters = new DynamicParameters();

            if (startDate.HasValue)
            {
                whereClause += " AND a.WORKDATE >= :StartDate";
                parameters.Add("StartDate", startDate.Value);
            }

            if (endDate.HasValue)
            {
                whereClause += " AND a.WORKDATE <= :EndDate";
                parameters.Add("EndDate", endDate.Value);
            }

            if (departmentId.HasValue)
            {
                whereClause += " AND s.DEPARTMENTID = :DepartmentId";
                parameters.Add("DepartmentId", departmentId.Value);
            }

            var sql = $@"
                SELECT 
                    s.STAFFID,
                    s.STAFFNAME,
                    d.DEPARTMENTNAME,
                    COUNT(a.ATTENDANCEID) as TOTALDAYS,
                    SUM(CASE WHEN a.STATUS = '正常' THEN 1 ELSE 0 END) as NORMALDAYS,
                    SUM(CASE WHEN a.STATUS = '迟到' THEN 1 ELSE 0 END) as LATEDAYS,
                    SUM(CASE WHEN a.STATUS = '早退' THEN 1 ELSE 0 END) as EARLYLEAVEDAYS,
                    SUM(CASE WHEN a.STATUS = '缺勤' THEN 1 ELSE 0 END) as ABSENTDAYS,
                    NVL(SUM(a.ACTUALWORKHOURS), 0) as TOTALWORKHOURS,
                    NVL(AVG(a.ACTUALWORKHOURS), 0) as AVGWORKHOURS,
                    CASE 
                        WHEN COUNT(a.ATTENDANCEID) = 0 THEN 0
                        ELSE ROUND((COUNT(a.ATTENDANCEID) - SUM(CASE WHEN a.STATUS = '缺勤' THEN 1 ELSE 0 END)) * 100.0 / COUNT(a.ATTENDANCEID), 2)
                    END as ATTENDANCERATE
                FROM PUB.STAFF s
                LEFT JOIN PUB.ATTENDANCE a ON s.STAFFID = a.STAFFID
                LEFT JOIN PUB.DEPARTMENT d ON s.DEPARTMENTID = d.DEPARTMENTID
                {whereClause}
                AND s.STATUS = '在职'
                GROUP BY s.STAFFID, s.STAFFNAME, d.DEPARTMENTNAME
                ORDER BY s.STAFFNAME";

            return await connection.QueryAsync(sql, parameters);
        }

        // 更新员工状态
        public async Task<bool> UpdateStaffStatusAsync(int staffId, string status)
        {
            using var connection = _dbService.CreateConnection();
            var sql = "UPDATE PUB.STAFF SET STATUS = :Status WHERE STAFFID = :StaffId";
            var result = await connection.ExecuteAsync(sql, new { Status = status, StaffId = staffId });
            return result > 0;
        }

        // 搜索员工
        public async Task<IEnumerable<Staff>> SearchStaffAsync(string? name, int? departmentId, string? position)
        {
            using var connection = _dbService.CreateConnection();
            var whereClause = "WHERE 1=1";
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(name))
            {
                whereClause += " AND s.STAFFNAME LIKE :Name";
                parameters.Add("Name", $"%{name}%");
            }

            if (departmentId.HasValue)
            {
                whereClause += " AND s.DEPARTMENTID = :DepartmentId";
                parameters.Add("DepartmentId", departmentId.Value);
            }

            if (!string.IsNullOrEmpty(position))
            {
                whereClause += " AND s.POSITION LIKE :Position";
                parameters.Add("Position", $"%{position}%");
            }

            var sql = $@"
                SELECT s.*, d.DEPARTMENTNAME 
                FROM PUB.STAFF s
                LEFT JOIN PUB.DEPARTMENT d ON s.DEPARTMENTID = d.DEPARTMENTID
                {whereClause}
                ORDER BY s.STAFFNAME";

            return await connection.QueryAsync<Staff>(sql, parameters);
        }

        // 硬删除员工
        public async Task<bool> HardDeleteStaffAsync(int staffId)
        {
            using var connection = _dbService.CreateConnection();
            
            // 检查是否有考勤记录
            var hasAttendance = await connection.QueryFirstOrDefaultAsync<int>(
                "SELECT COUNT(*) FROM PUB.ATTENDANCE WHERE STAFFID = :StaffId",
                new { StaffId = staffId });

            if (hasAttendance > 0)
                return false; // 有考勤记录，不能删除

            var sql = "DELETE FROM PUB.STAFF WHERE STAFFID = :StaffId";
            var result = await connection.ExecuteAsync(sql, new { StaffId = staffId });
            return result > 0;
        }
    }
}
