using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ConsoleApp1.Services;

namespace ConsoleApp1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QRCodeController : ControllerBase
    {
        private readonly QRCodeService _qrCodeService;
        private readonly ILogger<QRCodeController> _logger;

        public QRCodeController(QRCodeService qrCodeService, ILogger<QRCodeController> logger)
        {
            _qrCodeService = qrCodeService;
            _logger = logger;
        }

        /// <summary>
        /// ���ɵ�����̨��С������
        /// </summary>
        /// <param name="tableNumber">��̨��</param>
        /// <param name="storeId">����ID</param>
        /// <returns>С������ͼƬ</returns>
        [HttpGet("generate/{tableNumber}")]
        public async Task<IActionResult> GenerateQRCode(string tableNumber, int storeId = 1)
        {
            try
            {
                var qrCodeBytes = await _qrCodeService.GenerateQRCodeAsync(tableNumber, storeId);

                // ����ͼƬ�ļ�
                return File(qrCodeBytes, "image/png", $"qrcode_table_{tableNumber}.png");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"������̨{tableNumber}��С������ʧ��");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// ���ɵ�����̨��С�����루Base64��ʽ��
        /// </summary>
        /// <param name="tableNumber">��̨��</param>
        /// <param name="storeId">����ID</param>
        /// <returns>Base64�����С������</returns>
        [HttpGet("generate-base64/{tableNumber}")]
        public async Task<IActionResult> GenerateQRCodeBase64(string tableNumber, int storeId = 1)
        {
            try
            {
                var base64String = await _qrCodeService.GenerateQRCodeBase64Async(tableNumber, storeId);

                return Ok(new
                {
                    tableNumber = tableNumber,
                    storeId = storeId,
                    qrCodeBase64 = base64String,
                    imageUrl = $"/api/qrcode/generate/{tableNumber}?storeId={storeId}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"������̨{tableNumber}��С������Base64ʧ��");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// �������ɶ����̨��С������
        /// </summary>
        /// <param name="request">������������</param>
        /// <returns>������̨��С��������Ϣ</returns>
        [HttpPost("generate-batch")]
        public async Task<IActionResult> GenerateBatchQRCodes([FromBody] BatchGenerateRequest request)
        {
            try
            {
                var qrCodes = await _qrCodeService.GenerateBatchQRCodesAsync(request.TableNumbers, request.StoreId);

                var results = new List<object>();
                foreach (var kvp in qrCodes)
                {
                    results.Add(new
                    {
                        tableNumber = kvp.Key,
                        storeId = request.StoreId,
                        imageUrl = $"/api/qrcode/generate/{kvp.Key}?storeId={request.StoreId}",
                        imageSize = kvp.Value.Length
                    });
                }

                return Ok(new
                {
                    success = true,
                    totalCount = results.Count,
                    results = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "��������С������ʧ��");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// ����ָ����Χ����̨�ŵ�С������
        /// </summary>
        /// <param name="request">��Χ��������</param>
        /// <returns>������̨��С��������Ϣ</returns>
        [HttpPost("generate-range")]
        public async Task<IActionResult> GenerateRangeQRCodes([FromBody] RangeGenerateRequest request)
        {
            try
            {
                var qrCodes = await _qrCodeService.GenerateRangeQRCodesAsync(request.StartTableNumber, request.EndTableNumber, request.StoreId);

                var results = new List<object>();
                foreach (var kvp in qrCodes)
                {
                    results.Add(new
                    {
                        tableNumber = kvp.Key,
                        storeId = request.StoreId,
                        imageUrl = $"/api/qrcode/generate/{kvp.Key}?storeId={request.StoreId}",
                        imageSize = kvp.Value.Length
                    });
                }

                return Ok(new
                {
                    success = true,
                    startTableNumber = request.StartTableNumber,
                    endTableNumber = request.EndTableNumber,
                    totalCount = results.Count,
                    results = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"������̨{request.StartTableNumber}-{request.EndTableNumber}��Χ��С������ʧ��");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// ����������̨��С�����루ZIPѹ������
        /// </summary>
        /// <param name="startTableNumber">��ʼ��̨��</param>
        /// <param name="endTableNumber">������̨��</param>
        /// <param name="storeId">����ID</param>
        /// <returns>ZIPѹ����</returns>
        [HttpGet("download-zip")]
        public async Task<IActionResult> DownloadQRCodesZip(int startTableNumber, int endTableNumber, int storeId = 1)
        {
            try
            {
                var qrCodes = await _qrCodeService.GenerateRangeQRCodesAsync(startTableNumber, endTableNumber, storeId);

                // ����ZIP�ļ�
                using var memoryStream = new MemoryStream();
                using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
                {
                    foreach (var kvp in qrCodes)
                    {
                        var entry = archive.CreateEntry($"qrcode_table_{kvp.Key}.png");
                        using var entryStream = entry.Open();
                        await entryStream.WriteAsync(kvp.Value, 0, kvp.Value.Length);
                    }
                }

                memoryStream.Position = 0;
                var fileName = $"qrcodes_tables_{startTableNumber}-{endTableNumber}_store{storeId}.zip";

                return File(memoryStream.ToArray(), "application/zip", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"������̨{startTableNumber}-{endTableNumber}��Χ��С������ZIPʧ��");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// ���Խӿ� - ������״̬
        /// </summary>
        /// <returns>����״̬��Ϣ</returns>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new
            {
                message = "QRCode������������",
                timestamp = DateTime.Now,
                endpoints = new
                {
                    single = "GET /api/qrcode/generate/{tableNumber}",
                    singleBase64 = "GET /api/qrcode/generate-base64/{tableNumber}",
                    batch = "POST /api/qrcode/generate-batch",
                    range = "POST /api/qrcode/generate-range",
                    downloadZip = "GET /api/qrcode/download-zip?startTableNumber=X&endTableNumber=Y&storeId=Z"
                }
            });
        }
    }

    /// <summary>
    /// ������������ģ��
    /// </summary>
    public class BatchGenerateRequest
    {
        public List<string> TableNumbers { get; set; } = new List<string>();
        public int StoreId { get; set; } = 1;
    }

    /// <summary>
    /// ��Χ��������ģ��
    /// </summary>
    public class RangeGenerateRequest
    {
        public int StartTableNumber { get; set; }
        public int EndTableNumber { get; set; }
        public int StoreId { get; set; } = 1;
    }
}
