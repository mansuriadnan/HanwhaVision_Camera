using AutoMapper;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.License;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace HanwhaClient.Application.Services
{
    public class LicenseService : ILicenseService
    {
        private readonly IDeviceMasterRepository _deviceMasterRepository;
        private readonly IUsersRepository _usersRepository;
        private readonly IPermissionService _permissionService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LicenseService(IDeviceMasterRepository deviceMasterRepository,
            IConfiguration configuration,
            IUsersRepository usersRepository,
            IPermissionService permissionService,
            IHttpContextAccessor httpContextAccessor
            )
        {
            this._deviceMasterRepository = deviceMasterRepository;
            this._usersRepository = usersRepository;
            this._permissionService = permissionService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Uploadfile(IFormFile file, string directoryName, string? fileName = null)
        {

            string _licenseDirectoryTemp = Path.Combine(Directory.GetCurrentDirectory(), directoryName + "/LicenseTemp");

            if (Directory.Exists(_licenseDirectoryTemp))
            {
                Directory.Delete(_licenseDirectoryTemp, recursive: true);
            }


            string _licenseDirectory = Path.Combine(Directory.GetCurrentDirectory(), directoryName);

            if (!Directory.Exists(_licenseDirectory))
            {
                Directory.CreateDirectory(_licenseDirectory);
            }

            var filePath = Path.Combine(_licenseDirectory, (fileName ?? file.FileName));

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }

        public async Task<LicenseResponse> GetLicenseDetailAsync(string clientId)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userName = user?.Claims.FirstOrDefault(x => x.Type == "unique_name")?.Value;
            LicenseDataModel licenseData = _permissionService._licenseData;
            var result = new LicenseResponse
            {
                ClientId = clientId,
                Cameras = licenseData.NumberOfCameras,
                Users = licenseData.NumberOfUsers,
                ExpiryDate = licenseData.ExpiryDate,
                UtilizedCamera = await _deviceMasterRepository.GetCameraCountAsync(),
                UtilizedUser = await _usersRepository.GetUserCountAsync(userName),
                CompanyName = licenseData.CompanyName,
                SiteName = licenseData.SiteName,
                StartDate = licenseData.StartDate,
                MACAddress = licenseData.MACAddress,
                CustomerName = licenseData.CustomerName,
                LicenseType = licenseData.LicenseType,
                HardwareId = licenseData.HardwareId,
                NoOfChannel = licenseData.NoOfChannel,
                IsANPR = licenseData.IsANPR ?? false,
                IsMaintenance = licenseData.IsMaintenance ?? false,
            };
            return result;
        }

        public async Task UploadTempfile(IFormFile file, string directoryName, string? fileName = null)
        {

            string _licenseDirectory = Path.Combine(Directory.GetCurrentDirectory(), directoryName);

            if (!Directory.Exists(_licenseDirectory))
            {
                Directory.CreateDirectory(_licenseDirectory);
            }

            var filePath = Path.Combine(_licenseDirectory, (fileName ?? file.FileName));

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
        }
    }
}
