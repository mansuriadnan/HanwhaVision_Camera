using HanwhaClient.Application;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Application.Services;
using HanwhaClient.Helper;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using HanwhaClient.Model.License;
using HanwhaClient.Model.Role;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IHttpContextAccessor _context;
        private readonly IStringLocalizer<AppMessages> _localizer;
        private readonly IWidgetRepository _widgetRepository;
        private readonly IPermissionService _permissionService;


        public RoleController(IRoleService roleService, ICurrentUserService currentUserService, IHttpContextAccessor context, IStringLocalizer<AppMessages> localizer, IWidgetRepository widgetRepository, IPermissionService permissionService)
        {
            _roleService = roleService;
            _currentUserService = currentUserService;
            _context = context;
            _localizer = localizer;
            _widgetRepository = widgetRepository;
            _permissionService = permissionService;
        }

        [HttpGet("GetAllRoles")]
        [CustomAuthorize([ScreenNames.RoleMaster, ScreenNames.UserMaster])]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<RoleMaster>>>> GetAllRoles()
        {
            var data = await _roleService.GetRolesAsync();
            if (data != null && data.Any())
            {
                return StandardAPIResponse<IEnumerable<RoleMaster>>.SuccessResponse(data, "");
            }
            return StandardAPIResponse<IEnumerable<RoleMaster>>.ErrorResponse(null, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status400BadRequest);
        }

        [HttpPost("AddUpdateRole")]
        [CustomAuthorize([ScreenNames.AddorUpdateRole])]
        public async Task<ActionResult<StandardAPIResponse<string>>> AddUpdateRole(RoleRequestModel roleRequest)
        {
            var userId = _currentUserService.UserId;
            var data = await _roleService.SaveRoleAsync(roleRequest, userId);

            if (string.IsNullOrEmpty(data.ErrorMessage))
            {
                return StandardAPIResponse<string>.SuccessResponse(data.RoleId, string.IsNullOrEmpty(roleRequest.Id) ? _localizer[MessageKeys.RecordUpdated] : _localizer[MessageKeys.RecordAdded]);
            }
            return StandardAPIResponse<string>.ErrorResponse(null, data.ErrorMessage, StatusCodes.Status400BadRequest);
        }

        [HttpPost]
        [Route("DeleteRole")]
        [CustomAuthorize([ScreenNames.DeleteRole])]
        public async Task<ActionResult<StandardAPIResponse<int>>> DeleteRole(DeleteRequestDto dto)
        {
            var userId = _currentUserService.UserId;
            var result = await _roleService.DeleteRoleAsync(dto.Id, userId);

            return result switch
            {
                1 => StandardAPIResponse<int>.SuccessResponse(result, _localizer[MessageKeys.RecordDeleted]),
                2 => StandardAPIResponse<int>.ErrorResponse(result, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status400BadRequest),
                _ => StandardAPIResponse<int>.ErrorResponse(result, _localizer[MessageKeys.RoleExists], StatusCodes.Status400BadRequest),
            };
        }


        [HttpGet("GetPermissions")]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<RolePermissionResponseModel>>>> GetRolePermissions()
        {
            var data = await _roleService.GetRolePermissionAsync();
            return StandardAPIResponse<IEnumerable<RolePermissionResponseModel>>.SuccessResponse(data, _localizer[MessageKeys.RecordRetrieved]);
        }

        [HttpPost("UploadFile")]
        public IActionResult UploadFile([FromForm] IFormFileCollection files, RolePermissionResponseModel model)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest(StandardAPIResponse<string>.ErrorResponse(null, _localizer[MessageKeys.FileUploaded], StatusCodes.Status400BadRequest));
            }
            return Ok(new { Message = _localizer[MessageKeys.FileUploaded], FileName = files[0].FileName });
        }

        [HttpGet("UserRolePermissions")]
        public async Task<ActionResult<StandardAPIResponse<UserRolePermissionResponseDto>>> GetUserRolePermissions()
        {
            // string roleName = _context.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "role")?.Value;
            var roleNames = _context.HttpContext.User.Claims
                            .Where(x => x.Type == "role")
                            .Select(x => x.Value)
                            .ToList();

            var result = await _roleService.GetUserRolePermission(roleNames);

            var widgetCategories = await _widgetRepository.GetAllAsync();

            var licenseData = _permissionService._licenseData;

            var allScreens = result.ScreensPermission;
            var allWidgets = result.WidgetsPermission;

            if (!licenseData.IsANPR ?? false)
            {
                var getVehicleOwners = allScreens.Find(x => x.ScreenName == "Vehicle Owners");
                if (getVehicleOwners != null)
                {
                    // Remove both from the list (if found)
                    allScreens.Remove(getVehicleOwners);
                    allScreens.RemoveAll(x => x.ParentsScreenId == getVehicleOwners.Id);
                }
            }

            if (!licenseData.IsMaintenance ?? false)
            {
                var getResult = allScreens.Find(x => x.ScreenName == "Maintenance");
                if (getResult != null)
                {
                    // Remove both from the list (if found)
                    allScreens.Remove(getResult);
                    allScreens.RemoveAll(x => x.ParentsScreenId == getResult.Id);
                }
                
                // Collect all widget IDs from *all* "Maintenance" categories + the categories' own Ids
                var maintenanceIds = widgetCategories
                    .Where(c => c.CategoryName == "Maintenance")
                    .SelectMany(c =>
                    {
                        // Collect widget ids from this category (null-safe)
                        var widgetIds = (c.Widgets ?? Enumerable.Empty<WidgetItem>())
                            .Select(w => w.WidgetId);

                        // Include the category's own Id
                        return widgetIds.Append(c.Id);
                    })
                    .ToHashSet(); // Fast membership checks

                if (maintenanceIds.Count > 0)
                {
                    // Remove any widget whose WidgetId is in maintenanceIds
                    allWidgets.RemoveAll(w => maintenanceIds.Contains(w.WidgetId));
                }
            }

            result = new UserRolePermissionResponseDto
            {
                ScreensPermission = allScreens,
                WidgetsPermission = allWidgets
            };

            if (result == null)
            {
                return StandardAPIResponse<UserRolePermissionResponseDto>.ErrorResponse(null, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);
            }
            return StandardAPIResponse<UserRolePermissionResponseDto>.SuccessResponse(result, "", StatusCodes.Status200OK);
        }
    }
}
