using Microsoft.AspNetCore.Mvc;
using ConsoleApp1.Services;
using ConsoleApp1.Models;

namespace ConsoleApp1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerService _customerService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(CustomerService customerService, ILogger<CustomerController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        /// <summary>
        /// 获取客户基本信息
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <returns>客户基本信息</returns>
        [HttpGet("{customerId}")]
        public async Task<ActionResult<CustomerProfileInfo>> GetCustomerProfile(int customerId)
        {
            try
        {
                var customerInfo = await _customerService.GetCustomerProfileAsync(customerId);
                if (customerInfo == null)
                {
                    return NotFound(new { message = "客户不存在" });
                }

                return Ok(customerInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取客户 {customerId} 基本信息失败");
                return StatusCode(500, new { message = "获取客户信息失败", error = ex.Message });
            }
        }

        /// <summary>
        /// 更新客户基本信息
        /// </summary>
        /// <param name="customerId">客户ID</param>
        /// <param name="updateInfo">更新信息</param>
        /// <returns>更新结果</returns>
        [HttpPut("{customerId}")]
        public async Task<ActionResult> UpdateCustomerProfile(int customerId, [FromBody] CustomerUpdateInfo updateInfo)
        {
            try
            {
                var success = await _customerService.UpdateCustomerProfileAsync(customerId, updateInfo);
                if (!success)
                {
                    return NotFound(new { message = "客户不存在" });
                }

                return Ok(new { message = "客户信息更新成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新客户 {customerId} 信息失败");
                return StatusCode(500, new { message = "更新客户信息失败", error = ex.Message });
            }
        }
    }

    /// <summary>
    /// 客户档案信息响应模型
    /// </summary>
    public class CustomerProfileInfo
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? VipLevelName { get; set; }
        public DateTime RegisterTime { get; set; }
    }

    /// <summary>
    /// 客户更新信息请求模型
    /// </summary>
    public class CustomerUpdateInfo
    {
        public string CustomerName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }
}
