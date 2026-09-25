using HanwhaClient.Application;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Application.Services;
using HanwhaClient.Helper;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using MongoDB.Driver.Core.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CustomAuthorize([ScreenNames.MultiServerMaster])]
    public class MultiServerManagementController : ControllerBase
    {
        private readonly IViMultiServerManagementService _serverManagementService;
        private readonly IViServerManagementRepository _viServerManagementRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<AppMessages> _localizer;
        private readonly IReportService _reportService;

        public MultiServerManagementController(IViMultiServerManagementService serverManagementService,
            ICurrentUserService currentUserService,
            IStringLocalizer<AppMessages> localizer,
            IViServerManagementRepository viServerManagementRepository,
            IReportService reportService)
        {
            _serverManagementService = serverManagementService;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _viServerManagementRepository = viServerManagementRepository;
            _reportService = reportService;
        }


        [HttpPost]
        [Route("AddUpdateServer")]
        [CustomAuthorize([ScreenNames.AddOrUpdateMultiServers])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> AddUpdateServerDetail(ViMultiServerManagementDTO serverManagementRequest)
        {
            var userId = _currentUserService.UserId;
            var result = await _serverManagementService.AddUpdateServerDetails(serverManagementRequest, userId);
            if (result.isSuccess)
            {
                return StandardAPIResponse<bool>.SuccessResponse(result.isSuccess, string.IsNullOrEmpty(serverManagementRequest.Id) ? _localizer[MessageKeys.RecordAdded] : _localizer[MessageKeys.RecordUpdated], StatusCodes.Status200OK);
            }
            return StandardAPIResponse<bool>.ErrorResponse(result.isSuccess, result.errorMessage, StatusCodes.Status400BadRequest);
        }

        [HttpPost]
        [Route("EnableServer")]
        [CustomAuthorize([ScreenNames.EnabledMultiServers])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> EnableServerManagement(EnabledServerRequest request)
        {
            var userId = _currentUserService.UserId;
            var data = await _serverManagementService.EnableServerManagement(request, userId);
            if (data.isSuccess)
            {
                return StandardAPIResponse<bool>.SuccessResponse(data.isSuccess, _localizer[MessageKeys.RecordUpdated], StatusCodes.Status200OK);
            }
            return StandardAPIResponse<bool>.ErrorResponse(data.isSuccess, data.errorMessage, StatusCodes.Status400BadRequest);
        }

        

        [HttpGet]
        [Route("GetAllServer")]
        [CustomAuthorize([ScreenNames.ViewListofMultiServers])]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<ViMultiServerManagement>>>> GetAllServerDetails()
        {

            var data = await _serverManagementService.GetAllServerDetails();
            if (data.Count() > 0)
            {
                return StandardAPIResponse<IEnumerable<ViMultiServerManagement>>.SuccessResponse(data, "", StatusCodes.Status200OK);
            }
            return StandardAPIResponse<IEnumerable<ViMultiServerManagement>>.SuccessResponse(null, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);
        }

        [HttpPost]
        [Route("DeleteServer")]
        [CustomAuthorize([ScreenNames.DeleteMultiServers])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> DeleteServerManagement(DeleteServerManagement request)
        {
            var userId = _currentUserService.UserId;
            var data = await _serverManagementService.DeleteServerManagement(request, userId);
            if (data)
            {
                return StandardAPIResponse<bool>.SuccessResponse(data, _localizer[MessageKeys.RecordDeleted]);
            }
            return StandardAPIResponse<bool>.ErrorResponse(data, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);

        }

        [HttpPost("MongoDatabaseConnection")]
        public async Task<ActionResult<StandardAPIResponse<string>>> MongoDatabaseConnection(DatabaseConnectionRequest databaseConnectionRequest)
        {
            var apiBaseUrl = databaseConnectionRequest?.HostingAddress;
            var username = databaseConnectionRequest.Username;
            var password = databaseConnectionRequest.Password;
            string mongoConnectionString = "";

            try
            {
                using var httpClient = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
                var loginResponse = await _reportService.GetAuthToken(httpClient, username, password);
                if (!loginResponse.IsSuccess)
                {
                    return StandardAPIResponse<string>.ErrorResponse(null, "Authentication failed: The credentials provided for the server are invalid.", StatusCodes.Status500InternalServerError);
                }

                httpClient.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", loginResponse.Data.AccessToken);

                var connectionStringData = await httpClient.GetAsync($"{apiBaseUrl}/api/MultiServerManagement/ApplicationDatabase");
                var connectionStringRes = JsonConvert.DeserializeObject<StandardAPIResponse<string>>(await connectionStringData.Content.ReadAsStringAsync());
                return StandardAPIResponse<string>.SuccessResponse(connectionStringRes.Data, "", StatusCodes.Status200OK);
            }
            catch (Exception ex)
            {
                return StandardAPIResponse<string>.ErrorResponse(null, "The provided server details are invalid. Please enter the correct details.", StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("ApplicationDatabase")]
        public async Task<ActionResult<StandardAPIResponse<string>>> ApplicationDatabase()
        {
            string connectionString = _viServerManagementRepository.GetApplicationConnectionString();

            if (!string.IsNullOrEmpty(connectionString))
            {
                return StandardAPIResponse<string>.SuccessResponse(connectionString, "", StatusCodes.Status200OK);
            }
            return StandardAPIResponse<string>.ErrorResponse(null, _localizer[MessageKeys.SomethingWentWrong], StatusCodes.Status500InternalServerError);
        }

        [HttpGet]
        [Route("GetAllActiveServerForDashboard")]
        [CustomAuthorize([ScreenNames.ViewListofMultiServers])]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<GetViMultiServerDashDto>>>> GetAllActiveServerForDashboard()
        {

            var data = await _serverManagementService.GetAllActiveServerForDashboardAsync();
            if (data.Count() > 0)
            {
                return StandardAPIResponse<IEnumerable<GetViMultiServerDashDto>>.SuccessResponse(data, "", StatusCodes.Status200OK);
            }
            return StandardAPIResponse<IEnumerable<GetViMultiServerDashDto>>.SuccessResponse(null, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);
        }

    }
}
