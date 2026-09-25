using HanwhaClient.Application;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Application.Services;
using HanwhaClient.Helper;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OpenCvSharp;
using System.Text;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CustomAuthorize([ScreenNames.MaintenanceMaster])]
    public class MaintenanceScheduleController : ControllerBase
    {
        private readonly IMaintenanceScheduleService _maintenanceScheduleService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<MaintenanceScheduleController> _logger;
        private readonly IStringLocalizer<AppMessages> _localizer;

        public MaintenanceScheduleController(
            IMaintenanceScheduleService service,
            ILogger<MaintenanceScheduleController> logger,
            ICurrentUserService currentUserService,
            IStringLocalizer<AppMessages> localizer)
        {
            _maintenanceScheduleService = service;
            _logger = logger;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        /// <summary>
        /// Create or update a maintenance schedule
        /// </summary>
        [HttpPost("AddUpdateMaintenanceSchedule")]
        [CustomAuthorize([ScreenNames.AddOrUpdateMaintenanceSchedule])]
        public async Task<ActionResult<StandardAPIResponse<string>>> AddUpdateMaintenanceSchedule(
            [FromBody] MaintenanceScheduleDto scheduleDto)
        {
            var userId = _currentUserService.UserId;
            scheduleDto.IsScheduleManually = true;
            // When called from API (manual insert), set isManual = true
            var result = await _maintenanceScheduleService.AddUpdateMaintenanceScheduleAsync(scheduleDto, userId);
            if (!string.IsNullOrEmpty(result))
            {
                return StandardAPIResponse<string>.SuccessResponse(null, !string.IsNullOrEmpty(scheduleDto.Id) ? _localizer[MessageKeys.RecordUpdated] : _localizer[MessageKeys.RecordAdded]);
            }
            return StandardAPIResponse<string>.ErrorResponse(null, _localizer[MessageKeys.MaintenanceDeviceExits], StatusCodes.Status200OK);

        }

        [HttpPost]
        [Route("GetMaintenanceSchedule")]
        [CustomAuthorize([ScreenNames.ViewMaintenanceSchedule])]
        public async Task<ActionResult<StandardAPIResponse<PagedResult<MaintenanceScheduleSearchResponseDto>>>> GetAllMaintenanceSchedules(MaintenanceScheduleSerachModel maintenanceScheduleSerachModel)
        {
            var schedulesResult = await _maintenanceScheduleService.GetAllAsync(maintenanceScheduleSerachModel);
            return StandardAPIResponse<PagedResult<MaintenanceScheduleSearchResponseDto>>.SuccessResponse(schedulesResult, "", StatusCodes.Status200OK);

        }

        [HttpPost]
        [Route("UpdateMaintenanceScheduleStatus")]
        [CustomAuthorize([ScreenNames.AddOrUpdateMaintenanceSchedule])]
        public async Task<ActionResult<StandardAPIResponse<string>>> UpdateMaintenanceScheduleStatus(UpdateMaintenanceScheduleStatusDto dto)
        {
            var userId = _currentUserService.UserId;
            var schedulesResult = await _maintenanceScheduleService.UpdateMaintenanceScheduleStatusAsync(dto, userId);
            return StandardAPIResponse<string>.SuccessResponse(null, _localizer[MessageKeys.RecordUpdated], StatusCodes.Status200OK);

        }

        [HttpPost]
        [Route("GetLiveImage")]
        [CustomAuthorize([ScreenNames.AddOrUpdateMaintenanceSchedule])]
        public async Task<ActionResult<StandardAPIResponse<string>>> GetLiveImage(RequestCurrentImageDto dto)
        {
            var resultImage = await _maintenanceScheduleService.GetLiveImageAsync(dto);
            return StandardAPIResponse<string>.SuccessResponse(resultImage, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("GetBeforeAfterImage")]
        [CustomAuthorize([ScreenNames.AddOrUpdateMaintenanceSchedule])]
        public async Task<ActionResult<StandardAPIResponse<string>>> GetBeforeAfterImage(RequestFullPath obj)
        {
            var result = await _maintenanceScheduleService.GetBeforeAfterImageAsync(obj.ImageFullPath);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                return StandardAPIResponse<string>.ErrorResponse(null, result.ErrorMessage, StatusCodes.Status200OK);

            }
            return StandardAPIResponse<string>.SuccessResponse(result.imageBase64, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("ExportMaintenanceScheduleCSV")]
        [CustomAuthorize([ScreenNames.ViewMaintenanceSchedule])]
        public async Task<IActionResult> ExportMaintenanceScheduleCSV(MaintenanceScheduleSerachModel maintenanceScheduleSerachModel)
        {
            var csvBuilder = await _maintenanceScheduleService.ExportMaintenanceScheduleCSV(maintenanceScheduleSerachModel);
            if (csvBuilder == null || csvBuilder.Length == 0)
            {
                return NoContent(); // HTTP 204
            }
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"Maintenance Schedule.csv");
        }

        [HttpPost("CompareImages")]
        [CustomAuthorize([ScreenNames.AddOrUpdateMaintenanceSchedule])]
        public async Task<ActionResult<StandardAPIResponse<CompareImageResponse>>> CompareImage([FromForm] CImages cImages)
        {
            if (cImages.img1 == null || cImages.img2 == null)
                return BadRequest("Both images are required.");

            // Save uploaded files temporarily
            var tempPath = Path.Combine(Path.GetTempPath(), "ImageCompare");
            Directory.CreateDirectory(tempPath);

            var img1Path = Path.Combine(tempPath, $"{Guid.NewGuid()}_{cImages.img1.FileName}");
            var img2Path = Path.Combine(tempPath, $"{Guid.NewGuid()}_{cImages.img2.FileName}");

            using (var stream = new FileStream(img1Path, FileMode.Create))
                await cImages.img1.CopyToAsync(stream);
            using (var stream = new FileStream(img2Path, FileMode.Create))
                await cImages.img2.CopyToAsync(stream);

            // Load images
            var img1 = Cv2.ImRead(img1Path, ImreadModes.Grayscale);
            var img2 = Cv2.ImRead(img2Path, ImreadModes.Grayscale);

            // Resize to same size
            Cv2.Resize(img2, img2, new OpenCvSharp.Size(img1.Width, img1.Height));

            // === FEATURE DETECTION ===
            var orb = ORB.Create(1000);
            KeyPoint[] keypoints1, keypoints2;
            using var descriptors1 = new Mat();
            using var descriptors2 = new Mat();
            orb.DetectAndCompute(img1, null, out keypoints1, descriptors1);
            orb.DetectAndCompute(img2, null, out keypoints2, descriptors2);

            if (descriptors1.Empty() || descriptors2.Empty())
                return BadRequest("Could not detect features in one or both images.");

            var bf = new BFMatcher(NormTypes.Hamming, crossCheck: true);
            var matches = bf.Match(descriptors1, descriptors2)
                            .OrderBy(m => m.Distance)
                            .Take(100)
                            .ToArray();
            Console.WriteLine($"matches.Length: {matches.Length}");

            if (matches.Length < 5)
                return BadRequest("Not enough feature matches found.");

            // === FEATURE DISPLACEMENT ===
            var avgFeatureDisplacement = matches.Average(m =>
            {
                var p1 = keypoints1[m.QueryIdx].Pt;
                var p2 = keypoints2[m.TrainIdx].Pt;
                return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
            });
            double featureDisplacementPercent = avgFeatureDisplacement / Math.Max(img1.Width, img1.Height) * 100.0;

            // === EDGE OVERLAP ===
            var edges1 = new Mat();
            var edges2 = new Mat();
            Cv2.Canny(img1, edges1, 100, 200);
            Cv2.Canny(img2, edges2, 100, 200);
            var overlap = new Mat();
            Cv2.BitwiseAnd(edges1, edges2, overlap);
            double edgeOverlapPercent = (Cv2.CountNonZero(overlap) * 100.0) / Math.Max(Cv2.CountNonZero(edges1), 1);

            // === BRIGHTNESS / CONTRAST ===
            double brightness1 = Cv2.Mean(img1).Val0;
            double brightness2 = Cv2.Mean(img2).Val0;
            double brightnessDiff = Math.Abs(brightness1 - brightness2) / 255.0 * 100.0;

            // Compute contrast as standard deviation of pixel intensities
            Mat mean1 = new Mat(), stddev1 = new Mat();
            Mat mean2 = new Mat(), stddev2 = new Mat();

            Cv2.MeanStdDev(img1, mean1, stddev1);
            Cv2.MeanStdDev(img2, mean2, stddev2);

            double contrast1 = stddev1.At<double>(0);
            double contrast2 = stddev2.At<double>(0);
            double contrastDiff = Math.Abs(contrast1 - contrast2);

            // === STRUCTURAL SIMILARITY (SSIM) ===
            double similarity = CalculateSSIM(img1, img2);
            double qualityPercent = similarity * 100.0;

            // === HOMOGRAPHY (Perspective shift) ===
            var pts1 = matches.Select(m => keypoints1[m.QueryIdx].Pt).ToArray();
            var pts2 = matches.Select(m => keypoints2[m.TrainIdx].Pt).ToArray();
            Mat homography = Cv2.FindHomography(InputArray.Create(pts1), InputArray.Create(pts2), HomographyMethods.Ransac);
            double homographyShift = Cv2.Norm(homography, NormTypes.L2) / 9.0 * 100.0;

            // === HORIZON TILT (Approximation) ===
            var lines1 = Cv2.HoughLines(edges1, 1, Math.PI / 180, 150);
            var lines2 = Cv2.HoughLines(edges2, 1, Math.PI / 180, 150);
            double tilt1 = lines1.Any() ? lines1.Average(l => (l.Theta * 180 / Math.PI) - 90) : 0;
            double tilt2 = lines2.Any() ? lines2.Average(l => (l.Theta * 180 / Math.PI) - 90) : 0;
            double tiltDifference = Math.Abs(tilt2 - tilt1);

            // === FINAL DIFFERENCE SCORE ===
            // You can weight factors if needed — here we just average normalized differences
            double overallDifferenceScore = (
                featureDisplacementPercent +
                brightnessDiff +
                contrastDiff +
                (100 - edgeOverlapPercent) +
                tiltDifference
            ) / 5.0;

            double overallSimilarity = 100 / (1 + overallDifferenceScore / 100);
            // Apply thresholds for dissimilar images
            if (matches.Length < 20 || similarity < 0.1 || edgeOverlapPercent < 0.1)
            {
                overallSimilarity = 0;
            }
            // Cleanup
            System.IO.File.Delete(img1Path);
            System.IO.File.Delete(img2Path);

            CompareImageResponse compareImageResponse = new CompareImageResponse()
            {
                OverallSimilarity = $"{overallSimilarity:F2}%",
                FeatureDisplacement = $"{featureDisplacementPercent:F2}%",
                EdgeOverlap = $"{edgeOverlapPercent:F2}%",
                BrightnessDifference = $"{brightnessDiff:F2}%",
                ContrastDifference = $"{contrastDiff:F2}",
                HomographyShift = $"{homographyShift:F2}%",
                HorizonTiltDifference = $"{tiltDifference:F2}°",
                StructuralSimilarity = $"{qualityPercent:F2}%",
            };

            return StandardAPIResponse<CompareImageResponse>.SuccessResponse(compareImageResponse, "", StatusCodes.Status200OK);
        }


        /// Calculate Structural Similarity (SSIM) between two grayscale images.
        /// Returns a value between 0 (Not matched) and 1 (identical).

        private double CalculateSSIM(Mat img1, Mat img2)
        {
            // Convert to float
            using var img1f = new Mat();
            using var img2f = new Mat();
            img1.ConvertTo(img1f, MatType.CV_32F);
            img2.ConvertTo(img2f, MatType.CV_32F);

            // Use Scalar for mean values
            Scalar mean1, stddev1;
            Scalar mean2, stddev2;

            Cv2.MeanStdDev(img1f, out mean1, out stddev1);
            Cv2.MeanStdDev(img2f, out mean2, out stddev2);

            double mean1Val = mean1.Val0;
            double mean2Val = mean2.Val0;
            double std1 = stddev1.Val0;
            double std2 = stddev2.Val0;

            // FIX 5: Use Scalar subtraction properly
            using var img1MeanRemoved = new Mat();
            using var img2MeanRemoved = new Mat();

            Cv2.Subtract(img1f, new Scalar(mean1Val), img1MeanRemoved);
            Cv2.Subtract(img2f, new Scalar(mean2Val), img2MeanRemoved);

            double covariance = Cv2.Mean(img1MeanRemoved.Mul(img2MeanRemoved)).Val0;

            // SSIM constants
            double C1 = Math.Pow(0.01 * 255, 2);
            double C2 = Math.Pow(0.03 * 255, 2);

            double numerator = (2 * mean1Val * mean2Val + C1) * (2 * covariance + C2);
            double denominator = (mean1Val * mean1Val + mean2Val * mean2Val + C1) * (std1 * std1 + std2 * std2 + C2);

            if (denominator == 0)
                return 0;

            double ssim = numerator / denominator;

            return Math.Max(0, Math.Min(1, ssim)); // Clamp between 0–1
        }

        
    }
}
