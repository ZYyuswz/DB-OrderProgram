using Microsoft.AspNetCore.Mvc;
using ConsoleApp1.Models;
using ConsoleApp1.Services;
using Microsoft.Extensions.Logging;

namespace ConsoleApp1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly ReviewService _reviewService;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(ReviewService reviewService, ILogger<ReviewController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        /// <summary>
        /// 创建客户评价
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<object>> CreateReview([FromBody] CreateReviewRequest request)
        {
            try
            {
                _logger.LogInformation($"创建评价请求: 订单ID={request.OrderID}, 客户ID={request.CustomerID}, 评分={request.OverallRating}");

                var review = new CustomerReview
                {
                    CustomerID = request.CustomerID,
                    OrderID = request.OrderID,
                    StoreID = request.StoreID,
                    OverallRating = request.OverallRating,
                    Comment = request.Comment,
                    ReviewTime = DateTime.Now,
                    Status = "待审核"
                };

                var reviewId = await _reviewService.CreateReviewAsync(review);

                return Ok(new
                {
                    success = true,
                    message = "评价提交成功",
                    reviewId = reviewId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建评价失败");
                return StatusCode(500, new
                {
                    success = false,
                    message = "评价提交失败",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 根据订单ID获取评价
        /// </summary>
        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<CustomerReview>> GetReviewByOrderId(int orderId)
        {
            try
            {
                _logger.LogInformation($"获取订单 {orderId} 的评价");
                var review = await _reviewService.GetReviewByOrderIdAsync(orderId);

                if (review == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "未找到该订单的评价"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = review
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取订单 {orderId} 的评价失败");
                return StatusCode(500, new
                {
                    success = false,
                    message = "获取评价失败",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 根据客户ID获取所有评价
        /// </summary>
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<List<CustomerReview>>> GetReviewsByCustomerId(int customerId)
        {
            try
            {
                _logger.LogInformation($"获取客户 {customerId} 的所有评价");
                var reviews = await _reviewService.GetReviewsByCustomerIdAsync(customerId);

                return Ok(new
                {
                    success = true,
                    data = reviews,
                    count = reviews.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取客户 {customerId} 的评价失败");
                return StatusCode(500, new
                {
                    success = false,
                    message = "获取评价失败",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 测试评价功能
        /// </summary>
        [HttpPost("test")]
        public async Task<ActionResult<object>> TestReview()
        {
            try
            {
                _logger.LogInformation("开始测试评价功能");
                var result = await _reviewService.TestCreateReviewAsync();

                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "评价功能测试成功"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "评价功能测试失败"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "评价功能测试失败");
                return StatusCode(500, new
                {
                    success = false,
                    message = "评价功能测试失败",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// 简单的连接测试
        /// </summary>
        [HttpGet("ping")]
        public ActionResult<object> Ping()
        {
            _logger.LogInformation("评价API连接测试");
            return Ok(new
            {
                success = true,
                message = "评价API连接正常",
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
    }

    /// <summary>
    /// 创建评价请求模型
    /// </summary>
    public class CreateReviewRequest
    {
        public int CustomerID { get; set; }
        public int OrderID { get; set; }
        public int StoreID { get; set; }
        public int OverallRating { get; set; }
        public string? Comment { get; set; }
    }
}
