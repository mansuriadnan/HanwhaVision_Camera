using HanwhaClient.Application;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Application.Services;
using HanwhaClient.Helper;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Text;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CustomAuthorize([ScreenNames.AuditLogMaster])]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;
        private readonly IStringLocalizer<AppMessages> _localizer;
        private readonly ICurrentUserService _currentUserService;
        public AuditLogController(IAuditLogService auditLogService, IStringLocalizer<AppMessages> localizer, ICurrentUserService currentUserService)
        {
            this._auditLogService = auditLogService;
            _localizer = localizer;
            _currentUserService = currentUserService;
        }

        //[HttpGet]
        //[Route("GetAuditLog")]
        //public async Task<ActionResult<StandardAPIResponse<AuditLogResponse>>> GetAuditLog(string CollectionName, int? PageSize, int? PageNo, string? OperationType, string? documentId)
        //{
        //    AuditLogRequest auditLogRequest = new AuditLogRequest();
        //    auditLogRequest.CollectioName = CollectionName;
        //    auditLogRequest.PageSize = PageSize;
        //    auditLogRequest.PageNo = PageNo;
        //    auditLogRequest.OperationType = OperationType;
        //    auditLogRequest.Id = documentId;

        //    var result = await _auditLogService.GetAuditLogDetail(auditLogRequest);
        //    var response = StandardAPIResponse<AuditLogResponse>.SuccessResponse(result.auditLogDetail, _localizer[MessageKeys.RecordRetrieved], StatusCodes.Status200OK, ReferenceData: result.referenceData);
        //    return response;
        //}


        [HttpGet]
        [Route("AuditLogCollectionName")]
        public async Task<ActionResult<StandardAPIResponse<List<string>>>> AuditLogCollectionName()
        {

            List<string> result = await _auditLogService.GetAuditLogCollectionName();
            var response = StandardAPIResponse<List<string>>.SuccessResponse(result, _localizer[MessageKeys.RecordRetrieved], StatusCodes.Status200OK);
            return response;

        }

        [HttpPost]
        [Route("GetAuditLogs")]
        public async Task<ActionResult<StandardAPIResponse<AuditLogsResponse>>> GetAuditLogs(AuditLogsRequest request)
        {
            var result = await _auditLogService.GetAuditLogsDetail(request);
            var response = StandardAPIResponse<AuditLogsResponse>.SuccessResponse(result.auditLogsDetail, AppMessageConstants.RecordRetrieved, StatusCodes.Status200OK, ReferenceData: result.referenceData);
            return response;
        }

        [HttpPost]
        [Route("ExportAuditLogsCSV")]
        [CustomAuthorize([ScreenNames.AuditLogMaster])]
        public async Task<IActionResult> ExportAuditLogsCSVAsync(AuditLogsRequest request)
        {
            var userId = _currentUserService.UserId;
            var csvBuilder = await _auditLogService.ExportAuditLogsCSVAsync(request, userId);
            if (csvBuilder == null || csvBuilder.Length == 0)
            {
                return NoContent(); // HTTP 204
            }
            var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(bytes, "text/csv", $"AuditLogs.csv");
        }

    }
}
