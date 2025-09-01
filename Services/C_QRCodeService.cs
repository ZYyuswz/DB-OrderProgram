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
        /// ����С������
        /// </summary>
        /// <param name="tableNumber">��̨��</param>
        /// <param name="storeId">����ID</param>
        /// <returns>��ά��ͼƬ���ֽ�����</returns>
        public async Task<byte[]> GenerateQRCodeAsync(string tableNumber, int storeId = 1)
        {
            try
            {
                if (string.IsNullOrEmpty(_accessToken))
                {
                    throw new InvalidOperationException("΢�ŷ�������δ����");
                }

                // ����С����ҳ��·����������̨�ź͵���ID����
                var path = $"pages/index/index?tableNumber={tableNumber}&storeId={storeId}";

                // �����������
                var requestData = new
                {
                    path = path,
                    width = 430, // ��ά����
                    auto_color = false, // �Ƿ��Զ�����������ɫ
                    line_color = new { r = 0, g = 0, b = 0 }, // ��ά����ɫ����ɫ
                    is_hyaline = false, // �Ƿ���Ҫ͸����ɫ
                    env_version = "release" // С����汾����ʽ��
                };

                var jsonContent = JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // ����΢��API����С������
                var url = $"https://api.weixin.qq.com/wxa/getwxacode?access_token={_accessToken}";
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var imageBytes = await response.Content.ReadAsByteArrayAsync();
                    _logger.LogInformation($"�ɹ�������̨{tableNumber}��С�����룬ͼƬ��С: {imageBytes.Length} �ֽ�");
                    return imageBytes;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"����С������ʧ�ܣ�HTTP״̬��: {response.StatusCode}, ������Ϣ: {errorContent}");
                    throw new Exception($"����С������ʧ��: {response.StatusCode} - {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"������̨{tableNumber}��С������ʱ��������");
                throw;
            }
        }

        /// <summary>
        /// �������ɶ����̨��С������
        /// </summary>
        /// <param name="tableNumbers">��̨���б�</param>
        /// <param name="storeId">����ID</param>
        /// <returns>��̨�ź�С��������ֵ�</returns>
        public async Task<Dictionary<string, byte[]>> GenerateBatchQRCodesAsync(List<string> tableNumbers, int storeId = 1)
        {
            var results = new Dictionary<string, byte[]>();

            foreach (var tableNumber in tableNumbers)
            {
                try
                {
                    var qrCode = await GenerateQRCodeAsync(tableNumber, storeId);
                    results[tableNumber] = qrCode;

                    // ����ӳٱ���API���ù���Ƶ��
                    await Task.Delay(100);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"������̨{tableNumber}��С������ʧ��");
                    // ��������������̨
                }
            }

            return results;
        }

        /// <summary>
        /// ����ָ����Χ����̨�ŵ�С������
        /// </summary>
        /// <param name="startTableNumber">��ʼ��̨��</param>
        /// <param name="endTableNumber">������̨��</param>
        /// <param name="storeId">����ID</param>
        /// <returns>��̨�ź�С��������ֵ�</returns>
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
        /// ��ȡС�������Base64���루����ǰ����ʾ��
        /// </summary>
        /// <param name="tableNumber">��̨��</param>
        /// <param name="storeId">����ID</param>
        /// <returns>Base64�����ͼƬ�ַ���</returns>
        public async Task<string> GenerateQRCodeBase64Async(string tableNumber, int storeId = 1)
        {
            var imageBytes = await GenerateQRCodeAsync(tableNumber, storeId);
            return Convert.ToBase64String(imageBytes);
        }
    }
}
