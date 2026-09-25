using HanwhaClient.Application;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Helper;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using HanwhaClient.Model.SSM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CustomAuthorize([ScreenNames.SsmMaster])]
    public class SSMServerManagementController : ControllerBase
    {
        ISSMServerManagementService _sSMServerManagementService;
        private ICurrentUserService _currentUserService;
        private IStringLocalizer<AppMessages> _localizer;
        private readonly ISsmClientService _ssmClientService;

        public SSMServerManagementController(ISSMServerManagementService sSMServerManagementService, ICurrentUserService currentUserService, IStringLocalizer<AppMessages> localizer, ISsmClientService ssmClientService)
        {
            _sSMServerManagementService = sSMServerManagementService;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _ssmClientService = ssmClientService;
        }
        [HttpPost]
        [Route("AddUpdateServer")]
        [CustomAuthorize([ScreenNames.AddOrUpdateSsmServers])]
        public async Task<ActionResult<StandardAPIResponse<string>>> AddUpdateServerDetail(SSMServerManagementRequest request)
        {
            var userId = _currentUserService.UserId;
            var result = await _sSMServerManagementService.AddUpdateServerDetails(request, userId);
            
            if (string.IsNullOrEmpty(result.Id))
            {
                return StandardAPIResponse<string>.ErrorResponse(string.Empty, result.ErrorMessage, StatusCodes.Status400BadRequest);
            }
            return StandardAPIResponse<string>.SuccessResponse(result.Id, string.IsNullOrEmpty(request.Id) ? _localizer[MessageKeys.RecordAdded] : _localizer[MessageKeys.RecordUpdated]);
        }

        [HttpGet]
        [Route("GetAllSsmServers")]
        [CustomAuthorize([ScreenNames.ViewListofSsmServers])]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<SsmSiteMapping>>>> GetAllSsmServersAsync()
        {
            var result = await _sSMServerManagementService.GetAllSsmServersAsync();
            var response = StandardAPIResponse<IEnumerable<SsmSiteMapping>>.SuccessResponse(result.data, _localizer[MessageKeys.RecordRetrieved], StatusCodes.Status200OK, ReferenceData: result.referenceData);
            return response;
        }

        [HttpPost]
        [Route("DeleteSsmServer")]
        [CustomAuthorize([ScreenNames.DeleteSsmServers])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> DeleteSsmServerManagement(DeleteSsmServerRequest request)
        {
            var userId = _currentUserService.UserId;
            var data = await _sSMServerManagementService.DeleteSsmServerManagement(request, userId);
            if (data)
            {
                return StandardAPIResponse<bool>.SuccessResponse(data, _localizer[MessageKeys.RecordDeleted]);
            }
            return StandardAPIResponse<bool>.ErrorResponse(data, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);
        }

        [HttpGet]
        [Route("ssmapitest")]
        public async Task<ActionResult<StandardAPIResponse<bool>>> AddUpdateServerDetail222()
        {


            var userId = _currentUserService.UserId;
            //var result = await _sSMServerManagementService.AddUpdateServerDetails(request, userId);
            const string baseUrl = "https://10.37.60.38:9991";
            const string username = "admin2";
            const string password = "admin@123";


            // ── Step 1: Authenticate ──────────────────────────────
            Console.WriteLine("=== Step 1: Authenticating ===");
            bool ok = await _ssmClientService.LoginAsync(baseUrl, username, password);
            if (!ok)
            {
                Console.WriteLine("Login failed. Exiting.");
                //return false;
            }

            //using var http = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
            //var client = new _ssmClientService(http);


            if (ok)
            {

                // ── Step 2: Get all SSM servers ───────────────────────
                Console.WriteLine("=== Step 2: GET /v3/metrics/servers ===");
                var servers = await _ssmClientService.GetAsync<List<SsmServerSummary>>("/v3/metrics/servers");


                return StandardAPIResponse<bool>.SuccessResponse(ok, string.IsNullOrEmpty("") ? _localizer[MessageKeys.RecordAdded] : _localizer[MessageKeys.RecordUpdated], StatusCodes.Status200OK);
            }
            return StandardAPIResponse<bool>.ErrorResponse(ok, "", StatusCodes.Status400BadRequest);
        }


    }
}
