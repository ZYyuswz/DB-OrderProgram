using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ConsoleApp1.Services
{
    public class QRCodeService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<QRCodeService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _accessToken;

        public QRCodeService(IConfiguration configuration, ILogger<QRCodeService> logger, HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
            _accessToken = _configuration["WeChat:AccessToken"];
        }

        /// <summary>
        /// 生成小程序码
        /// </summary>
        /// <param name="tableNumber">桌台号</param>
        /// <param name="storeId">店铺ID</param>
        /// <returns>二维码图片的字节数组</returns>
        public async Task<byte[]> GenerateQRCodeAsync(string tableNumber, int storeId = 1)
        {
            try
            {
                if (string.IsNullOrEmpty(_accessToken))
                {
                    throw new InvalidOperationException("微信访问令牌未配置");
                }

                // 构建小程序页面路径，包含桌台号和店铺ID参数
                var path = $"pages/index/index?tableNumber={tableNumber}&storeId={storeId}";
                
                // 构建请求参数
                var requestData = new
                {
                    path = path,
                    width = 430, // 二维码宽度
                    auto_color = false, // 是否自动配置线条颜色
                    line_color = new { r = 0, g = 0, b = 0 }, // 二维码颜色，黑色
                    is_hyaline = false, // 是否需要透明底色
                    env_version = "release" // 小程序版本，正式版
                };

                var jsonContent = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // 调用微信API生成小程序码
                var url = $"https://api.weixin.qq.com/wxa/getwxacode?access_token={_accessToken}";
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var imageBytes = await response.Content.ReadAsByteArrayAsync();
                    _logger.LogInformation($"成功生成桌台{tableNumber}的小程序码，图片大小: {imageBytes.Length} 字节");
                    return imageBytes;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"生成小程序码失败，HTTP状态码: {response.StatusCode}, 错误信息: {errorContent}");
                    throw new Exception($"生成小程序码失败: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"生成桌台{tableNumber}的小程序码时发生错误");
                throw;
            }
        }

        /// <summary>
        /// 批量生成多个桌台的小程序码
        /// </summary>
        /// <param name="tableNumbers">桌台号列表</param>
        /// <param name="storeId">店铺ID</param>
        /// <returns>桌台号和小程序码的字典</returns>
        public async Task<Dictionary<string, byte[]>> GenerateBatchQRCodesAsync(List<string> tableNumbers, int storeId = 1)
        {
            var results = new Dictionary<string, byte[]>();
            
            foreach (var tableNumber in tableNumbers)
            {
                try
                {
                    var qrCode = await GenerateQRCodeAsync(tableNumber, storeId);
                    results[tableNumber] = qrCode;
                    
                    // 添加延迟避免API调用过于频繁
                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"生成桌台{tableNumber}的小程序码失败");
                    // 继续处理其他桌台
                }
            }

            return results;
        }

        /// <summary>
        /// 生成指定范围内桌台号的小程序码
        /// </summary>
        /// <param name="startTableNumber">起始桌台号</param>
        /// <param name="endTableNumber">结束桌台号</param>
        /// <param name="storeId">店铺ID</param>
        /// <returns>桌台号和小程序码的字典</returns>
        public async Task<Dictionary<string, byte[]>> GenerateRangeQRCodesAsync(int startTableNumber, int endTableNumber, int storeId = 1)
        {
            var tableNumbers = new List<string>();
            for (int i = startTableNumber; i <= endTableNumber; i++)
            {
                tableNumbers.Add(i.ToString());
            }

            return await GenerateBatchQRCodesAsync(tableNumbers, storeId);
        }

        /// <summary>
        /// 获取小程序码的Base64编码（用于前端显示）
        /// </summary>
        /// <param name="tableNumber">桌台号</param>
        /// <param name="storeId">店铺ID</param>
        /// <returns>Base64编码的图片字符串</returns>
        public async Task<string> GenerateQRCodeBase64Async(string tableNumber, int storeId = 1)
        {
            var imageBytes = await GenerateQRCodeAsync(tableNumber, storeId);
            return Convert.ToBase64String(imageBytes);
        }
    }
}
