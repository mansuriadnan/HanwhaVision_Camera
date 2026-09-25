using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using FluentFTP;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Core.Interfaces;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.DbEntities;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.Formula.Functions;
using System.Reflection.Metadata;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        private readonly IClientSettingService _clientSettingService;
        private readonly IDeviceEventsRepository _deviceEventsRepository;
        private readonly IDeviceMasterRepository _deviceMasterRepository;
        private readonly ILogger<VideoController> _logger;
        private readonly IFileLogger _fileLogger;


        public VideoController(IClientSettingService clientSettingService, IDeviceEventsRepository deviceEventsRepository, IDeviceMasterRepository deviceMasterRepository, HttpClient httpClient, ILogger<VideoController> logger, IFileLogger fileLogger)
        {
            _clientSettingService = clientSettingService;
            _deviceEventsRepository = deviceEventsRepository;
            _deviceMasterRepository = deviceMasterRepository;
            _logger = logger;
            _fileLogger = fileLogger;
        }

        [HttpGet("StreamVideoWithTempFile/{eventId}")]
        public async Task<IActionResult> StreamVideoWithTempFile(string eventId)
        {
            string tempFilePath = null;
            FtpClient client = null;

            try
            {
                _fileLogger.Log($"StreamVideoWithTempFile started for eventId: {eventId} at {DateTime.UtcNow}");

                var result = await _clientSettingService.GetClientSetting();
                var eventData = await _deviceEventsRepository.GetAsync(eventId);

                if (result?.FtpConfiguration == null || eventData == null)
                {
                    _fileLogger.Log("Configuration or event data not found");
                    return NotFound(new { message = "Configuration or event data not found" });
                }

                var deviceData = await _deviceMasterRepository.GetAsync(eventData.DeviceId);
                if (deviceData == null)
                {
                    _fileLogger.Log("Device data not found");
                    return NotFound(new { message = "Device data not found" });
                }

                var deviceIp = deviceData.IpAddress.Split(":")[0];
                var eventTime = eventData.DeviceTime.Value.ToLocalTime();

                client = new FtpClient(
                    result.FtpConfiguration.Host,
                    result.FtpConfiguration.Username,
                    result.FtpConfiguration.Password
                );
                client.Connect();

                string originalFileName = $"{deviceIp}-{eventData.DeviceTime.Value.ToLocalTime():yy-MM-dd-HH-mm-ss}-EventRule_{eventData.RuleIndex}-CH{eventData.ChannelNo + 1}.mkv";

                _fileLogger.Log("fileName " + originalFileName);

                // Search for the file within ±10 seconds
                var foundFileName = FindFileWithNearbySeconds(client, deviceIp, eventTime, eventData, _fileLogger);

                if (foundFileName == null)
                {
                    _fileLogger.Log($"No video file {originalFileName} found within ±10 seconds of {eventTime:yy-MM-dd-HH-mm-ss}");
                    return NotFound($"Video file not found: {originalFileName}");
                }

                _fileLogger.Log($"Found file: {foundFileName}");

                // HEAD request — just confirm existence, no body needed
                if (HttpMethods.IsHead(Request.Method))
                {
                    return Ok();
                }

                // GET request — download and stream
                tempFilePath = Path.GetTempFileName();
                client.DownloadFile(tempFilePath, foundFileName);

                client.Disconnect();
                client.Dispose();
                client = null;

                var fileStream = new FileStream(
                    tempFilePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 4096,
                    useAsync: true
                );

                Response.Headers.Append("Accept-Ranges", "bytes");
                Response.Headers.Append("Content-Length", fileStream.Length.ToString());

                HttpContext.Response.RegisterForDispose(new TempFileCleanup(tempFilePath, _fileLogger));

                return File(fileStream, "video/x-matroska", enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                _fileLogger.Log($"Error in StreamVideoWithTempFile: {ex.Message}");

                if (tempFilePath != null && System.IO.File.Exists(tempFilePath))
                {
                    try { System.IO.File.Delete(tempFilePath); }
                    catch (Exception cleanupEx) { _fileLogger.Log($"Temp cleanup failed: {cleanupEx.Message}"); }
                }

                return StatusCode(500, new { message = $"Error streaming video: {ex.Message}" });
            }
            finally
            {
                client?.Dispose();
            }
        }

        public class TempFileCleanup : IDisposable
        {
            private readonly string _path;
            private readonly IFileLogger _logger;

            public TempFileCleanup(string path, IFileLogger logger)
            {
                _path = path;
                _logger = logger;
            }

            public void Dispose()
            {
                try
                {
                    if (System.IO.File.Exists(_path))
                        System.IO.File.Delete(_path);
                }
                catch (Exception ex)
                {
                    _logger.Log("Failed to delete temp file: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Searches for a video file on the FTP server starting at the exact event time,
        /// then forward +1..+10 seconds, then backward -1..-10 seconds.
        /// Returns the filename if found, or null if not found within the range.
        /// </summary>
        private static string FindFileWithNearbySeconds(
            FtpClient client,
            string deviceIp,
            DateTime eventTime,
            DeviceEvents eventData,
            IFileLogger logger)
        {
            // Build the ordered list of candidate offsets: 0, +1, +2, ... +10, -1, -2, ... -10
            var offsets = BuildSearchOffsets(forwardLimit: 15, backwardLimit: 15);

            foreach (var offsetSeconds in offsets)
            {
                var candidateTime = eventTime.AddSeconds(offsetSeconds);
                var candidateFileName = BuildFileName(deviceIp, candidateTime, eventData);

                logger.Log($"Checking offset {offsetSeconds:+0;-0;0}s → {candidateFileName}");

                if (client.FileExists(candidateFileName))
                {
                    logger.Log($"File found at offset {offsetSeconds:+0;-0;0}s: {candidateFileName}");
                    return candidateFileName;
                }
            }

            return null;
        }

        /// <summary>
        /// Generates search offsets in the order: 0, +1, +2, ..., +forward, -1, -2, ..., -backward
        /// </summary>
        private static IEnumerable<int> BuildSearchOffsets(int forwardLimit, int backwardLimit)
        {
            // Start with 0 (exact match)
            yield return 0;

            // Forward: +1 to +forwardLimit
            for (int i = 1; i <= forwardLimit; i++)
                yield return i;

            // Backward: -1 to -backwardLimit
            for (int i = 1; i <= backwardLimit; i++)
                yield return -i;
        }

        /// <summary>
        /// Builds the FTP filename from device IP, timestamp, and event metadata.
        /// Format: {DeviceIp}-{yy-MM-dd-HH-mm-ss}-EventRule_{RuleIndex}-CH{ChannelNo}.mkv
        /// DateTime.AddSeconds() handles all overflow/underflow automatically
        /// (e.g. 13:34:59 + 1s = 13:35:00, 13:35:00 - 1s = 13:34:59)
        /// </summary>
        private static string BuildFileName(string deviceIp, DateTime time, DeviceEvents eventData)
        {
            return $"{deviceIp}-{time:yy-MM-dd-HH-mm-ss}" +
            $"-EventRule_{eventData.RuleIndex}" +
                   $"-CH{eventData.ChannelNo + 1}.mkv";
        }


        [HttpGet("StreamVideoWithTempFile1/{eventId}")]
        public async Task<IActionResult> StreamVideoWithTempFile1(string eventId)
        {
            string tempFilePath = null;
            FtpClient client = null;
            //eventId = "68523fdaee19ef2ba1f14f64";
            //eventId = "68661a01984fdb5929953e00";
            try
            {
                var result = await _clientSettingService.GetClientSetting();
                var eventData = await _deviceEventsRepository.GetAsync(eventId);

                if (result?.FtpConfiguration == null || eventData == null)
                {
                    _fileLogger.Log("Configuration or event data not found " + DateTime.UtcNow);
                    return NotFound("Configuration or event data not found");
                }

                var deviceData = await _deviceMasterRepository.GetAsync(eventData.DeviceId);

                if (deviceData == null)
                {
                    _fileLogger.Log("device data not found " + DateTime.UtcNow);
                    return NotFound("device data not found");
                }

                var deviceIp = deviceData.IpAddress.Split(":")[0];
                string ftpHost = result.FtpConfiguration.Host;
                string ftpUser = result.FtpConfiguration.Username;
                string ftpPass = result.FtpConfiguration.Password;

                string fileName = $"{deviceIp}-{eventData.DeviceTime.Value.ToLocalTime():yy-MM-dd-HH-mm-ss}-EventRule_{eventData.RuleIndex}-CH{eventData.ChannelNo + 1:D2}.mkv";

                _fileLogger.Log("fileName " + fileName);

                //fileName = "10.37.58.245-26-02-18-15-16-39-EventRule_1-CH01.mkv";
                //string fileName = "10.37.58.245-25-02-20-13-25-56-EventRule_1-CH01.mkv";
                //string fileName = "10.37.58.245-25-06-15-02-27-56-EventRule_1-CH01.mkv";
                //string fileName = "10.37.58.245-25-06-14-00-03-52-EventRule_1-CH01.mkv";
                //string fileName = "10.37.58.245-25-06-13-23-49-14-EventRule_1-CH01.mkv";
                //string fileName = "10.37.58.245-25-06-13-23-53-04-EventRule_1-CH01.mkv";
                //string fileName = "10.37.58.245-25-06-13-23-34-58-EventRule_1-CH01.mkv";

                client = new FtpClient(ftpHost, ftpUser, ftpPass);
                client.Connect();

                if (!client.FileExists(fileName))
                {
                    _fileLogger.Log("Video file not foun " + fileName);
                    return NotFound($"Video file not found: {fileName}");
                }

                // Create temp file
                tempFilePath = Path.GetTempFileName();

                // Download file to temp location
                client.DownloadFile(tempFilePath, fileName);

                // Now we can use the temp file with full range support
                var fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                Response.Headers.Add("Accept-Ranges", "bytes");
                Response.Headers.Add("Content-Length", fileStream.Length.ToString());

                // The FileResult will handle the stream disposal and temp file cleanup
                return File(fileStream, "video/x-matroska", enableRangeProcessing: true);
            }
            catch (Exception ex)
            {
                // Clean up temp file if something goes wrong
                if (tempFilePath != null && System.IO.File.Exists(tempFilePath))
                {
                    try { System.IO.File.Delete(tempFilePath); }
                    catch (Exception exx)
                    {

                        _fileLogger.Log("Video file not foun " + exx.Message);
                    }
                }

                client?.Dispose();
                _fileLogger.Log("Error streaming video " + ex.Message);
                return StatusCode(500, $"Error streaming video: {ex.Message}");
            }
            finally
            {
                client?.Dispose();
            }
        }

    }
}
