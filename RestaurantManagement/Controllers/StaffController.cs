using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Services;
using RestaurantManagement.Models;

namespace RestaurantManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StaffController : ControllerBase
    {
        private readonly StaffService _staffService;

        public StaffController(StaffService staffService)
        {
            _staffService = staffService;
        }

        // 获取所有员工
        [HttpGet]
        public async Task<IActionResult> GetAllStaff()
        {
            var staff = await _staffService.GetAllStaffAsync();
            return Ok(staff);
        }

        // 获取单个员工
        [HttpGet("{id}")]
        public async Task<IActionResult> GetStaff(int id)
        {
            var staff = await _staffService.GetStaffByIdAsync(id);
            if (staff == null)
                return NotFound();
            return Ok(staff);
        }

        // 新增员工
        [HttpPost]
        public async Task<IActionResult> AddStaff([FromBody] Staff staff)
        {
            staff.HireDate = DateTime.Now;
            staff.CreateTime = DateTime.Now;
            staff.UpdateTime = DateTime.Now;
            if (string.IsNullOrEmpty(staff.Status))
                staff.Status = "在职";
            var result = await _staffService.AddStaffAsync(staff);
            if (!result)
                return BadRequest("添加员工失败，请检查必填字段是否完整。");
            return Ok();
        }

        // 修改员工信息
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStaff(int id, [FromBody] Staff staff)
        {
            staff.StaffID = id;
            staff.UpdateTime = DateTime.Now;
            var result = await _staffService.UpdateStaffAsync(staff);
            if (!result)
                return BadRequest("更新员工信息失败。");
            return Ok();
        }

        // 修改员工状态
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStaffStatus(int id, [FromBody] string status)
        {
            var result = await _staffService.UpdateStaffStatusAsync(id, status);
            if (!result)
                return BadRequest("更新员工状态失败。");
            return Ok();
        }

        // 按条件搜索员工（支持姓名、部门、职位）
        [HttpGet("search")]
        public async Task<IActionResult> SearchStaff(
            [FromQuery] string? name,
            [FromQuery] int? departmentId,
            [FromQuery] string? position)
        {
            var staff = await _staffService.SearchStaffAsync(name, departmentId, position);
            return Ok(staff);
        }

        // 获取考勤记录
        [HttpGet("attendance")]
        public async Task<IActionResult> GetAttendanceRecords(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? staffId)
        {
            var records = await _staffService.GetAttendanceRecordsAsync(startDate, endDate, staffId);
            return Ok(records);
        }

        // 员工打卡
        [HttpPost("{id}/checkin")]
        public async Task<IActionResult> CheckIn(int id)
        {
            var result = await _staffService.CheckInAsync(id);
            if (!result)
                return BadRequest("打卡失败。");
            return Ok();
        }

        // 员工签退
        [HttpPost("{id}/checkout")]
        public async Task<IActionResult> CheckOut(int id)
        {
            var result = await _staffService.CheckOutAsync(id);
            if (!result)
                return BadRequest("签退失败。");
            return Ok();
        }

        // 获取考勤统计
        [HttpGet("attendance/statistics")]
        public async Task<IActionResult> GetAttendanceStatistics(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? departmentId)
        {
            var statistics = await _staffService.GetAttendanceStatisticsAsync(startDate, endDate, departmentId);
            return Ok(statistics);
        }
    }
}