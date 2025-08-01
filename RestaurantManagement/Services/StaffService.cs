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

        // 获取所有员�?
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

        // 添加员工
        public async Task<bool> AddStaffAsync(Staff staff)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                INSERT INTO PuB.Staff (StaffName, Position, Phone, Salary, Status, WorkSchedule, HireDate)
                VALUES (:StaffName, :Position, :Phone, :Salary, :Status, :WorkSchedule, :HireDate)";
            var result = await connection.ExecuteAsync(sql, staff);
            return result > 0;
        }

        // 更新员工信息
        public async Task<bool> UpdateStaffAsync(Staff staff)
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                UPDATE PUB.Staff 
                SET StaffName = :StaffName, Position = :Position, Phone = :Phone, 
                    Salary = :Salary, Status = :Status, WorkSchedule = :WorkSchedule
                WHERE StaffID = :StaffID";
            var result = await connection.ExecuteAsync(sql, staff);
            return result > 0;
        }

        // 更新员工状�?
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

        // 员工打卡
        public async Task<bool> CheckInAsync(int staffId)
        {
            using var connection = _dbService.CreateConnection();
            var today = DateTime.Today;
            
            // 检查今天是否已经有考勤记录
            var existingRecord = await connection.QueryFirstOrDefaultAsync<Attendance>(
                "SELECT * FROM PUB.Attendance WHERE StaffID = :StaffId AND WorkDate = :WorkDate",
                new { StaffId = staffId, WorkDate = today });

            if (existingRecord != null)
            {
                // 更新打卡时间
                var sql = "UPDATE PUB.Attendance SET CheckInTime = :CheckInTime WHERE AttendanceID = :AttendanceId";
                var result = await connection.ExecuteAsync(sql, new { 
                    CheckInTime = DateTime.Now, 
                    AttendanceId = existingRecord.AttendanceID 
                });
                return result > 0;
            }
            else
            {
                // 创建新的考勤记录
                var sql = @"
                    INSERT INTO PUB.Attendance (StaffID, WorkDate, CheckInTime, Status)
                    VALUES (:StaffId, :WorkDate, :CheckInTime, :Status)";
                var result = await connection.ExecuteAsync(sql, new { 
                    StaffId = staffId, 
                    WorkDate = today, 
                    CheckInTime = DateTime.Now,
                    Status = "正常"
                });
                return result > 0;
            }
        }

        // 员工签退
        public async Task<bool> CheckOutAsync(int staffId)
        {
            using var connection = _dbService.CreateConnection();
            var today = DateTime.Today;
            
            var sql = @"
                UPDATE PUB.Attendance 
                SET CheckOutTime = :CheckOutTime,
                    WorkHours = DATEDIFF(MINUTE, CheckInTime, :CheckOutTime) / 60.0
                WHERE StaffID = :StaffId AND WorkDate = :WorkDate";
            
            var result = await connection.ExecuteAsync(sql, new { 
                CheckOutTime = DateTime.Now, 
                StaffId = staffId, 
                WorkDate = today 
            });
            return result > 0;
        }
    }
}
