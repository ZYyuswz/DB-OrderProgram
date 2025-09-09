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

                return Ok(new { 
                    success = true, 
                    message = "评价提交成功", 
                    reviewId = reviewId 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建评价失败");
                return StatusCode(500, new { 
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
                    return NotFound(new { 
                        success = false, 
                        message = "未找到该订单的评价" 
                    });
                }

                return Ok(new { 
                    success = true, 
                    data = review 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取订单 {orderId} 的评价失败");
                return StatusCode(500, new { 
                    success = false, 
                    message = "获取评价失败", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// 根据客户ID获取所有评价（分页）
        /// </summary>
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<List<CustomerReview>>> GetReviewsByCustomerId(
            int customerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation($"获取客户 {customerId} 的所有评价，页码: {page}, 每页: {pageSize}");
                
                var reviews = await _reviewService.GetReviewsByCustomerIdAsync(customerId);
                
                // 分页处理
                var totalCount = reviews.Count;
                var pagedReviews = reviews
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // 计算统计信息
                var stats = CalculateReviewStats(reviews);

                return Ok(new { 
                    success = true, 
                    data = pagedReviews,
                    totalCount = totalCount,
                    currentPage = page,
                    totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    stats = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取客户 {customerId} 的评价失败");
                return StatusCode(500, new { 
                    success = false, 
                    message = "获取评价失败", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// 获取评价统计信息
        /// </summary>
        [HttpGet("customer/{customerId}/stats")]
        public async Task<ActionResult<object>> GetReviewStats(int customerId)
        {
            try
            {
                _logger.LogInformation($"获取客户 {customerId} 的评价统计");
                
                var reviews = await _reviewService.GetReviewsByCustomerIdAsync(customerId);
                var stats = CalculateReviewStats(reviews);

                return Ok(new { 
                    success = true, 
                    data = stats 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取客户 {customerId} 的评价统计失败");
                return StatusCode(500, new { 
                    success = false, 
                    message = "获取统计失败", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// 更新评价点赞数
        /// </summary>
        [HttpPost("{reviewId}/helpful")]
        public async Task<ActionResult<object>> MarkReviewAsHelpful(int reviewId, [FromBody] HelpfulRequest request)
        {
            try
            {
                _logger.LogInformation($"更新评价 {reviewId} 的点赞状态，操作: {(request.IsHelpful ? "点赞" : "取消点赞")}");

                var success = await _reviewService.UpdateReviewHelpfulCountAsync(reviewId, request.IsHelpful);
                
                if (success)
                {
                    return Ok(new { 
                        success = true, 
                        message = request.IsHelpful ? "点赞成功" : "取消点赞成功" 
                    });
                }
                else
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "更新点赞状态失败" 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新评价 {reviewId} 点赞状态失败");
                return StatusCode(500, new { 
                    success = false, 
                    message = "更新点赞状态失败", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// 删除评价
        /// </summary>
        [HttpDelete("{reviewId}")]
        public async Task<ActionResult<object>> DeleteReview(int reviewId)
        {
            try
            {
                _logger.LogInformation($"删除评价 {reviewId}");

                var success = await _reviewService.DeleteReviewAsync(reviewId);
                
                if (success)
                {
                    return Ok(new { 
                        success = true, 
                        message = "评价删除成功" 
                    });
                }
                else
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "评价删除失败" 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除评价 {reviewId} 失败");
                return StatusCode(500, new { 
                    success = false, 
                    message = "删除评价失败", 
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
                    return Ok(new { 
                        success = true, 
                        message = "评价功能测试成功" 
                    });
                }
                else
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "评价功能测试失败" 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "评价功能测试失败");
                return StatusCode(500, new { 
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
            return Ok(new { 
                success = true, 
                message = "评价API连接正常", 
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        #region 私有方法

        /// <summary>
        /// 计算评价统计信息
        /// </summary>
        private object CalculateReviewStats(List<CustomerReview> reviews)
        {
            if (reviews == null || reviews.Count == 0)
            {
                return new
                {
                    totalReviews = 0,
                    averageRating = 0,
                    helpfulCount = 0,
                    ratingDistribution = new { fiveStar = 0, fourStar = 0, threeStar = 0, twoStar = 0, oneStar = 0 }
                };
            }

            var totalRating = reviews.Sum(r => r.OverallRating);
            var averageRating = Math.Round(totalRating / (double)reviews.Count, 1);

            // 模拟获赞数（实际应该从数据库获取）
            var random = new Random();
            var helpfulCount = reviews.Sum(r => random.Next(0, 10));

            // 评分分布
            var ratingDistribution = new
            {
                fiveStar = reviews.Count(r => r.OverallRating == 5),
                fourStar = reviews.Count(r => r.OverallRating == 4),
                threeStar = reviews.Count(r => r.OverallRating == 3),
                twoStar = reviews.Count(r => r.OverallRating == 2),
                oneStar = reviews.Count(r => r.OverallRating == 1)
            };

            return new
            {
                totalReviews = reviews.Count,
                averageRating = averageRating,
                helpfulCount = helpfulCount,
                ratingDistribution = ratingDistribution
            };
        }

        #endregion
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

    /// <summary>
    /// 点赞请求模型
    /// </summary>
    public class HelpfulRequest
    {
        public bool IsHelpful { get; set; }
    }
}