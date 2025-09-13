using Microsoft.AspNetCore.Mvc;
using ConsoleApp1.Services;
using ConsoleApp1.Models;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace ConsoleApp1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<CustomerController> _logger;
        private readonly CustomerService _customerService;

        public CustomerController(DatabaseService databaseService, 
                                ILogger<CustomerController> logger,
                                CustomerService customerService)
        {
            _databaseService = databaseService;
            _logger = logger;
            _customerService = customerService;
        }

        #region 客户档案管理

        /// <summary>
        /// 获取客户基本信息
        /// </summary>
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
        [HttpPut("{customerId}")]
        public async Task<ActionResult> UpdateCustomerProfile(int customerId, [FromBody] CustomerUpdateInfo updateInfo)
        {
            try
            {
                var success = await _customerService.UpdateCustomerProfileAsync(customerId, updateInfo);
                if (success == 0)
                {
                    return NotFound(new { 
                        success = false, 
                        message = "客户不存在" 
                        });
                }

                return Ok(new { 
                    success = true, 
                    message = "客户信息更新成功" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新客户 {customerId} 信息失败");
                return StatusCode(500, new { 
                    success = false, 
                    message = "更新客户信息失败", 
                    error = ex.Message 
                });
            }
        }

        #endregion

        #region 注册、登录、忘记密码

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
                var isPhoneRegistered = await IsPhoneRegisteredAsync(request.Phone);
                if (isPhoneRegistered)
                {
                    _logger.LogWarning($"手机号 {request.Phone} 已被注册");
                    return BadRequest(new { 
                        success = false, 
                        message = "该手机号已被注册，请使用其他手机号或尝试登录" 
                    });
                }

                // 2. 验证用户名是否已存在（防止生成的默认用户名重复）
                var defaultUsername = "用户" + request.Phone.Substring(request.Phone.Length - 4);
                var isUsernameExists = await IsUsernameExistsAsync(defaultUsername);
                if (isUsernameExists)
                {
                    // 如果默认用户名已存在，添加随机后缀
                    var random = new Random();
                    defaultUsername = $"用户{request.Phone.Substring(request.Phone.Length - 4)}_{random.Next(1000, 9999)}";
                    _logger.LogInformation($"默认用户名已存在，使用新用户名: {defaultUsername}");
                }

                // 3. 验证密码确认
                if (request.Password != request.ConfirmPassword)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "两次输入的密码不一致" 
                    });
                }

                // 4. 验证密码长度
                if (request.Password.Length < 6)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "密码长度至少6位" 
                    });
                }

                // 5. 创建客户记录
                var customerId = await CreateCustomerAsync(request, defaultUsername);
                
                if (customerId > 0)
                {
                    _logger.LogInformation($"客户注册成功: ID={customerId}, 手机号={request.Phone}, 用户名={defaultUsername}");
                    
                    return Ok(new { 
                        success = true, 
                        message = "注册成功", 
                        data = new { 
                            customerId = customerId,
                            username = defaultUsername
                        }
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
            catch (OracleException oracleEx) when (oracleEx.Number == 1) // ORA-00001: 违反唯一约束条件
            {
                _logger.LogWarning(oracleEx, $"注册时违反唯一约束: {oracleEx.Message}");
                
                // 检查是手机号冲突还是用户名冲突
                if (oracleEx.Message.Contains("PHONE"))
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "该手机号已被注册，请使用其他手机号或尝试登录" 
                    });
                }
                else if (oracleEx.Message.Contains("CUSTOMERNAME"))
                {
                    // 用户名冲突，重新尝试注册
                    return await HandleUsernameConflictAndRetry(request);
                }
                
                return BadRequest(new { 
                    success = false, 
                    message = "注册信息冲突，请修改后重试" 
                });
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
        /// 处理用户名冲突并重试注册
        /// </summary>
        private async Task<ActionResult<object>> HandleUsernameConflictAndRetry(RegisterRequest request)
        {
            try
            {
                // 生成带随机后缀的用户名
                var random = new Random();
                var uniqueUsername = $"用户{request.Phone.Substring(request.Phone.Length - 4)}_{random.Next(1000, 9999)}";
                
                // 再次检查用户名是否唯一
                while (await IsUsernameExistsAsync(uniqueUsername))
                {
                    uniqueUsername = $"用户{request.Phone.Substring(request.Phone.Length - 4)}_{random.Next(1000, 9999)}";
                }

                var customerId = await CreateCustomerAsync(request, uniqueUsername);
                
                if (customerId > 0)
                {
                    _logger.LogInformation($"重试注册成功: ID={customerId}, 手机号={request.Phone}, 用户名={uniqueUsername}");
                    
                    return Ok(new { 
                        success = true, 
                        message = "注册成功", 
                        data = new { 
                            customerId = customerId,
                            username = uniqueUsername
                        }
                    });
                }
                
                return BadRequest(new { 
                    success = false, 
                    message = "注册失败，请重试" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重试注册失败");
                return StatusCode(500, new { 
                    success = false, 
                    message = "注册失败，系统错误" 
                });
            }
        }

        /// <summary>
        /// 客户登录（使用手机号）
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

                // 返回用户信息
                return Ok(new { 
                    success = true, 
                    message = "登录成功",
                    data = new {
                        userInfo = new {
                            customerId = customer.CustomerID,
                            nickname = customer.CustomerName,
                            phone = customer.Phone,
                            memberLevel = GetVipLevelName(customer.VIPLevel ?? 0),
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
        /// 使用用户名登录
        /// </summary>
        [HttpPost("login-by-username")]
        public async Task<ActionResult<object>> LoginByUsername([FromBody] LoginByUsernameRequest request)
        {
            try
            {
                _logger.LogInformation($"客户登录请求: 用户名={request.Username}");

                // 验证用户 credentials
                var customer = await ValidateCredentialsByUsernameAsync(request.Username, request.Password);
                
                if (customer == null)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "用户名或密码错误" 
                    });
                }

                // 返回用户信息
                return Ok(new { 
                    success = true, 
                    message = "登录成功",
                    data = new {
                        userInfo = new {
                            customerId = customer.CustomerID,
                            nickname = customer.CustomerName,
                            phone = customer.Phone,
                            memberLevel = GetVipLevelName(customer.VIPLevel ?? 0),
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
        /// 检查手机号是否可用
        /// </summary>
        [HttpPost("check-phone")]
        public async Task<ActionResult<object>> CheckPhoneAvailability([FromBody] CheckPhoneRequest request)
        {
            try
            {
                _logger.LogInformation($"检查手机号可用性: {request.Phone}");

                var isRegistered = await IsPhoneRegisteredAsync(request.Phone);
                
                return Ok(new {
                    success = true,
                    data = new {
                        available = !isRegistered,
                        message = isRegistered ? "该手机号已被注册" : "手机号可用"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查手机号可用性失败");
                return StatusCode(500, new {
                    success = false,
                    message = "检查失败，系统错误"
                });
            }
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

        #endregion

        #region 私有方法

        private string GetVipLevelName(int vipLevel)
        {
            return vipLevel switch
            {
                1 => "青铜会员",
                2 => "白银会员",
                3 => "黄金会员",
                4 => "铂金会员",
                5 => "钻石会员",
                _ => "普通会员"
            };
        }

        // 检查手机号是否已注册
        private async Task<bool> IsPhoneRegisteredAsync(string phone)
        {
            try
            {
                using var connection = new OracleConnection(_databaseService.GetConnectionString("OracleConnection"));
                await connection.OpenAsync();
                
                var sql = "SELECT COUNT(*) FROM PUB.Customer WHERE Phone = :phone AND Status = '正常'";
                
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

        // 检查用户名是否已存在
        private async Task<bool> IsUsernameExistsAsync(string username)
        {
            try
            {
                using var connection = new OracleConnection(_databaseService.GetConnectionString("OracleConnection"));
                await connection.OpenAsync();
                
                var sql = "SELECT COUNT(*) FROM PUB.Customer WHERE CustomerName = :username AND Status = '正常'";
                
                using var command = new OracleCommand(sql, connection);
                command.Parameters.Add(":username", OracleDbType.Varchar2).Value = username;
                
                var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查用户名是否存在失败");
                throw;
            }
        }

        // 创建客户记录
        private async Task<int> CreateCustomerAsync(RegisterRequest request, string username)
        {
            try
            {
                using var connection = new OracleConnection(_databaseService.GetConnectionString("OracleConnection"));
                await connection.OpenAsync();
                
                var sql = @"INSERT INTO PUB.Customer (
                           CustomerID, CustomerName, Phone, Password, 
                           RegisterTime, TotalConsumption, VIPPoints, Status)
                           VALUES (
                           PUB.seq_customer_id.NEXTVAL, :CustomerName, :Phone, :Password,
                           SYSDATE, 0, 0, '正常')
                           RETURNING CustomerID INTO :CustomerID";
                
                using var command = new OracleCommand(sql, connection);
                
                command.Parameters.Add(":CustomerName", OracleDbType.Varchar2).Value = username;
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

        // 使用手机号验证凭证
        private async Task<Customer> ValidateCredentialsAsync(string phone, string password)
        {
            try
            {
                using var connection = new OracleConnection(_databaseService.GetConnectionString("OracleConnection"));
                await connection.OpenAsync();
                
                var sql = @"SELECT CustomerID, CustomerName, Phone, Password, 
                                  VIPPoints, TotalConsumption, VIPLevel 
                           FROM PUB.Customer 
                           WHERE Phone = :phone AND Password = :password AND Status = '正常'";
                
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
                        TotalConsumption = reader.GetDecimal(reader.GetOrdinal("TotalConsumption")),
                        VIPLevel = reader.IsDBNull(reader.GetOrdinal("VIPLevel")) ? null : reader.GetInt32(reader.GetOrdinal("VIPLevel"))
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

        // 使用用户名验证凭证
        private async Task<Customer> ValidateCredentialsByUsernameAsync(string username, string password)
        {
            try
            {
                using var connection = new OracleConnection(_databaseService.GetConnectionString("OracleConnection"));
                await connection.OpenAsync();
                
                var sql = @"SELECT CustomerID, CustomerName, Phone, Password, 
                                  VIPPoints, TotalConsumption, VIPLevel 
                           FROM PUB.Customer 
                           WHERE CustomerName = :username AND Password = :password AND Status = '正常'";
                
                using var command = new OracleCommand(sql, connection);
                command.Parameters.Add(":username", OracleDbType.Varchar2).Value = username;
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
                        TotalConsumption = reader.GetDecimal(reader.GetOrdinal("TotalConsumption")),
                        VIPLevel = reader.IsDBNull(reader.GetOrdinal("VIPLevel")) ? null : reader.GetInt32(reader.GetOrdinal("VIPLevel"))
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

        // 根据用户名和手机号查询客户
        private async Task<Customer> GetCustomerByUsernameAndPhoneAsync(string username, string phone)
        {
            try
            {
                using var connection = new OracleConnection(_databaseService.GetConnectionString("OracleConnection"));
                await connection.OpenAsync();
                
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

        // 更新客户密码
        private async Task<bool> UpdateCustomerPasswordAsync(int customerId, string newPassword)
        {
            try
            {
                using var connection = new OracleConnection(_databaseService.GetConnectionString("OracleConnection"));
                await connection.OpenAsync();
                
                var sql = @"UPDATE PUB.Customer 
                           SET Password = :password 
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

        // 密码哈希
        private string HashPassword(string password)
        {
            // 简单实现，生产环境应该使用更安全的加密方式
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }

        #endregion
    }

    #region 模型类

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

    /// <summary>
    /// 注册请求模型
    /// </summary>
    public class RegisterRequest
    {
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// 登录请求模型（手机号）
    /// </summary>
    public class LoginRequest
    {
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// 登录请求模型（用户名）
    /// </summary>
    public class LoginByUsernameRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
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
    /// 检查手机号请求模型
    /// </summary>
    public class CheckPhoneRequest
    {
        public string Phone { get; set; } = string.Empty;
    }

    #endregion
}