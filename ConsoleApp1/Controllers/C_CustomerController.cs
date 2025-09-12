using Microsoft.AspNetCore.Mvc;
using ConsoleApp1.Services;
using ConsoleApp1.Models;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;

namespace ConsoleApp1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(DatabaseService databaseService, ILogger<CustomerController> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        /// <summary>
        /// 客户注册
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<object>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                _logger.LogInformation($"客户注册请求: 手机号={request.Phone}");

                // 1. 验证手机号是否已注册
                if (await IsPhoneRegisteredAsync(request.Phone))
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "该手机号已注册" 
                    });
                }

                // 2. 验证密码确认
                if (request.Password != request.ConfirmPassword)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "两次输入的密码不一致" 
                    });
                }

                // 3. 验证密码长度
                if (request.Password.Length < 6)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "密码长度至少6位" 
                    });
                }

                // 4. 创建客户记录
                var customerId = await CreateCustomerAsync(request);
                
                if (customerId > 0)
                {
                    _logger.LogInformation($"客户注册成功: ID={customerId}, 手机号={request.Phone}");
                    
                    return Ok(new { 
                        success = true, 
                        message = "注册成功", 
                        data = new { customerId = customerId }
                    });
                }
                else
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "注册失败，请重试" 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "客户注册失败");
                return StatusCode(500, new { 
                    success = false, 
                    message = "注册失败，系统错误", 
                    error = ex.Message 
                });
            }
        }

        /// <summary>
        /// 客户登录
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<object>> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation($"客户登录请求: 手机号={request.Phone}");

                // 验证用户 credentials
                var customer = await ValidateCredentialsAsync(request.Phone, request.Password);
                
                if (customer == null)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "手机号或密码错误" 
                    });
                }

                // 返回用户信息（简化版，不含token）
                return Ok(new { 
                    success = true, 
                    message = "登录成功",
                    data = new {
                        userInfo = new {
                            customerId = customer.CustomerID,
                            nickname = customer.CustomerName,
                            phone = customer.Phone,
                            memberLevel = "普通会员",
                            points = customer.VIPPoints,
                            totalConsumption = customer.TotalConsumption
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "客户登录失败");
                return StatusCode(500, new { 
                    success = false, 
                    message = "登录失败，系统错误" 
                });
            }
        }

        /// <summary>
        /// 检查手机号是否已注册
        /// </summary>
        private async Task<bool> IsPhoneRegisteredAsync(string phone)
        {
            try
            {
                using var connection = new OracleConnection(_databaseService.GetConnectionString());
                await connection.OpenAsync();
                
                var sql = "SELECT COUNT(*) FROM PUB.Customer WHERE Phone = :phone";
                
                using var command = new OracleCommand(sql, connection);
                command.Parameters.Add(":phone", OracleDbType.Varchar2).Value = phone;
                
                var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查手机号是否注册失败");
                throw;
            }
        }

        /// <summary>
        /// 创建客户记录
        /// </summary>
        private async Task<int> CreateCustomerAsync(RegisterRequest request)
        {
            try
            {
                using var connection = new OracleConnection(_databaseService.GetConnectionString());
                await connection.OpenAsync();
                
                var sql = @"INSERT INTO PUB.Customer (
                           CustomerID, CustomerName, Phone, Password, 
                           RegisterTime, TotalConsumption, VIPPoints, Status)
                           VALUES (
                           PUB.seq_customer_id.NEXTVAL, :CustomerName, :Phone, :Password,
                           SYSDATE, 0, 0, '正常')
                           RETURNING CustomerID INTO :CustomerID";
                
                using var command = new OracleCommand(sql, connection);
                
                // 生成默认用户名
                var customerName = "用户" + request.Phone.Substring(7);
                
                command.Parameters.Add(":CustomerName", OracleDbType.Varchar2).Value = customerName;
                command.Parameters.Add(":Phone", OracleDbType.Varchar2).Value = request.Phone;
                command.Parameters.Add(":Password", OracleDbType.Varchar2).Value = HashPassword(request.Password);
                
                var customerIdParam = new OracleParameter(":CustomerID", OracleDbType.Int32)
                {
                    Direction = System.Data.ParameterDirection.Output
                };
                command.Parameters.Add(customerIdParam);
                
                await command.ExecuteNonQueryAsync();
                
                var oracleDecimal = (Oracle.ManagedDataAccess.Types.OracleDecimal)customerIdParam.Value;
                return oracleDecimal.ToInt32();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建客户记录失败");
                return -1;
            }
        }

        /// <summary>
        /// 验证用户凭证
        /// </summary>
        private async Task<Customer> ValidateCredentialsAsync(string phone, string password)
        {
            try
            {
                using var connection = new OracleConnection(_databaseService.GetConnectionString());
                await connection.OpenAsync();
                
                var sql = @"SELECT CustomerID, CustomerName, Phone, Password, 
                                  VIPPoints, TotalConsumption 
                           FROM PUB.Customer 
                           WHERE Phone = :phone AND Password = :password";
                
                using var command = new OracleCommand(sql, connection);
                command.Parameters.Add(":phone", OracleDbType.Varchar2).Value = phone;
                command.Parameters.Add(":password", OracleDbType.Varchar2).Value = HashPassword(password);
                
                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    return new Customer
                    {
                        CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                        CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                        Phone = reader.GetString(reader.GetOrdinal("Phone")),
                        VIPPoints = reader.GetInt32(reader.GetOrdinal("VIPPoints")),
                        TotalConsumption = reader.GetDecimal(reader.GetOrdinal("TotalConsumption"))
                    };
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "验证用户凭证失败");
                throw;
            }
        }

        /// <summary>
        /// 密码加密（简单实现）
        /// </summary>
        private string HashPassword(string password)
        {
            // 实际项目中应该使用更安全的加密方式
            // 这里使用简单的Base64编码，生产环境请使用BCrypt等
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }

/// <summary>
/// 重置密码请求模型
/// </summary>
public class ResetPasswordRequest
{
    public string Username { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// 重置密码
/// </summary>
[HttpPost("reset-password")]
public async Task<ActionResult<object>> ResetPassword([FromBody] ResetPasswordRequest request)
{
    try
    {
        _logger.LogInformation($"重置密码请求: 用户名={request.Username}, 手机号={request.Phone}");

        // 1. 验证密码确认
        if (request.NewPassword != request.ConfirmPassword)
        {
            return BadRequest(new { 
                success = false, 
                message = "两次输入的密码不一致" 
            });
        }

        // 2. 验证密码长度
        if (request.NewPassword.Length < 6)
        {
            return BadRequest(new { 
                success = false, 
                message = "密码长度至少6位" 
            });
        }

        // 3. 验证用户名和手机号是否匹配
        var customer = await GetCustomerByUsernameAndPhoneAsync(request.Username, request.Phone);
        
        if (customer == null)
        {
            return BadRequest(new { 
                success = false, 
                message = "用户名和手机号不匹配" 
            });
        }

        // 4. 更新密码
        var success = await UpdateCustomerPasswordAsync(customer.CustomerID, request.NewPassword);
        
        if (success)
        {
            _logger.LogInformation($"密码重置成功: 用户ID={customer.CustomerID}");
            
            return Ok(new { 
                success = true, 
                message = "密码重置成功" 
            });
        }
        else
        {
            return BadRequest(new { 
                success = false, 
                message = "密码重置失败，请重试" 
            });
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "重置密码失败");
        return StatusCode(500, new { 
            success = false, 
            message = "重置密码失败，系统错误" 
        });
    }
}

/// <summary>
/// 根据用户名和手机号获取客户信息
/// </summary>
private async Task<Customer> GetCustomerByUsernameAndPhoneAsync(string username, string phone)
{
    try
    {
        using var connection = new OracleConnection(_databaseService.GetConnectionString());
        await connection.OpenAsync();
        
        // 假设用户名存储在CustomerName字段中
        var sql = @"SELECT CustomerID, CustomerName, Phone 
                   FROM PUB.Customer 
                   WHERE CustomerName = :username AND Phone = :phone";
        
        using var command = new OracleCommand(sql, connection);
        command.Parameters.Add(":username", OracleDbType.Varchar2).Value = username;
        command.Parameters.Add(":phone", OracleDbType.Varchar2).Value = phone;
        
        using var reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return new Customer
            {
                CustomerID = reader.GetInt32(reader.GetOrdinal("CustomerID")),
                CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                Phone = reader.GetString(reader.GetOrdinal("Phone"))
            };
        }
        
        return null;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "根据用户名和手机号查询客户失败");
        throw;
    }
}

/// <summary>
/// 更新客户密码
/// </summary>
private async Task<bool> UpdateCustomerPasswordAsync(int customerId, string newPassword)
{
    try
    {
        using var connection = new OracleConnection(_databaseService.GetConnectionString());
        await connection.OpenAsync();
        
        var sql = @"UPDATE PUB.Customer 
                   SET Password = :password, UpdateTime = SYSDATE 
                   WHERE CustomerID = :customerId";
        
        using var command = new OracleCommand(sql, connection);
        command.Parameters.Add(":password", OracleDbType.Varchar2).Value = HashPassword(newPassword);
        command.Parameters.Add(":customerId", OracleDbType.Int32).Value = customerId;
        
        int rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"更新客户 {customerId} 密码失败");
        return false;
    }
}
    }
}