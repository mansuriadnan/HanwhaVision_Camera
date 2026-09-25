using HanwhaClient.Application;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Helper;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientSettingController : ControllerBase
    {
        private readonly IClientSettingService _clientSettingService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IClientTimeZoneService _clientTimeZoneService;
        private readonly ILicenseService _licenseService;
        private readonly IAuthService _authService;
        private readonly IUsersService _usersService;
        private readonly IWebHostEnvironment _env;
        private readonly IStringLocalizer<AppMessages> _localizer;
        private readonly IVehicleCurrentParkingCountRepository _vehicleCurrentParkingCountRepository;

        public ClientSettingController(IClientSettingService clientSettingService,
            ICurrentUserService currentUserService,
            IClientTimeZoneService clientTimeZoneService,
            ILicenseService licenseService,
            IAuthService authService,
            IUsersService usersService,
            IWebHostEnvironment env,
            IStringLocalizer<AppMessages> localizer,
            IVehicleCurrentParkingCountRepository vehicleCurrentParkingCountRepository)
        {
            this._clientSettingService = clientSettingService;
            this._currentUserService = currentUserService;
            this._clientTimeZoneService = clientTimeZoneService;
            _licenseService = licenseService;
            _authService = authService;
            _usersService = usersService;
            _env = env;
            _localizer = localizer;
            _vehicleCurrentParkingCountRepository = vehicleCurrentParkingCountRepository;
        }

        [HttpPost("uploadClientLogo")]
        [CustomAuthorize([ScreenNames.ViewCompanyConfigurations])]
        public async Task<ActionResult<StandardAPIResponse<string>>> UploadClientLogo(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(_localizer[MessageKeys.FileUploadFailure]);
            }

            await _clientSettingService.SaveClientLogo(file, null);
            return StandardAPIResponse<string>.SuccessResponse(file.FileName, _localizer[MessageKeys.FileUploaded]);
        }

        [HttpGet("Logo")]
        [Authorize]
        public async Task<ActionResult<StandardAPIResponse<string>>> GetClientLogo()
        {
            var data = await _clientSettingService.GetClientLogo(null);
            return StandardAPIResponse<string>.SuccessResponse(data, _localizer[MessageKeys.RecordRetrieved]);
        }

        [HttpPost("Smpt")]
        [CustomAuthorize([ScreenNames.ViewSmtpConfigurations])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> SaveClientSMTPSettings(SmtpSettings smtpSettings)
        {
            var data = await _clientSettingService.SaveClientSMTPSettings(smtpSettings, null);
            return StandardAPIResponse<bool>.SuccessResponse(data, _localizer[MessageKeys.RecordAdded]);
        }

        [HttpPost("ClientOperationalTiming")]
        [CustomAuthorize([ScreenNames.ViewOPTimeConfigurations])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> SaveClientOperationalTiming(ClientOperationalTimingRequest clientOperationalTiming)
        {
            var userId = _currentUserService.UserId;
            var data = await _clientSettingService.SaveClientOperationalTiming(clientOperationalTiming, userId);
            return StandardAPIResponse<bool>.SuccessResponse(data, _localizer[MessageKeys.RecordUpdated], StatusCodes.Status200OK);
        }

        [HttpPost("ClientOperationalTimeZone")]
        [Authorize]
        public async Task<ActionResult<StandardAPIResponse<bool>>> SaveClientOperationalTimeZone(ClientOperationalTimeZoneRequest clientOperationalTiming)
        {
            var userId = _currentUserService.UserId;
            var data = await _clientSettingService.SaveClientOperationalTimeZone(clientOperationalTiming, userId);
            return StandardAPIResponse<bool>.SuccessResponse(data, _localizer[MessageKeys.RecordUpdated], StatusCodes.Status200OK);
        }

        [HttpGet("ClientSetting")]
        [Authorize]
        public async Task<ActionResult<StandardAPIResponse<ClientSettings>>> GetClientSetting()
        {
            var data = await _clientSettingService.GetClientSetting();
            return StandardAPIResponse<ClientSettings>.SuccessResponse(data, "", StatusCodes.Status200OK);
        }

        [HttpGet("ClientTimeZones")]
        [Authorize]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<ClientTimezones>>>> GetClientTimeZones()
        {
            var data = await _clientTimeZoneService.GetClientTimezones();
            return StandardAPIResponse<IEnumerable<ClientTimezones>>.SuccessResponse(data, "", StatusCodes.Status200OK);
        }

        [HttpPost("GoogleApiKey")]
        [CustomAuthorize([ScreenNames.ViewGoogleMapKeyConfigurations])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> SaveGoogleApiKey(GoogleApiKeyRequest googleApiKeyRequest)
        {
            var userId = _currentUserService.UserId;

            using var httpClient = new HttpClient();
            var result = await httpClient.GetAsync("https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=latitude,longitude&radius=1500&key=" + googleApiKeyRequest.ApiKey);
            var apiData = await result.Content.ReadAsStringAsync();
            if (apiData.Contains("The provided API key is invalid"))
            {
                return StandardAPIResponse<bool>.ErrorResponse(false, _localizer[MessageKeys.InvalidGoogleKey], StatusCodes.Status401Unauthorized);
            }
            var data = await _clientSettingService.SaveGoogleApiKeyAsync(googleApiKeyRequest, userId);
            return StandardAPIResponse<bool>.SuccessResponse(data, _localizer[MessageKeys.RecordUpdated], StatusCodes.Status200OK);
        }

        [HttpPost("FTPConfiguration")]
        [CustomAuthorize([ScreenNames.ViewFTPConfigurations])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> SaveFTPConfiguration(FtpConfigurationRequest ftpConfigurationRequest)
        {
            var userId = _currentUserService.UserId;
            var data = await _clientSettingService.SaveFTPConfigurationAsync(ftpConfigurationRequest, userId);
            if (data)
            {
                return StandardAPIResponse<bool>.SuccessResponse(data, _localizer[MessageKeys.RecordUpdated], StatusCodes.Status200OK);
            }
            return StandardAPIResponse<bool>.SuccessResponse(data, "", StatusCodes.Status500InternalServerError);
        }

        [HttpPost("SSLCertificate")]
        [CustomAuthorize([ScreenNames.ViewSSLConfigurations])]
        public async Task<ActionResult<StandardAPIResponse<string>>> SaveSSLCertificate(IFormFile file)
        {
            var userId = _currentUserService.UserId;
            if (file == null || file.Length == 0)
            {
                return BadRequest(_localizer[MessageKeys.SomethingWentWrong]);
            }

            await _licenseService.Uploadfile(file, "Certificate", "SSLCertificate.crt");
            await _clientSettingService.UploadSSLCertificate(file.FileName, userId);
            return StandardAPIResponse<string>.SuccessResponse(file.FileName, _localizer[MessageKeys.FileUploaded]);
        }

        [HttpPost("TurnReportSchedule")]
        [CustomAuthorize([ScreenNames.ViewReportScheduler])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> TurnReportSchedule(TurnReportSchedule turnReportSchedule)
        {
            var userId = _currentUserService.UserId;
            var data = await _clientSettingService.TurnReportScheduleAsync(turnReportSchedule, userId);
            if (data)
            {
                return StandardAPIResponse<bool>.SuccessResponse(data, _localizer[MessageKeys.RecordUpdated], StatusCodes.Status200OK);
            }
            return StandardAPIResponse<bool>.SuccessResponse(data, _localizer[MessageKeys.SomethingWentWrong], StatusCodes.Status500InternalServerError);
        }

        [HttpPost("ReportSchedule")]
        [CustomAuthorize([ScreenNames.ViewReportScheduler])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> AddUpdateReportSchedule(ReportScheduleRequest reportScheduleRequest)
        {
            var userId = _currentUserService.UserId;
            var data = await _clientSettingService.AddUpdateReportScheduleAsync(reportScheduleRequest, userId);
            if (data)
            {
                return StandardAPIResponse<bool>.SuccessResponse(data, _localizer[MessageKeys.RecordUpdated], StatusCodes.Status200OK);
            }
            return StandardAPIResponse<bool>.SuccessResponse(data, _localizer[MessageKeys.SomethingWentWrong], StatusCodes.Status500InternalServerError);
        }

        [HttpPost("BackupDBConfiguration")]
         [CustomAuthorize([ScreenNames.View_and_Configure_Scheduled_Database_Backup])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> BackupDBConfiguration(BackupDBConfigurationRequest backupDBConfigurationRequest)
        {
            var userId = _currentUserService.UserId;
            var data = await _clientSettingService.BackupDBConfigurationAsync(backupDBConfigurationRequest, userId);
            if (data)
            {
                return StandardAPIResponse<bool>.SuccessResponse(data, _localizer[MessageKeys.RecordUpdated], StatusCodes.Status200OK);
            }
            return StandardAPIResponse<bool>.SuccessResponse(data, _localizer[MessageKeys.SomethingWentWrong], StatusCodes.Status500InternalServerError);
        }


        [HttpPost("RetentionDBConfiguration")]
        [CustomAuthorize([ScreenNames.View_and_Configure_Retention_Setup_Details])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> RetentionDBConfiguration(RetentionDBConfigurationRequest retentionDBConfigurationRequest)
        {
            var userId = _currentUserService.UserId;
            var data = await _clientSettingService.RetentionDBConfigurationAsync(retentionDBConfigurationRequest, userId);
            if (data)
            {
                return StandardAPIResponse<bool>.SuccessResponse(data, _localizer[MessageKeys.RecordUpdated], StatusCodes.Status200OK);
            }
            return StandardAPIResponse<bool>.SuccessResponse(data, _localizer[MessageKeys.SomethingWentWrong], StatusCodes.Status500InternalServerError);
        }

        [HttpPost("ANPRImageLocation")]
        [CustomAuthorize([ScreenNames.View_and_Configure_ANPR])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> ANPRImageLocationConfiguration(ANPRImageLocationRequest aNPRImageLocationRequest)
        {
            var userId = _currentUserService.UserId;
            var data = await _clientSettingService.ANPRImageLocationConfiguration(aNPRImageLocationRequest, userId);
            if (data)
            {
                return StandardAPIResponse<bool>.SuccessResponse(data, _localizer[MessageKeys.RecordUpdated], StatusCodes.Status200OK);
            }
            return StandardAPIResponse<bool>.SuccessResponse(data, _localizer[MessageKeys.SomethingWentWrong], StatusCodes.Status500InternalServerError);
        }

        [HttpPost("ResetParkingCount")]
        [CustomAuthorize([ScreenNames.View_and_Configure_Reset_Vehicle_Parking_Count])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> ResetParkingCount(ParkingCountResetRequest parkingCountResetRequest)
        {
            var data = await _vehicleCurrentParkingCountRepository.ResetParkingCount(parkingCountResetRequest);
            if (data)
            {
                return StandardAPIResponse<bool>.SuccessResponse(data, _localizer[MessageKeys.RecordUpdated], StatusCodes.Status200OK);
            }
            return StandardAPIResponse<bool>.SuccessResponse(data, _localizer[MessageKeys.SomethingWentWrong], StatusCodes.Status500InternalServerError);
        }

        [HttpGet("ApplicationMainLogo")]
        //[CustomAuthorize([ScreenNames.GeneralMaster])]
        public async Task<ActionResult<StandardAPIResponse<string>>> ApplicationMainLogo()
        {
            return StandardAPIResponse<string>.SuccessResponse(AppLogoString.logo, _localizer[MessageKeys.RecordUpdated], StatusCodes.Status200OK);
        }

        //[HttpGet("chart")]
        //public async Task<IActionResult> GenerateChartReportPdf()
        //{
        //    try
        //    {
        //        var pdfBytes = await _clientSettingService.GenerateChartReportPdf();
        //        return File(pdfBytes, "application/pdf", "chart.pdf");
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception
        //        return BadRequest("PDF generation failed: " + ex.Message);
        //    }
        //}

    }
}
