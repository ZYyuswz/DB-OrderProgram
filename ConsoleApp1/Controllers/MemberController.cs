using Microsoft.AspNetCore.Mvc;
using ConsoleApp1.Services;
using ConsoleApp1.Models;

namespace ConsoleApp1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MemberController : ControllerBase
    {
        private readonly MemberService _memberService;
        private readonly ILogger<MemberController> _logger;

        public MemberController(MemberService memberService, ILogger<MemberController> logger)
        {
            _memberService = memberService;
            _logger = logger;
        }

        /// <summary>
        /// 获取客户的会员信息
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <returns>会员信息</returns>
        [HttpGet("customer/{customerId}/info")]
        public async Task<ActionResult<MemberInfo>> GetCustomerMemberInfo(int customerId)
        {
            try
            {
                _logger.LogInformation($"获取客户 {customerId} 的会员信息");
                
                var memberInfo = await _memberService.GetCustomerMemberInfoAsync(customerId);
                
                if (memberInfo == null)
                {
                    return NotFound(new { message = "客户不存在" });
                }
                
                return Ok(memberInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取客户 {customerId} 会员信息失败");
                return StatusCode(500, new { message = "获取会员信息失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 获取所有会员等级规则
        /// </summary>
        /// <returns>会员等级规则列表</returns>
        [HttpGet("levels")]
        public async Task<ActionResult<List<MemberLevel>>> GetMemberLevels()
        {
            try
            {
                _logger.LogInformation("获取会员等级规则");
                
                var levels = await _memberService.GetMemberLevelsAsync();
                
                return Ok(levels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取会员等级规则失败");
                return StatusCode(500, new { message = "获取会员等级规则失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 更新客户累计消费金额
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <returns>更新结果</returns>
        [HttpPost("customer/{customerId}/update-consumption")]
        public async Task<ActionResult> UpdateCustomerTotalConsumption(int customerId)
        {
            try
            {
                _logger.LogInformation($"更新客户 {customerId} 的累计消费金额");
                
                var success = await _memberService.UpdateCustomerTotalConsumptionAsync(customerId);
                
                if (success)
                {
                    return Ok(new { message = "累计消费金额更新成功" });
                }
                else
                {
                    return BadRequest(new { message = "累计消费金额更新失败" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新客户 {customerId} 累计消费金额失败");
                return StatusCode(500, new { message = "更新累计消费金额失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 更新客户会员等级
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <returns>更新结果</returns>
        [HttpPost("customer/{customerId}/update-level")]
        public async Task<ActionResult> UpdateCustomerMemberLevel(int customerId)
        {
            try
            {
                _logger.LogInformation($"更新客户 {customerId} 的会员等级");
                
                var success = await _memberService.UpdateCustomerMemberLevelAsync(customerId);
                
                if (success)
                {
                    return Ok(new { message = "会员等级更新成功" });
                }
                else
                {
                    return BadRequest(new { message = "会员等级更新失败" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新客户 {customerId} 会员等级失败");
                return StatusCode(500, new { message = "更新会员等级失败", error = ex.Message });
            }
        }



        /// <summary>
        /// 获取客户的消费记录统计
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <returns>消费统计信息</returns>
        [HttpGet("customer/{customerId}/consumption-stats")]
        public async Task<ActionResult<ConsumptionStats>> GetCustomerConsumptionStats(int customerId)
        {
            try
            {
                _logger.LogInformation($"获取客户 {customerId} 的消费统计");
                
                var stats = await _memberService.GetCustomerConsumptionStatsAsync(customerId);
                
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取客户 {customerId} 消费统计失败");
                return StatusCode(500, new { message = "获取消费统计失败", error = ex.Message });
            }
        }
    }

    /// <summary>
    /// 会员信息响应模型
    /// </summary>
    public class MemberInfo
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalConsumption { get; set; }
        public string CurrentLevel { get; set; } = string.Empty;
        public string CurrentLevelName { get; set; } = string.Empty;
        public string NextLevel { get; set; } = string.Empty;
        public string NextLevelName { get; set; } = string.Empty;
        public decimal NextLevelThreshold { get; set; }
        public decimal ProgressToNextLevel { get; set; }
        public int VipPoints { get; set; }
        public DateTime RegisterTime { get; set; }
        public List<MemberPrivilege> Privileges { get; set; } = new List<MemberPrivilege>();
    }

    /// <summary>
    /// 会员等级模型
    /// </summary>
    public class MemberLevel
    {
        public string LevelCode { get; set; } = string.Empty;
        public string LevelName { get; set; } = string.Empty;
        public decimal MinConsumption { get; set; }
        public decimal? MaxConsumption { get; set; }
        public string LevelColor { get; set; } = string.Empty;
        public string LevelIcon { get; set; } = string.Empty;
        public List<MemberPrivilege> Privileges { get; set; } = new List<MemberPrivilege>();
    }

    /// <summary>
    /// 会员特权模型
    /// </summary>
    public class MemberPrivilege
    {
        public string PrivilegeType { get; set; } = string.Empty;
        public string PrivilegeName { get; set; } = string.Empty;
        public string PrivilegeDesc { get; set; } = string.Empty;
        public string PrivilegeValue { get; set; } = string.Empty;
        public string PrivilegeIcon { get; set; } = string.Empty;
    }

    /// <summary>
    /// 消费统计模型
    /// </summary>
    public class ConsumptionStats
    {
        public int CustomerId { get; set; }
        public decimal TotalConsumption { get; set; }
        public int TotalOrders { get; set; }
        public decimal MonthlyConsumption { get; set; }
        public int MonthlyOrders { get; set; }
        public decimal AverageOrderAmount { get; set; }
        public DateTime LastOrderTime { get; set; }
        public string FavoriteStore { get; set; } = string.Empty;
    }
}