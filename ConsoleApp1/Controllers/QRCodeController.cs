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
        /// 生成单个桌台的小程序码
        /// </summary>
        /// <param name="tableNumber">桌台号</param>
        /// <param name="storeId">店铺ID</param>
        /// <returns>小程序码图片</returns>
        [HttpGet("generate/{tableNumber}")]
        public async Task<IActionResult> GenerateQRCode(string tableNumber, int storeId = 1)
        {
            try
            {
                var qrCodeBytes = await _qrCodeService.GenerateQRCodeAsync(tableNumber, storeId);
                
                // 返回图片文件
                return File(qrCodeBytes, "image/png", $"qrcode_table_{tableNumber}.png");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"生成桌台{tableNumber}的小程序码失败");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// 生成单个桌台的小程序码（Base64格式）
        /// </summary>
        /// <param name="tableNumber">桌台号</param>
        /// <param name="storeId">店铺ID</param>
        /// <returns>Base64编码的小程序码</returns>
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
                _logger.LogError(ex, $"生成桌台{tableNumber}的小程序码Base64失败");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// 批量生成多个桌台的小程序码
        /// </summary>
        /// <param name="request">批量生成请求</param>
        /// <returns>所有桌台的小程序码信息</returns>
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
                _logger.LogError(ex, "批量生成小程序码失败");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// 生成指定范围内桌台号的小程序码
        /// </summary>
        /// <param name="request">范围生成请求</param>
        /// <returns>所有桌台的小程序码信息</returns>
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
                _logger.LogError(ex, $"生成桌台{request.StartTableNumber}-{request.EndTableNumber}范围的小程序码失败");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// 下载所有桌台的小程序码（ZIP压缩包）
        /// </summary>
        /// <param name="startTableNumber">起始桌台号</param>
        /// <param name="endTableNumber">结束桌台号</param>
        /// <param name="storeId">店铺ID</param>
        /// <returns>ZIP压缩包</returns>
        [HttpGet("download-zip")]
        public async Task<IActionResult> DownloadQRCodesZip(int startTableNumber, int endTableNumber, int storeId = 1)
        {
            try
            {
                var qrCodes = await _qrCodeService.GenerateRangeQRCodesAsync(startTableNumber, endTableNumber, storeId);
                
                // 创建ZIP文件
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
                _logger.LogError(ex, $"下载桌台{startTableNumber}-{endTableNumber}范围的小程序码ZIP失败");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// 测试接口 - 检查服务状态
        /// </summary>
        /// <returns>服务状态信息</returns>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new
            {
                message = "QRCode服务正常运行",
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
    /// 批量生成请求模型
    /// </summary>
    public class BatchGenerateRequest
    {
        public List<string> TableNumbers { get; set; } = new List<string>();
        public int StoreId { get; set; } = 1;
    }

    /// <summary>
    /// 范围生成请求模型
    /// </summary>
    public class RangeGenerateRequest
    {
        public int StartTableNumber { get; set; }
        public int EndTableNumber { get; set; }
        public int StoreId { get; set; } = 1;
    }
}
