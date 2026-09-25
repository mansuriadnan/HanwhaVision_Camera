using HanwhaClient.Application;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Application.Services;
using HanwhaClient.Core.Interfaces;
using HanwhaClient.Helper;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[CustomAuthorize([ScreenNames.WidgetPermission])]
    public class DashboardPreferenceController : ControllerBase
    {
        private readonly IDashboardPreferenceService _dashboardPreferenceService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICsvGenerator _csvGenerator;
        private readonly IExcelGenerator _excelGenerator;
        private readonly IUsersService _userService;
        private readonly IUserNotificationRepository _userNotificationRepository;
        private readonly IUserNotificationService _userNotificationService;
        private readonly IStringLocalizer<AppMessages> _localizer;

        public DashboardPreferenceController(IDashboardPreferenceService dashboardPreferenceService,
            IPdfGenerator pdfService, ICsvGenerator csvGenerator,
            IExcelGenerator excelGenerator, IUsersService usersService,
            ICurrentUserService currentUserService,
            IUserNotificationRepository userNotificationRepository,
            IUserNotificationService userNotificationService, IStringLocalizer<AppMessages> localizer)
        {
            _dashboardPreferenceService = dashboardPreferenceService;
            _currentUserService = currentUserService;
            _csvGenerator = csvGenerator;
            _excelGenerator = excelGenerator;
            _userService = usersService;
            _userNotificationRepository = userNotificationRepository;
            _userNotificationService = userNotificationService;
            _localizer = localizer;
        }

        [HttpPost]
        //[CustomAuthorize([ScreenNames.WidgetPermission])]
        public async Task<ActionResult<StandardAPIResponse<string>>> SaveDashboardDesign(SaveDashboardDesign dashboardDesign)
        {
            var userId = _currentUserService.UserId;
            var data = await _dashboardPreferenceService.SaveDashboardDesignAsync(dashboardDesign, userId);
            if (string.IsNullOrEmpty(data.ErrorMessage))
            {
                return StandardAPIResponse<string>.SuccessResponse(data.Id, string.IsNullOrEmpty(dashboardDesign.Id) ? _localizer[MessageKeys.RecordAdded] : _localizer[MessageKeys.RecordUpdated]);
            }
            return StandardAPIResponse<string>.ErrorResponse(null, data.ErrorMessage, StatusCodes.Status400BadRequest);
        }

        [HttpPost]
        [Route("DeleteDashboard")]
        //[CustomAuthorize([ScreenNames.WidgetPermission])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> DeleteDashboard(DeleteRequestDto dto)
        {
            var userId = _currentUserService.UserId;
            var data = await _dashboardPreferenceService.DeleteDashboardAsync(dto.Id, userId);
            if (!data)
            {
                return StandardAPIResponse<bool>.ErrorResponse(false, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);
            }
            return StandardAPIResponse<bool>.SuccessResponse(data, _localizer[MessageKeys.RecordDeleted]);
        }

        [HttpGet]
        //[CustomAuthorize([ScreenNames.WidgetPermission])]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<GetDashboardPreferenceResponse>>>> GetDashboardPreference()
        {
            var userId = _currentUserService.UserId;
            var result = await _dashboardPreferenceService.GetDashboardPreferenceByUserIdAsync(userId);
            return StandardAPIResponse<IEnumerable<GetDashboardPreferenceResponse>>.SuccessResponse(result, _localizer[MessageKeys.RecordRetrieved], StatusCodes.Status200OK);
        }

        [HttpGet]
        [Route("GenerateExcel")]
        //[CustomAuthorize([ScreenNames.CanDownloadReports])]
        public async Task<ActionResult> GenerateExcel()
        {
            var data = await _userService.GetAllUsersAsync();
            var excel = _excelGenerator.ConvertDatatoExcel(data.data);
            return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DynamicData.xlsx");
        }

        [HttpGet]
        [Route("UserNotification")]
        [CustomAuthorize([ScreenNames.ViewListofNotifications])]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<UserNotificationResponse>>>> GetUserNotification(int pageNo = 1, int pageSize = 10)
        {
            var userId = _currentUserService.UserId;
            var result = await _dashboardPreferenceService.GetUserNotificationAsync(pageNo, pageSize);
            return StandardAPIResponse<IEnumerable<UserNotificationResponse>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpGet]
        [Route("UserNotificationCount")]
        [CustomAuthorize([ScreenNames.ViewListofNotifications])]
        public async Task<ActionResult<StandardAPIResponse<long>>> GetUserNotificationCount()
        {
            var result = await _userNotificationRepository.GetUserNotificationCountAsync();
            return StandardAPIResponse<long>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("MarkReadUserNotification")]
        [CustomAuthorize([ScreenNames.ViewListofNotifications])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> MarkReadUserNotification(MarkReadNotificationRequest markReadNotificationRequest)
        {
            markReadNotificationRequest.UserId = _currentUserService.UserId;
            var result = await _userNotificationService.MarkReadUserNotification(markReadNotificationRequest);
            return StandardAPIResponse<bool>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }
    }
}
