using HanwhaClient.Application;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Core.Services.License;
using HanwhaClient.Helper;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.License;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LicenseController : ControllerBase
    {
        private readonly ILicenseService _licenseService;
        private readonly IPermissionService _permissionService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILicenseHistoryService _licenseHistoryService;
        private readonly IStringLocalizer<AppMessages> _localizer;


        public LicenseController(ILicenseService licenseService,
            IPermissionService permissionService,
            ICurrentUserService currentUserService,
            ILicenseHistoryService licenseHistoryService,
            IStringLocalizer<AppMessages> localizer)
        {
            _licenseService = licenseService;
            _permissionService = permissionService;
            _currentUserService = currentUserService;
            _licenseHistoryService = licenseHistoryService;
            _localizer = localizer;
        }

        [HttpGet("ValidateLicense")]
        //[CustomAuthorize([ScreenNames.LicenseMaster])]
        [Authorize]
        public async Task<ActionResult<StandardAPIResponse<bool>>> ValidateLicense()
        {
            _permissionService.RefreshLicenseData();
            var result = _permissionService._licenseData;

            if (result.IsValid)
            {
                return StandardAPIResponse<bool>.SuccessResponse(result.IsValid, _localizer[MessageKeys.LicenseAuthenticated]);
            }
            return StandardAPIResponse<bool>.ErrorResponse(result.IsValid, result.ErrorMessage, StatusCodes.Status400BadRequest);
        }

        [HttpPost("UploadLicense")]
        [CustomAuthorize([ScreenNames.CanAddOrUpdateLicense])]
        public async Task<ActionResult<StandardAPIResponse<string>>> UploadLicense(IFormFile file)
        {
            return await HandleLicenseUpload(
                file,
                tempFileName: "license.lic",
                finalFileName: "license.lic"
            );
        }



        [HttpPost("UploadLicenseKey")]
        [CustomAuthorize([ScreenNames.CanAddOrUpdateLicense])]
        public async Task<ActionResult<StandardAPIResponse<string>>> UploadLicenseKey(IFormFile file)
        {
            return await HandleLicenseUpload(
                file,
                tempFileName: "public-key.pem",
                finalFileName: "public-key.pem"
            );
        }

        private async Task<ActionResult<StandardAPIResponse<string>>> HandleLicenseUpload(
            IFormFile file,
            string tempFileName,
            string finalFileName)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(_localizer[MessageKeys.FileUploadFailure]);
            }

            // Step 1: Upload to temp
            await _licenseService.UploadTempfile(file, "License/LicenseTemp", tempFileName);

            // Step 2: Validate
            _permissionService.RefreshLicenseData();
            if (_permissionService._licenseData.ErrorMessage != null)
            {
                return StandardAPIResponse<string>.ErrorResponse(null, _localizer[MessageKeys.FileUploadFailure], StatusCodes.Status404NotFound);
            }

            // Step 3: Upload to final path
            await _licenseService.Uploadfile(file, "License", finalFileName);

            // Step 4: Optional history logging (only if required for both files)
            await _licenseHistoryService.SaveLicenseHistoryAsync(_currentUserService.UserId);

            return StandardAPIResponse<string>.SuccessResponse(file.FileName, _localizer[MessageKeys.FileUploaded]);
        }


        //[HttpPost("UploadLicense")]
        //[CustomAuthorize([ScreenNames.CanAddOrUpdateLicense])]
        //public async Task<ActionResult<StandardAPIResponse<string>>> UploadLicense(IFormFile file)
        //{
        //    if (file == null || file.Length == 0)
        //    {
        //        return BadRequest(_localizer[MessageKeys.FileUploadError);
        //    }

        //    await _licenseService.UploadTempfile(file, "License/LicenseTemp", "license.lic");
        //    _permissionService.RefreshLicenseData();
        //    if (_permissionService._licenseData.ErrorMessage == null)
        //    {
        //        await _licenseService.Uploadfile(file, "License", "license.lic");
        //        await _licenseHistoryService.SaveLicenseHistoryAsync(_currentUserService.UserId);
        //    }
        //    return StandardAPIResponse<string>.SuccessResponse(file.FileName, _localizer[MessageKeys.FileUploadSuccess);
        //}

        //[HttpPost("UploadLicenseKey")]
        //[CustomAuthorize([ScreenNames.CanAddOrUpdateLicense])]
        //public async Task<ActionResult<StandardAPIResponse<string>>> UploadLicenseKey(IFormFile file)
        //{
        //    if (file == null || file.Length == 0)
        //    {
        //        return BadRequest(_localizer[MessageKeys.FileUploadError);
        //    }
        //    await _licenseService.UploadTempfile(file, "License/LicenseTemp", "public-key.pem");
        //    await _licenseService.Uploadfile(file, "License", "public-key.pem");
        //    return StandardAPIResponse<string>.SuccessResponse(file.FileName, _localizer[MessageKeys.FileUploadSuccess);
        //}

        [HttpGet("GetHardwareId")]
        [CustomAuthorize([ScreenNames.GeneralMaster])]
        public async Task<ActionResult<StandardAPIResponse<string>>> GetHardwareId()
        {
            string machineId = HardwareHelper.GetHardwareId();
            return StandardAPIResponse<string>.SuccessResponse(machineId, _localizer[MessageKeys.RecordRetrieved]);
        }

        [HttpGet("LicenseDetail")]
        [CustomAuthorize([ScreenNames.LicenseMaster])]
        public async Task<ActionResult<StandardAPIResponse<LicenseResponse>>> LicenseDetail()
        {
            var userId = _currentUserService.UserId;
            var result = await _licenseService.GetLicenseDetailAsync(userId);
            if (result == null)
            {
                return StandardAPIResponse<LicenseResponse>.ErrorResponse(null, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);
            }
            return StandardAPIResponse<LicenseResponse>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpGet("LicenseHistory")]
        [CustomAuthorize([ScreenNames.LicenseMaster])]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<LicenseHistory>>>> LicenseHistory()
        {
            var result = await _licenseHistoryService.GetLicenseHistoryAsync();
            return StandardAPIResponse<IEnumerable<LicenseHistory>>.SuccessResponse(result.data, "", StatusCodes.Status200OK, ReferenceData: result.referenceData);
        }

        [HttpPost("UploadFullLicense")]
        [CustomAuthorize([ScreenNames.CanAddOrUpdateLicense])]
        public async Task<ActionResult<StandardAPIResponse<string>>> UploadFullLicense(IFormFile licenseFile, IFormFile publicKeyFile)
        {
            if (licenseFile == null || licenseFile.Length == 0 || publicKeyFile == null || publicKeyFile.Length == 0)
            {
                return BadRequest(_localizer[MessageKeys.LicenseKeyRequired]);
            }

            // Check file extensions
            var licenseExtension = Path.GetExtension(licenseFile.FileName)?.ToLower();
            var keyExtension = Path.GetExtension(publicKeyFile.FileName)?.ToLower();

            if (licenseExtension != ".lic" || keyExtension != ".pem")
            {
                return StandardAPIResponse<string>.ErrorResponse("", _localizer[MessageKeys.LicenseInvalidFileTypes]);
            }

            // Step 1: Upload both files to Temp folder
            await _licenseService.UploadTempfile(licenseFile, "License/LicenseTemp", "license.lic");
            await _licenseService.UploadTempfile(publicKeyFile, "License/LicenseTemp", "public-key.pem");

            // Step 2: Validate with both files
            _permissionService.RefreshLicenseData();
            if (_permissionService._licenseData.ErrorMessage != null)
            {
                string _licenseDirectoryTemp = Path.Combine(Directory.GetCurrentDirectory(), "License/LicenseTemp");

                if (Directory.Exists(_licenseDirectoryTemp))
                {
                    Directory.Delete(_licenseDirectoryTemp, recursive: true);
                }

                return StandardAPIResponse<string>.ErrorResponse(null, _permissionService._licenseData.ErrorMessage, StatusCodes.Status400BadRequest);
            }
            else
            {
                // Step 3: Upload both files to final folder
                await _licenseService.Uploadfile(licenseFile, "License", "license.lic");
                await _licenseService.Uploadfile(publicKeyFile, "License", "public-key.pem");

                // Step 4: Save history
                await _licenseHistoryService.SaveLicenseHistoryAsync(_currentUserService.UserId);

                return StandardAPIResponse<string>.SuccessResponse(_localizer[MessageKeys.LicenseKeyUploaded], _localizer[MessageKeys.FileUploaded]);
            }
        }

    }
}
