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
        /// �����ͻ�����
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<object>> CreateReview([FromBody] CreateReviewRequest request)
        {
            try
            {
                _logger.LogInformation($"������������: ����ID={request.OrderID}, �ͻ�ID={request.CustomerID}, ����={request.OverallRating}");

                var review = new CustomerReview
                {
                    CustomerID = request.CustomerID,
                    OrderID = request.OrderID,
                    StoreID = request.StoreID,
                    OverallRating = request.OverallRating,
                    Comment = request.Comment,
                    ReviewTime = DateTime.Now,
                    Status = "�����"
                };

                var reviewId = await _reviewService.CreateReviewAsync(review);

                return Ok(new
                {
                    success = true,
                    message = "�����ύ�ɹ�",
                    reviewId = reviewId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "��������ʧ��");
                return StatusCode(500, new
                {
                    success = false,
                    message = "�����ύʧ��",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// ���ݶ���ID��ȡ����
        /// </summary>
        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<CustomerReview>> GetReviewByOrderId(int orderId)
        {
            try
            {
                _logger.LogInformation($"��ȡ���� {orderId} ������");
                var review = await _reviewService.GetReviewByOrderIdAsync(orderId);

                if (review == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "δ�ҵ��ö���������"
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
                _logger.LogError(ex, $"��ȡ���� {orderId} ������ʧ��");
                return StatusCode(500, new
                {
                    success = false,
                    message = "��ȡ����ʧ��",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// ���ݿͻ�ID��ȡ��������
        /// </summary>
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<List<CustomerReview>>> GetReviewsByCustomerId(int customerId)
        {
            try
            {
                _logger.LogInformation($"��ȡ�ͻ� {customerId} ����������");
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
                _logger.LogError(ex, $"��ȡ�ͻ� {customerId} ������ʧ��");
                return StatusCode(500, new
                {
                    success = false,
                    message = "��ȡ����ʧ��",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// �������۹���
        /// </summary>
        [HttpPost("test")]
        public async Task<ActionResult<object>> TestReview()
        {
            try
            {
                _logger.LogInformation("��ʼ�������۹���");
                var result = await _reviewService.TestCreateReviewAsync();

                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "���۹��ܲ��Գɹ�"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "���۹��ܲ���ʧ��"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "���۹��ܲ���ʧ��");
                return StatusCode(500, new
                {
                    success = false,
                    message = "���۹��ܲ���ʧ��",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// �򵥵����Ӳ���
        /// </summary>
        [HttpGet("ping")]
        public ActionResult<object> Ping()
        {
            _logger.LogInformation("����API���Ӳ���");
            return Ok(new
            {
                success = true,
                message = "����API��������",
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
    }

    /// <summary>
    /// ������������ģ��
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
