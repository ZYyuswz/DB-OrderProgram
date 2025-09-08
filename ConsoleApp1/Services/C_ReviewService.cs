using ConsoleApp1.Models;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace ConsoleApp1.Services
{
    public class ReviewService
    {
        private readonly string _connectionString;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(IConfiguration configuration, ILogger<ReviewService> logger)
        {
            _connectionString = configuration.GetConnectionString("OracleConnection");
            _logger = logger;
        }

        /// <summary>
        /// �����ͻ�����
        /// </summary>
        public async Task<int> CreateReviewAsync(CustomerReview review)
        {
            try
            {
                _logger.LogInformation($"��ʼ�������ۣ�����: CustomerID={review.CustomerID}, OrderID={review.OrderID}, StoreID={review.StoreID}, Rating={review.OverallRating}");

                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();

                _logger.LogInformation("���ݿ������ѽ���");

                var sql = @"INSERT INTO PUB.CUSTOMERREVIEW 
                           (REVIEWID, CUSTOMERID, ORDERID, STOREID, OVERALLRATING, REVIEWCOMMENT, REVIEWTIME, STATUS) 
                           VALUES 
                           (PUB.seq_order_id.NEXTVAL, :CUSTOMERID, :ORDERID, :STOREID, :OVERALLRATING, :REVIEWCOMMENT, :REVIEWTIME, :STATUS)";

                _logger.LogInformation($"ִ��SQL: {sql}");
                _logger.LogInformation($"����ֵ: CUSTOMERID={review.CustomerID}, ORDERID={review.OrderID}, STOREID={review.StoreID}, OVERALLRATING={review.OverallRating}, COMMENT='{review.Comment}', REVIEWTIME={review.ReviewTime}, STATUS='{review.Status}'");

                using var command = new OracleCommand(sql, connection);
                command.Parameters.Add(":CUSTOMERID", OracleDbType.Int32).Value = review.CustomerID;
                command.Parameters.Add(":ORDERID", OracleDbType.Int32).Value = review.OrderID;
                command.Parameters.Add(":STOREID", OracleDbType.Int32).Value = review.StoreID;
                command.Parameters.Add(":OVERALLRATING", OracleDbType.Int32).Value = review.OverallRating;
                command.Parameters.Add(":REVIEWCOMMENT", OracleDbType.Varchar2).Value = review.Comment ?? "";
                command.Parameters.Add(":REVIEWTIME", OracleDbType.Date).Value = review.ReviewTime;
                command.Parameters.Add(":STATUS", OracleDbType.Varchar2).Value = review.Status;

                await command.ExecuteNonQueryAsync();

                // ��ȡ�ղ��������ID��ͨ����ѯ������ļ�¼��
                var getLastIdSql = "SELECT PUB.seq_order_id.CURRVAL FROM DUAL";
                using var getIdCommand = new OracleCommand(getLastIdSql, connection);
                var reviewId = Convert.ToInt32(await getIdCommand.ExecuteScalarAsync());

                _logger.LogInformation($"�ɹ��������ۣ�����ID: {reviewId}");
                return reviewId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "��������ʧ��");
                throw;
            }
        }

        /// <summary>
        /// ���ݶ���ID��ȡ����
        /// </summary>
        public async Task<CustomerReview?> GetReviewByOrderIdAsync(int orderId)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"SELECT r.REVIEWID, r.CUSTOMERID, r.ORDERID, r.STOREID, r.OVERALLRATING, 
                                  r.REVIEWCOMMENT, r.REVIEWTIME, r.STATUS,
                                  c.CUSTOMERNAME, s.STORENAME, o.ORDERTIME
                           FROM PUB.CUSTOMERREVIEW r
                           LEFT JOIN PUB.CUSTOMER c ON r.CUSTOMERID = c.CUSTOMERID
                           LEFT JOIN PUB.STORE s ON r.STOREID = s.STOREID
                           LEFT JOIN PUB.ORDERS o ON r.ORDERID = o.ORDERID
                           WHERE r.ORDERID = :orderId";

                using var command = new OracleCommand(sql, connection);
                command.Parameters.Add(":orderId", OracleDbType.Int32).Value = orderId;

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new CustomerReview
                    {
                        ReviewID = reader.GetInt32(reader.GetOrdinal("REVIEWID")),
                        CustomerID = reader.GetInt32(reader.GetOrdinal("CUSTOMERID")),
                        OrderID = reader.GetInt32(reader.GetOrdinal("ORDERID")),
                        StoreID = reader.GetInt32(reader.GetOrdinal("STOREID")),
                        OverallRating = reader.GetInt32(reader.GetOrdinal("OVERALLRATING")),
                        Comment = reader.IsDBNull(reader.GetOrdinal("REVIEWCOMMENT")) ? null : reader.GetString(reader.GetOrdinal("REVIEWCOMMENT")),
                        ReviewTime = reader.GetDateTime(reader.GetOrdinal("REVIEWTIME")),
                        Status = reader.GetString(reader.GetOrdinal("STATUS")),
                        CustomerName = reader.IsDBNull(reader.GetOrdinal("CUSTOMERNAME")) ? null : reader.GetString(reader.GetOrdinal("CUSTOMERNAME")),
                        StoreName = reader.IsDBNull(reader.GetOrdinal("STORENAME")) ? null : reader.GetString(reader.GetOrdinal("STORENAME")),
                        OrderTime = reader.IsDBNull(reader.GetOrdinal("ORDERTIME")) ? null : reader.GetDateTime(reader.GetOrdinal("ORDERTIME")).ToString("yyyy-MM-dd HH:mm:ss")
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"��ȡ���� {orderId} ������ʧ��");
                throw;
            }
        }

        /// <summary>
        /// ���ݿͻ�ID��ȡ��������
        /// </summary>
        public async Task<List<CustomerReview>> GetReviewsByCustomerIdAsync(int customerId)
        {
            try
            {
                var reviews = new List<CustomerReview>();
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"SELECT r.REVIEWID, r.CUSTOMERID, r.ORDERID, r.STOREID, r.OVERALLRATING, 
                                  r.REVIEWCOMMENT, r.REVIEWTIME, r.STATUS,
                                  c.CUSTOMERNAME, s.STORENAME, o.ORDERTIME
                           FROM PUB.CUSTOMERREVIEW r
                           LEFT JOIN PUB.CUSTOMER c ON r.CUSTOMERID = c.CUSTOMERID
                           LEFT JOIN PUB.STORE s ON r.STOREID = s.STOREID
                           LEFT JOIN PUB.ORDERS o ON r.ORDERID = o.ORDERID
                           WHERE r.CUSTOMERID = :customerId
                           ORDER BY r.REVIEWTIME DESC";

                using var command = new OracleCommand(sql, connection);
                command.Parameters.Add(":customerId", OracleDbType.Int32).Value = customerId;

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    reviews.Add(new CustomerReview
                    {
                        ReviewID = reader.GetInt32(reader.GetOrdinal("REVIEWID")),
                        CustomerID = reader.GetInt32(reader.GetOrdinal("CUSTOMERID")),
                        OrderID = reader.GetInt32(reader.GetOrdinal("ORDERID")),
                        StoreID = reader.GetInt32(reader.GetOrdinal("STOREID")),
                        OverallRating = reader.GetInt32(reader.GetOrdinal("OVERALLRATING")),
                        Comment = reader.IsDBNull(reader.GetOrdinal("REVIEWCOMMENT")) ? null : reader.GetString(reader.GetOrdinal("REVIEWCOMMENT")),
                        ReviewTime = reader.GetDateTime(reader.GetOrdinal("REVIEWTIME")),
                        Status = reader.GetString(reader.GetOrdinal("STATUS")),
                        CustomerName = reader.IsDBNull(reader.GetOrdinal("CUSTOMERNAME")) ? null : reader.GetString(reader.GetOrdinal("CUSTOMERNAME")),
                        StoreName = reader.IsDBNull(reader.GetOrdinal("STORENAME")) ? null : reader.GetString(reader.GetOrdinal("STORENAME")),
                        OrderTime = reader.IsDBNull(reader.GetOrdinal("ORDERTIME")) ? null : reader.GetDateTime(reader.GetOrdinal("ORDERTIME")).ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }

                _logger.LogInformation($"�ɹ���ѯ���ͻ� {customerId} �� {reviews.Count} ������");
                return reviews;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"��ѯ�ͻ� {customerId} ������ʧ��");
                throw;
            }
        }

        /// <summary>
        /// ���Դ������۹���
        /// </summary>
        public async Task<bool> TestCreateReviewAsync()
        {
            try
            {
                var testReview = new CustomerReview
                {
                    CustomerID = 1,
                    OrderID = 1,
                    StoreID = 1,
                    OverallRating = 5,
                    Comment = "�������� - ����ܺã���Ʒ��ζ��",
                    ReviewTime = DateTime.Now,
                    Status = "�����"
                };

                var reviewId = await CreateReviewAsync(testReview);
                _logger.LogInformation($"���Դ������۳ɹ�������ID: {reviewId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "���Դ�������ʧ��");
                return false;
            }
        }
    }
}
