using HanwhaClient.Application;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Application.Services;
using HanwhaClient.Helper;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using System.Text.Json;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[CustomAuthorize([ScreenNames.IdracMaster])]
    public class IDracManagementController : ControllerBase
    {
        private IIDracManagementService _iDracManagementService;
        private ICurrentUserService _currentUserService;
        private IStringLocalizer<AppMessages> _localizer;
        private readonly ILogger<IDracManagementController> _logger;

        public IDracManagementController(IIDracManagementService iDracManagementService, 
            ICurrentUserService currentUserService,
            IStringLocalizer<AppMessages> localizer,
            ILogger<IDracManagementController> logger)
        {
            _iDracManagementService = iDracManagementService;
            _currentUserService = currentUserService;
            _localizer = localizer;
            _logger = logger;
        }
        [HttpPost]
        [Route("AddUpdateIDracServer")]
        [CustomAuthorize([ScreenNames.AddOrUpdateIdracServers])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> AddUpdateIDracServer(IDracServerManagementRequest request)
        {
            var userId = _currentUserService.UserId;
            var result = await _iDracManagementService.AddUpdateServerDetails(request, userId);
            if (result.isSuccess)
            {
                return StandardAPIResponse<bool>.SuccessResponse(result.isSuccess, string.IsNullOrEmpty(request.Id) ? _localizer[MessageKeys.RecordAdded] : _localizer[MessageKeys.RecordUpdated], StatusCodes.Status200OK);
            }
            return StandardAPIResponse<bool>.ErrorResponse(result.isSuccess, result.ErrorMessage , StatusCodes.Status400BadRequest);
        }

        [HttpGet]
        [Route("GetAllIDracServer")]
        [CustomAuthorize([ScreenNames.ViewListofIdracServers])]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<IDracMaster>>>> GetAllIDracServerAsync()
        {
            var result = await _iDracManagementService.GetAllIDracServerAsync();
            var response = StandardAPIResponse<IEnumerable<IDracMaster>>.SuccessResponse(result.data, _localizer[MessageKeys.RecordRetrieved], StatusCodes.Status200OK, ReferenceData: result.referenceData);
            return response;
        }

        [HttpPost]
        [Route("DeleteIDracServer")]
        [CustomAuthorize([ScreenNames.DeleteIdracServers])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> DeleteIDracServerManagement(DeleteIDracServerRequest request)
        {
            var userId = _currentUserService.UserId;
            var data = await _iDracManagementService.DeleteIDracServerManagement(request, userId);
            if (data)
            {
                return StandardAPIResponse<bool>.SuccessResponse(data, _localizer[MessageKeys.RecordDeleted]);
            }
            return StandardAPIResponse<bool>.ErrorResponse(data, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);

        }
        [HttpPost]
        [Route("AddIdracEventAlarm")]
        [CustomAuthorize([ScreenNames.AddOrEditAlarmEventIdracServers])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> AddIdracEventAlarm(IDracEventAlarmRequest request)
        {
            var userId = _currentUserService.UserId;
            var result = await _iDracManagementService.AddAlarmDetailsAsync(request);
            if (result.success)
            {
                return StandardAPIResponse<bool>.SuccessResponse(result.success, _localizer[MessageKeys.RecordAdded], StatusCodes.Status200OK);
            }
            return StandardAPIResponse<bool>.ErrorResponse(result.success, result.errorMessage, StatusCodes.Status400BadRequest);
        }

        //[HttpPost]
        //// [Authorize(AuthenticationSchemes = "BasicAuthentication", Roles = "idrac")]
        //[Route("IdracEventAlarmReceiver")]
        //[AllowAnonymous]
        //public async Task<ActionResult<StandardAPIResponse<bool>>> IdracEventAlarmReceiver(dynamic request)
        //{
        //    Console.WriteLine(
        //"sub payload: " +
        //JsonSerializer.Serialize(request));

        //    var payload =
        //        JsonSerializer.Deserialize<SubscribedAlertEventPayload>(
        //            request.ToString(),
        //            new JsonSerializerOptions
        //            {
        //                PropertyNameCaseInsensitive = true
        //            });

        //    if (payload == null)
        //    {
        //        return StandardAPIResponse<bool>
        //            .ErrorResponse(
        //                false,
        //                "Invalid payload",
        //                StatusCodes.Status400BadRequest);
        //    }

        //    var result = await _iDracManagementService.IdracEventAlarmReceiverAsync(payload);

        //    if (result)
        //    {
        //        return StandardAPIResponse<bool>.SuccessResponse(
        //            result,
        //            _localizer[MessageKeys.RecordAdded],
        //            StatusCodes.Status200OK);
        //    }

        //    return StandardAPIResponse<bool>.ErrorResponse(
        //        result,
        //        "No events found.",
        //        StatusCodes.Status400BadRequest);
        //}
        [HttpPost]
        [Route("IdracEventAlarmReceiver")]
        [Authorize(AuthenticationSchemes = "BasicAuthentication", Roles = "idrac")]
        public async Task<ActionResult<StandardAPIResponse<bool>>> IdracEventAlarmReceiver(SubscribedAlertEventPayload request)
        {
            var result = await _iDracManagementService.IdracEventAlarmReceiverAsync(request);
            if (result)
            {
                return StandardAPIResponse<bool>.SuccessResponse(result, _localizer[MessageKeys.RecordAdded], StatusCodes.Status200OK);
            }
            return StandardAPIResponse<bool>.ErrorResponse(result, "No events found.", StatusCodes.Status400BadRequest);
        }

        [HttpPost]
        [Route("DeleteIdracSubscribedEvent")]
        [CustomAuthorize([ScreenNames.DeleteAlarmEventIdracServers])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> DeleteIdracSubscribedEvent(DeleteIdracSubscribedEventRequest request)
        {
            var userId = _currentUserService.UserId;
            var data = await _iDracManagementService.DeleteIdracSubscribedEventAsync(request.AlarmEventId, request.ServerId);
            if (data)
            {
                return StandardAPIResponse<bool>.SuccessResponse(data, _localizer[MessageKeys.RecordDeleted]);
            }
            return StandardAPIResponse<bool>.ErrorResponse(data, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);

        }
        [HttpGet]
        [Route("GetIdracSubscribedEvents")]   // no use of this API it is for testing only     
        public async Task<ActionResult<StandardAPIResponse<JsonElement>>> GetIdracSubscribedEvents()
        {
            var data =
                await _iDracManagementService
                    .GetIdracSubscribedEventsAsync();

            if (data.ValueKind != JsonValueKind.Undefined && data.ValueKind != JsonValueKind.Null)
            {
                return StandardAPIResponse<JsonElement>
                    .SuccessResponse(
                        data,
                        _localizer[MessageKeys.RecordRetrieved]);
            }

            return StandardAPIResponse<JsonElement>
                .ErrorResponse(
                    data,
                    _localizer[MessageKeys.RecordNotFound],
                    StatusCodes.Status404NotFound);
        }
    }
}
