using Dapper;
using RestaurantManagement.Models;
using System.Data;

namespace RestaurantManagement.Services
{
    public class ScheduleService
    {


        private readonly DatabaseService _dbService;
        public ScheduleService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        // 获取所有排班（带员工名和门店名）
        public async Task<IEnumerable<dynamic>> GetAllSchedulesAsync()
        {
            using var connection = _dbService.CreateConnection();
            var sql = @"
                SELECT s.*, st.STAFFNAME, st.STOREID, st2.STORENAME
                FROM PUB.STAFFSCHEDULE s
                LEFT JOIN PUB.STAFF st ON s.STAFFID = st.STAFFID
                LEFT JOIN PUB.STORE st2 ON s.STOREID = st2.STOREID
                ORDER BY s.WORKDATE, s.STARTTIME
            ";
            return await connection.QueryAsync(sql);
        }

        // 获取某员工某天的排班
        public async Task<IEnumerable<Schedule>> GetScheduleByStaffAndDateAsync(int staffId, int workDate)
        {
            using var connection = _dbService.CreateConnection();
                    var sql = @"SELECT * FROM PUB.STAFFSCHEDULE WHERE STAFFID = :StaffID AND WORKDATE = :WorkDate";
            return await connection.QueryAsync<Schedule>(sql, new { StaffID = staffId, WorkDate = workDate });
        }

        // 获取某门店某天所有排班
        public async Task<IEnumerable<Schedule>> GetScheduleByStoreAndDateAsync(int storeId, int workDate)
        {
            using var connection = _dbService.CreateConnection();
                    var sql = @"SELECT * FROM PUB.STAFFSCHEDULE WHERE STOREID = :StoreID AND WORKDATE = :WorkDate";
            return await connection.QueryAsync<Schedule>(sql, new { StoreID = storeId, WorkDate = workDate });
        }

        // 新增排班
        public async Task<int> AddScheduleAsync(Schedule schedule)
        {
            using var connection = _dbService.CreateConnection();
            // 转换时间为 INTERVAL DAY TO SECOND 格式
            string startInterval = ConvertToInterval(schedule.StartTime);
            string endInterval = ConvertToInterval(schedule.EndTime);
            // 查询当前最大ID
            var maxIdObj = await connection.QuerySingleOrDefaultAsync<int?>("SELECT MAX(SCHEDULEID) FROM PUB.STAFFSCHEDULE");
            int newId = (maxIdObj ?? 0) + 1;
            var sql = @"INSERT INTO PUB.STAFFSCHEDULE (SCHEDULEID, STAFFID, WORKDATE, STARTTIME, ENDTIME, STOREID, NOTES) VALUES (:ScheduleID, :StaffID, :WorkDate, TO_DSINTERVAL(:StartTime), TO_DSINTERVAL(:EndTime), :StoreID, :Notes)";
            var param = new {
                ScheduleID = newId,
                StaffID = schedule.StaffID,
                WorkDate = schedule.WorkDate,
                StartTime = startInterval,
                EndTime = endInterval,
                StoreID = schedule.StoreID,
                Notes = schedule.Notes
            };
            var result = await connection.ExecuteAsync(sql, param);
            if (result > 0)
            {
                return newId;
            }
            throw new Exception("新增排班失败");
        }

        // 编辑排班
        public async Task<bool> UpdateScheduleAsync(Schedule schedule)
        {
            using var connection = _dbService.CreateConnection();
            // 转换时间为 INTERVAL DAY TO SECOND 格式
            string startInterval = ConvertToInterval(schedule.StartTime);
            string endInterval = ConvertToInterval(schedule.EndTime);
                    var sql = @"UPDATE PUB.STAFFSCHEDULE SET WORKDATE = :WorkDate, STARTTIME = TO_DSINTERVAL(:StartTime), ENDTIME = TO_DSINTERVAL(:EndTime), STOREID = :StoreID, NOTES = :Notes WHERE SCHEDULEID = :ScheduleID";
            var param = new {
                WorkDate = schedule.WorkDate,
                StartTime = startInterval,
                EndTime = endInterval,
                StoreID = schedule.StoreID,
                Notes = schedule.Notes,
                ScheduleID = schedule.ScheduleID
            };
            var result = await connection.ExecuteAsync(sql, param);
            return result > 0;
        }
        // 工具方法：将 "08:00:00" 转为 "0 08:00:00" 以适配 INTERVAL DAY TO SECOND
        private string ConvertToInterval(object timeObj)
        {
            if (timeObj == null) return "0 00:00:00";
            string timeStr = timeObj?.ToString() ?? "00:00:00";
            // 如果已是 INTERVAL 格式则直接返回
            if (!string.IsNullOrEmpty(timeStr) && timeStr.Contains(" ")) return timeStr;
            // 只给时分秒加前缀 "0 "
            return $"0 {timeStr}";
        }
        

        // 删除排班
        public async Task<bool> DeleteScheduleAsync(int scheduleId)
        {
            using var connection = _dbService.CreateConnection();
                    var sql = @"DELETE FROM PUB.STAFFSCHEDULE WHERE SCHEDULEID = :ScheduleID";
            var result = await connection.ExecuteAsync(sql, new { ScheduleID = scheduleId });
            return result > 0;
        }
    }
}
