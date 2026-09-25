using HanwhaClient.Application;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Application.Services;
using HanwhaClient.Helper;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Text;
using System.Text.Json;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LicensePlateRecogController : ControllerBase
    {
        private readonly ILicensePlateRecogService _licensePlateRecogService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<AppMessages> _localizer;
        private readonly IClientSettingService _clientSettingService;

        public LicensePlateRecogController(ICurrentUserService currentUserService,
            ILicensePlateRecogService licensePlateRecogService,
            IStringLocalizer<AppMessages> localizer,
            IClientSettingService clientSettingService)
        {
            _licensePlateRecogService = licensePlateRecogService;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _clientSettingService = clientSettingService;
        }
        [HttpPost]
        [Route("")]
        [Authorize(AuthenticationSchemes = "BasicAuthentication", Roles = "vaxtor")]
        public async Task<ActionResult<StandardAPIResponse<string>>> CreateLicensePlateRecogAsync([FromBody] JsonElement request)
        {           
            var userId = _currentUserService.UserId;
            var data = await _licensePlateRecogService.SaveLicensePlateRecogDetailsAsync(request, userId);
            if (string.IsNullOrEmpty(data.Id))
            {
                return StandardAPIResponse<string>.ErrorResponse(string.Empty, data.ErrorMessage, StatusCodes.Status400BadRequest);
            }
            return StandardAPIResponse<string>.SuccessResponse(data.Id,AppMessageConstants.RecordAdded);
        }

        [HttpPost]
        [Route("GetAllLprDetails")]
        [CustomAuthorize([ScreenNames.ViewLPR])]
        public async Task<ActionResult<StandardAPIResponse<PagedResult<AllLprDetailsResponse>>>> GetAllLprDetails([FromBody] AllLprRequest request)
        {
            var (data, referenceData) = await _licensePlateRecogService.GetAllLprDetailsAsync(request);
            
            if (data != null && data.Items.Any())
            {
                return StandardAPIResponse<PagedResult<AllLprDetailsResponse>>.SuccessResponse(data, AppMessageConstants.RecordRetrieved, StatusCodes.Status200OK, ReferenceData: referenceData);
            }
            
            return StandardAPIResponse<PagedResult<AllLprDetailsResponse>>.SuccessResponse(null, AppMessageConstants.RecordNotFound, StatusCodes.Status200OK, ReferenceData: referenceData);
        }

        [HttpPost]
        [Route("ExportLPRDetailsCSV")]
        [CustomAuthorize([ScreenNames.ViewLPR])]
        public async Task<IActionResult> ExportDeviceCSV(AllLprRequest lprRequest)
        {
            var userId = _currentUserService.UserId;
            var csvBuilder = await _licensePlateRecogService.GetLPRDtailsCSVAsync(lprRequest, userId);
            if (csvBuilder == null || csvBuilder.Length == 0)
            {
                return NoContent(); // HTTP 204
            }
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"LPRDetails.csv");
        }

        [HttpPost]
        [Route("anpr-image")]
        public async Task<IActionResult> GetAnprImage(AnprImageRequest request)
        {
            var clientSettings = await _clientSettingService.GetClientSetting();

            string rootPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "ANPRImages");

            if (!string.IsNullOrEmpty(
                clientSettings?.ANPRImageConfiguration?.ImagePath))
            {
                rootPath = clientSettings.ANPRImageConfiguration.ImagePath;
            }

            string imagePath = Path.Combine(
                rootPath,
                request.Size.Equals("large", StringComparison.OrdinalIgnoreCase)
                    ? "Large"
                    : "Small",
                request.ImageName);

            if (!System.IO.File.Exists(imagePath))
                return NotFound();

            var bytes = await System.IO.File.ReadAllBytesAsync(imagePath);

            return File(bytes, "image/png");
        }

    }
}
