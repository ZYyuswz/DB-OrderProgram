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

        [HttpGet]
        public async Task<IActionResult> GetAllStaff()
        {
            var staff = await _staffService.GetAllStaffAsync();
            return Ok(staff);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStaff(int id)
        {
            var staff = await _staffService.GetStaffByIdAsync(id);
            if (staff == null)
                return NotFound();
            return Ok(staff);
        }

        [HttpPost]
        public async Task<IActionResult> AddStaff([FromBody] Staff staff)
        {
            staff.HireDate = DateTime.Now;
            staff.Status = "在职";
            var result = await _staffService.AddStaffAsync(staff);
            if (!result)
                return BadRequest();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStaff(int id, [FromBody] Staff staff)
        {
            staff.StaffID = id;
            var result = await _staffService.UpdateStaffAsync(staff);
            if (!result)
                return BadRequest();
            return Ok();
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStaffStatus(int id, [FromBody] string status)
        {
            var result = await _staffService.UpdateStaffStatusAsync(id, status);
            if (!result)
                return BadRequest();
            return Ok();
        }

        [HttpGet("attendance")]
        public async Task<IActionResult> GetAttendanceRecords(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? staffId)
        {
            var records = await _staffService.GetAttendanceRecordsAsync(startDate, endDate, staffId);
            return Ok(records);
        }

        [HttpPost("{id}/checkin")]
        public async Task<IActionResult> CheckIn(int id)
        {
            var result = await _staffService.CheckInAsync(id);
            if (!result)
                return BadRequest();
            return Ok();
        }

        [HttpPost("{id}/checkout")]
        public async Task<IActionResult> CheckOut(int id)
        {
            var result = await _staffService.CheckOutAsync(id);
            if (!result)
                return BadRequest();
            return Ok();
        }
    }
}
