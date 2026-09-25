using HanwhaClient.Application;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Helper;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using HanwhaClient.Model.Role;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [CustomAuthorize([ScreenNames.PermissionMaster])]
    public class RoleScreenMappingController : ControllerBase
    {
        private readonly IRoleScreenMappingService _roleScreenMappingService;
        private readonly IUsersService _usersService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPermissionService _permissionService;
        private readonly IStringLocalizer<AppMessages> _localizer;
        private readonly IWidgetRepository _widgetRepository;


        public RoleScreenMappingController(IRoleScreenMappingService roleScreenMappingService, IUsersService usersService, ICurrentUserService currentUserService, IPermissionService permissionService, IStringLocalizer<AppMessages> localizer, IWidgetRepository widgetRepository)
        {
            _roleScreenMappingService = roleScreenMappingService;
            _usersService = usersService;
            _currentUserService = currentUserService;
            _permissionService = permissionService;
            _localizer = localizer;
            _widgetRepository = widgetRepository;
        }

        [HttpGet("RoleScreenMappings/{roleId}")]
        [CustomAuthorize([ScreenNames.ScreenPermission])]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<ScreenMaster>>>> GetRoleScreenMappings(string roleId)
        {
            var result = await _roleScreenMappingService.GetRoleScreenMappingsByRoleIdAsync(roleId);
            if (result.data == null || result.data.Count() == 0)
            {
                return StandardAPIResponse<IEnumerable<ScreenMaster>>.ErrorResponse(null, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);
            }

            var licenseData = _permissionService._licenseData;

            // Work on a mutable copy
            var newData = result.data.ToList();

            if (!licenseData.IsMaintenance ?? false)
            {
                // Find all "Maintenance" screens (there might be more than one)
                var maintenanceScreens = newData
                .Where(x => x.ScreenName == "Maintenance")
                .ToList();

                if (maintenanceScreens.Any())
                {
                    // Collect parent Ids for "Maintenance" screens
                    var maintenanceScreenIds = maintenanceScreens.Select(x => x.Id).ToHashSet();

                    // Remove all "Maintenance" parents and their direct children
                    newData.RemoveAll(x => maintenanceScreenIds.Contains(x.Id) || maintenanceScreenIds.Contains(x.ParentsScreenId));
                }
            }
            if (!licenseData.IsANPR ?? false)
            {
                var getVehicleOwners = newData.Where(x => x.ScreenName == "Vehicle Owners");

                if (getVehicleOwners.Any())
                {
                    // Collect parent Ids for "Maintenance" screens
                    var vehicleOwnerIds = getVehicleOwners.Select(x => x.Id).ToHashSet();

                    // Remove all "Maintenance" parents and their direct children
                    newData.RemoveAll(x => vehicleOwnerIds.Contains(x.Id) || vehicleOwnerIds.Contains(x.ParentsScreenId));
                }

                var getLPR = newData.Where(x => x.ScreenName == "LPR");

                if (getLPR.Any())
                {
                    // Collect parent Ids for "LPR" screens
                    var lprIds = getLPR.Select(x => x.Id).ToHashSet();

                    // Remove all "LPR" parents and their direct children
                    newData.RemoveAll(x => lprIds.Contains(x.Id) || lprIds.Contains(x.ParentsScreenId));
                }

                newData.RemoveAll(x => x.ScreenName == ScreenNames.View_and_Configure_ANPR || x.ScreenName == ScreenNames.UpdateMaintenanceDevice || x.ScreenName == ScreenNames.ManuallyOpenGate
                 || x.ScreenName == ScreenNames.ViewAuditLogsANPRVehicle || x.ScreenName == ScreenNames.ViewAuditLogsvehicleOwner);

            }
            result.data = newData;

            return StandardAPIResponse<IEnumerable<ScreenMaster>>.SuccessResponse(result.data, "", StatusCodes.Status200OK, ReferenceData: result.referenceData);
        }

        [HttpPost]
        [Route("AddRolePermission")]
        [CustomAuthorize([ScreenNames.ScreenPermission])]
        public async Task<ActionResult<StandardAPIResponse<string>>> AddRolePermission([FromBody] List<RoleScreenMappingRequestDto> roleScreenMappingRequestDto)
        {
            var userId = _currentUserService.UserId;
            var result = await _roleScreenMappingService.AddUpdateRoleScreenMapping(roleScreenMappingRequestDto, userId);
            if (result == null || result.Count() == 0)
            {
                return StandardAPIResponse<string>.ErrorResponse(null, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);
            }
            _permissionService.RefreshPermissionData();

            return StandardAPIResponse<string>.SuccessResponse(result, _localizer[MessageKeys.RecordUpdated], StatusCodes.Status200OK);

        }


        [HttpGet("FloorRoleScreenMappings/{roleId}")]
        [CustomAuthorize([ScreenNames.FloorZonePermission])]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<DataAccessPermissionResponseModel>>>> GetFloorRoleScreenMappingsAsync(string roleId)
        {
            var result = await _roleScreenMappingService.GetFloorRoleScreenMappingsByRoleIdAsync(roleId);
            if (result == null || result.Count() == 0)
            {
                return StandardAPIResponse<IEnumerable<DataAccessPermissionResponseModel>>.ErrorResponse(null, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);
            }
            return StandardAPIResponse<IEnumerable<DataAccessPermissionResponseModel>>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }


        [HttpPost]
        [Route("FloorRolePermission")]
        [CustomAuthorize([ScreenNames.FloorZonePermission])]
        public async Task<ActionResult<StandardAPIResponse<string>>> FloorRolePermission([FromBody] SaveFloorRoleMappingRequest floorRoleScreenMappingRequest)
        {
            var userId = _currentUserService.UserId;
            var result = await _roleScreenMappingService.UpdateFloorRoleScreenMappingAsync(floorRoleScreenMappingRequest, userId);
            if (!string.IsNullOrEmpty(result))
            {
                return StandardAPIResponse<string>.ErrorResponse(null, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);
            }
            _permissionService.RefreshPermissionData();
            return StandardAPIResponse<string>.SuccessResponse(result, _localizer[MessageKeys.RecordUpdated], StatusCodes.Status200OK);

        }

        [HttpPost]
        [Route("Widget")]
        [CustomAuthorize([ScreenNames.WidgetPermission])]
        public async Task<ActionResult<StandardAPIResponse<string>>> WidgetRolePermission([FromBody] SaveWidgetAccessPermissionRequest widgetRoleScreenMappingRequest)
        {
            var userId = _currentUserService.UserId;
            var result = await _roleScreenMappingService.UpdateWidgetRoleScreenMappingAsync(widgetRoleScreenMappingRequest, userId);
            if (!string.IsNullOrEmpty(result))
            {
                return StandardAPIResponse<string>.ErrorResponse(null, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);
            }
            _permissionService.RefreshPermissionData();
            _permissionService.RefreshWidgetPermissionData();
            return StandardAPIResponse<string>.SuccessResponse(result, _localizer[MessageKeys.RecordUpdated], StatusCodes.Status200OK);
        }

        [HttpGet("Widget/{roleId}")]
        [CustomAuthorize([ScreenNames.WidgetPermission])]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<WidgetAccessPermissionResponse>>>> GetWidgetRoleScreenMappingsAsync(string roleId)
        {
            var result = await _roleScreenMappingService.GetWidgetsByRoleIdAsync(roleId);
            if (result == null || !result.Any())
            {
                return StandardAPIResponse<IEnumerable<WidgetAccessPermissionResponse>>
                    .ErrorResponse(null, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);
            }

            var licenseData = _permissionService._licenseData;

            IEnumerable<WidgetMaster> widgetCategories = new List<WidgetMaster>();
            try
            {
                widgetCategories = await _widgetRepository.GetAllAsync();

                widgetCategories = widgetCategories
                    .Where(c => c != null
                        && !string.IsNullOrWhiteSpace(c.CategoryName)
                        && c.Widgets != null)
                    .ToList();
            }
            catch (Exception ex)
            {
                var mss = ex.Message;
            }

            if (widgetCategories.Any() && (!(licenseData?.IsMaintenance ?? false) || !(licenseData?.IsANPR ?? false)))
            {
                // Build list of category names to exclude based on license conditions
                var excludedCategories = new List<string>();

                if (!(licenseData?.IsMaintenance ?? false))
                    excludedCategories.Add(ScreenNames.Maintenance);

                if (!(licenseData?.IsANPR ?? false))
                    excludedCategories.Add(ScreenNames.ANPR);

                if (excludedCategories.Any())
                {
                    var maintenanceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    var excludedWidgetCategories = widgetCategories
                        .Where(c => excludedCategories.Contains(c.CategoryName, StringComparer.OrdinalIgnoreCase))
                        .ToList();

                    foreach (var category in excludedWidgetCategories)
                    {
                        if (category == null) continue;

                        if (!string.IsNullOrWhiteSpace(category.Id))
                            maintenanceIds.Add(category.Id);

                        var widgets = category.Widgets ?? Enumerable.Empty<WidgetItem>();
                        foreach (var widget in widgets)
                        {
                            if (widget == null) continue;

                            if (!string.IsNullOrWhiteSpace(widget.WidgetId))
                                maintenanceIds.Add(widget.WidgetId);
                        }
                    }

                    if (maintenanceIds.Any())
                    {
                        result = result
                            .Where(r => r != null && !maintenanceIds.Contains(r.Id ?? string.Empty))
                            .Select(r => new WidgetAccessPermissionResponse
                            {
                                Id = r.Id,
                                CategoryName = r.CategoryName,
                                AccessAllowed = r.AccessAllowed,
                                Widgets = (r.Widgets ?? Enumerable.Empty<WidgetAccessPermissionResponse.WidgetItem>())
                                    .Where(w => w != null && !maintenanceIds.Contains(w.WidgetId ?? string.Empty))
                                    .ToList()
                            })
                            .ToList();
                    }
                }
            }

            return StandardAPIResponse<IEnumerable<WidgetAccessPermissionResponse>>
                .SuccessResponse(result, "", StatusCodes.Status200OK);
        }
    }

}
