using Microsoft.AspNetCore.Mvc;
using RestaurantManagement.Models;
using RestaurantManagement.Services;

namespace RestaurantManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScheduleController : ControllerBase
    {
        private readonly ScheduleService _scheduleService;
        public ScheduleController(ScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        // 获取所有排班
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _scheduleService.GetAllSchedulesAsync();
            return Ok(result);
        }

        // 获取某员工某天排班
        [HttpGet("staff/{staffId}/{workDate}")]
        public async Task<IActionResult> GetByStaffAndDate(int staffId, int workDate)
        {
            var result = await _scheduleService.GetScheduleByStaffAndDateAsync(staffId, workDate);
            return Ok(result);
        }

        // 获取某门店某天所有排班
        [HttpGet("store/{storeId}/{workDate}")]
        public async Task<IActionResult> GetByStoreAndDate(int storeId, int workDate)
        {
            var result = await _scheduleService.GetScheduleByStoreAndDateAsync(storeId, workDate);
            return Ok(result);
        }

        // 新增排班
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] Schedule schedule)
        {
            var id = await _scheduleService.AddScheduleAsync(schedule);
            return Ok(new { ScheduleID = id });
        }

        // 编辑排班
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] Schedule schedule)
        {
            var success = await _scheduleService.UpdateScheduleAsync(schedule);
            return Ok(new { Success = success });
        }

        // 删除排班
        [HttpDelete("{scheduleId}")]
        public async Task<IActionResult> Delete(int scheduleId)
        {
            var success = await _scheduleService.DeleteScheduleAsync(scheduleId);
            return Ok(new { Success = success });
        }
    }
}
