using HanwhaClient.Application;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Helper;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExceptionController : ControllerBase
    {
        private readonly IExceptionLogService _exceptionLogService;
        private readonly IStringLocalizer<AppMessages> _localizer;

        public ExceptionController(IExceptionLogService exceptionLogService, IStringLocalizer<AppMessages> localizer)
        {
            _exceptionLogService = exceptionLogService;
            _localizer = localizer;
        }

        [HttpPost]
        [Route("GetExceptionLogs")]
        [Authorize]
        public async Task<ActionResult<StandardAPIResponse<ExceptionLogsResponse>>> GetExceptionLogs([FromBody] ExceptionLogsRequest request)
        {
            var data = await _exceptionLogService.GetExceptionLogsAsync(request);
            if (data.ExceptionLogsDetails.Count() > 0)
            {
                return StandardAPIResponse<ExceptionLogsResponse>.SuccessResponse(data, "", StatusCodes.Status200OK);
            }
            return StandardAPIResponse<ExceptionLogsResponse>.SuccessResponse(null, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);

        }
        [HttpPost]
        [Route("SendExceptionAlertEmail")]
        [Authorize]
        public async Task<ActionResult<StandardAPIResponse<bool>>> SendExceptionAlertEmail([FromBody] SendExceptionAlertEmailRequest request)
        {
            var data = await _exceptionLogService.SendExceptionAlertEmail(request);
            return StandardAPIResponse<bool>.SuccessResponse(data, _localizer[MessageKeys.EmailSentSuccessfully], StatusCodes.Status200OK);

        }
    }
}
