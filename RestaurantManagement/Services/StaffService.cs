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
            
            // 如果Status为空或null，则不更新状态字段
            string sql;
            if (string.IsNullOrEmpty(staff.Status))
            {
                sql = @"
                    UPDATE PUB.STAFF 
                    SET STAFFNAME = :StaffName, GENDER = :Gender, POSITION = :Position, PHONE = :Phone, 
                        EMAIL = :Email, HIREDATE = :HireDate, SALARY = :Salary, DEPARTMENTID = :DepartmentID, 
                        STOREID = :StoreID, WORKSCHEDULE = :WorkSchedule, UPDATETIME = :UpdateTime
                    WHERE STAFFID = :StaffID";
            }
            else
            {
                sql = @"
                    UPDATE PUB.STAFF 
                    SET STAFFNAME = :StaffName, GENDER = :Gender, POSITION = :Position, PHONE = :Phone, 
                        EMAIL = :Email, HIREDATE = :HireDate, SALARY = :Salary, DEPARTMENTID = :DepartmentID, 
                        STOREID = :StoreID, STATUS = :Status, WORKSCHEDULE = :WorkSchedule, UPDATETIME = :UpdateTime
                    WHERE STAFFID = :StaffID";
            }
            
            staff.UpdateTime = DateTime.Now;

            if (staff.DepartmentID == 0 || staff.StoreID == 0)
                throw new ArgumentException("DepartmentID 和 StoreID 必须有效且存在。");

            var result = await connection.ExecuteAsync(sql, staff);
            return result > 0;
        }

        // 删除员工（增强版，包含安全检查）
        public async Task<bool> DeleteStaffAsync(int staffId)
        {
            using var connection = _dbService.CreateConnection();
            
            // 检查员工是否存在
            var staff = await GetStaffByIdAsync(staffId);
            if (staff == null)
                return false;

            // 检查员工是否有未完成的考勤记录
            var hasActiveAttendance = await connection.QueryFirstOrDefaultAsync<int?>(
                "SELECT COUNT(*) FROM PUB.ATTENDANCE WHERE STAFFID = :StaffId AND CHECKOUTTIME IS NULL",
                new { StaffId = staffId });

            if (hasActiveAttendance > 0)
                throw new InvalidOperationException("该员工还有未完成的考勤记录，无法删除");

            // 软删除：将状态设为"离职"而不是物理删除（避免约束冲突）
            var sql = "UPDATE PUB.STAFF SET STATUS = '离职', UPDATETIME = :UpdateTime WHERE STAFFID = :StaffId";
            var result = await connection.ExecuteAsync(sql, new { 
                StaffId = staffId,
                UpdateTime = DateTime.Now
            });
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

        // 物理删除员工（谨慎使用）
        public async Task<bool> HardDeleteStaffAsync(int staffId)
        {
            using var connection = _dbService.CreateConnection();
            
            // 检查员工是否存在
            var staff = await GetStaffByIdAsync(staffId);
            if (staff == null)
                return false;

            // 检查是否有相关的考勤记录
            var hasAttendanceRecords = await connection.QueryFirstOrDefaultAsync<int?>(
                "SELECT COUNT(*) FROM PUB.ATTENDANCE WHERE STAFFID = :StaffId",
                new { StaffId = staffId });

            if (hasAttendanceRecords > 0)
                throw new InvalidOperationException("该员工有考勤记录，不能物理删除。请使用软删除。");

            // 物理删除员工记录
            var sql = "DELETE FROM PUB.STAFF WHERE STAFFID = :StaffId";
            var result = await connection.ExecuteAsync(sql, new { StaffId = staffId });
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
                    a.ATTENDANCEID,
                    a.STAFFID,
                    a.WORKDATE,
                    a.CHECKINTIME,
                    a.CHECKOUTTIME,
                    a.ACTUALWORKHOURS as WORKHOURS,
                    a.STATUS,
                    s.STAFFNAME,
                    s.POSITION,
                    d.DEPARTMENTNAME,
                    st.STORENAME
                FROM PUB.ATTENDANCE a
                INNER JOIN PUB.STAFF s ON a.STAFFID = s.STAFFID
                LEFT JOIN PUB.DEPARTMENT d ON s.DEPARTMENTID = d.DEPARTMENTID
                LEFT JOIN PUB.STORE st ON s.STOREID = st.STOREID
                {whereClause}
                ORDER BY a.WORKDATE DESC, s.STAFFNAME";

            return await connection.QueryAsync(sql, parameters);
        }

        // 员工打卡（补全StoreID并计算状态）
        public async Task<bool> CheckInAsync(int staffId)
        {
            using var connection = _dbService.CreateConnection();
            var today = DateTime.Today;
            var now = DateTime.Now;

            // 查询员工信息，获取StoreID
            var staff = await GetStaffByIdAsync(staffId);
            if (staff == null) return false;

            // 检查今天是否已经有考勤记录
            var existingRecord = await connection.QueryFirstOrDefaultAsync<Attendance>(
                "SELECT * FROM PUB.ATTENDANCE WHERE STAFFID = :StaffId AND WORKDATE = :WorkDate",
                new { StaffId = staffId, WorkDate = today });

            // 计算状态（标准上班时间为9:00）
            var standardStartTime = new TimeSpan(9, 0, 0);
            var currentTime = now.TimeOfDay;
            var status = currentTime > standardStartTime.Add(TimeSpan.FromMinutes(30)) ? "迟到" : "正常";

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
                // 创建新的考勤记录（手动生成ATTENDANCEID）
                // 先获取下一个ATTENDANCEID
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

        // 员工签退（修正字段名为ActualWorkHours并检测早退）
        public async Task<bool> CheckOutAsync(int staffId)
        {
            using var connection = _dbService.CreateConnection();
            var today = DateTime.Today;
            var now = DateTime.Now;

            // 获取CheckInTime
            var attendance = await connection.QueryFirstOrDefaultAsync<Attendance>(
                "SELECT * FROM PUB.ATTENDANCE WHERE STAFFID = :StaffId AND WORKDATE = :WorkDate",
                new { StaffId = staffId, WorkDate = today });

            if (attendance == null || attendance.CheckInTime == null)
                return false;

            var actualWorkHours = (decimal)((now - attendance.CheckInTime.Value).TotalHours);

            // 计算状态（标准下班时间为18:00，标准工作时长8小时）
            var standardEndTime = new TimeSpan(18, 0, 0);
            var currentTime = now.TimeOfDay;
            var currentStatus = attendance.Status;

            // 如果当前时间早于标准下班时间30分钟以上，且工作时长少于7.5小时，则为早退
            if (currentTime < standardEndTime.Subtract(TimeSpan.FromMinutes(30)) && actualWorkHours < 7.5m)
            {
                currentStatus = "早退";
            }
            // 如果已经是迟到状态，保持迟到状态
            else if (currentStatus != "迟到")
            {
                currentStatus = "正常";
            }

            var sql = @"
                UPDATE PUB.ATTENDANCE 
                SET CHECKOUTTIME = :CheckOutTime,
                    ACTUALWORKHOURS = :ActualWorkHours,
                    STATUS = :Status
                WHERE STAFFID = :StaffId AND WORKDATE = :WorkDate";

            var result = await connection.ExecuteAsync(sql, new
            {
                CheckOutTime = now,
                ActualWorkHours = actualWorkHours,
                Status = currentStatus,
                StaffId = staffId,
                WorkDate = today
            });
            return result > 0;
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
    }
}