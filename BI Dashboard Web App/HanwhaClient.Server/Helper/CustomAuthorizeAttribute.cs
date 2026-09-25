using HanwhaClient.Application;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Model.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace HanwhaClient.Helper
{
    public class CustomAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _fromPermission;
        private readonly string[] _screenName;

        public CustomAuthorizeAttribute(string[] screenName
            , string fromPermission = ""
            )
        {
            _screenName = screenName;
            _fromPermission = fromPermission;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Get services from the request context (Service Locator Pattern)
            var localizer = context.HttpContext.RequestServices
                .GetRequiredService<IStringLocalizer<AppMessages>>();

            var logger = context.HttpContext.RequestServices.GetService(typeof(ILogger<CustomAuthorizeAttribute>)) as ILogger<CustomAuthorizeAttribute>;

            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                LogAndSetUnauthorizedResult(context, logger, localizer, localizer[MessageKeys.UnauthorizedAccessAttempt]);
                return;
            }

            var userRoles = context.HttpContext.User.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                .Select(c => c.Value.ToLower())
                .ToList();

            if (!userRoles.Any())
            {
                LogAndSetUnauthorizedResult(context, logger, localizer, localizer[MessageKeys.InsufficientPermissions]);
                return;
            }

            var permissionService = context.HttpContext.RequestServices.GetService(typeof(IPermissionService)) as IPermissionService;

            if (string.IsNullOrEmpty(_fromPermission))
            {
                foreach (var userRole in userRoles)
                {
                    foreach (var screenName in _screenName)
                    {
                        //if (_roles.Contains(userRole) && permissionService != null && permissionService.checkPermission(userRole, _screenName))
                        if (permissionService != null && permissionService.checkPermission(userRole, screenName))
                        {
                            logger?.LogInformation("User with role {UserRole} authorized successfully.", userRole);
                            await Task.CompletedTask; // Exit early for the first valid role
                            return;
                        }
                    }
                }
            }
            else
            {
                foreach (var userRole in userRoles)
                {
                    foreach (var screenName in _screenName)
                    {
                        if (permissionService != null && permissionService.CheckWidgetPermission(userRole, _fromPermission, screenName))
                        {
                            logger?.LogInformation("User with role {UserRole} authorized successfully.", userRole);
                            await Task.CompletedTask; // Exit early for the first valid role
                            return;
                        }
                    }
                }

            }

            LogAndSetUnauthorizedResult(context, logger, localizer, localizer[MessageKeys.InsufficientPermissions]);
        }


        private static void LogAndSetUnauthorizedResult(
     AuthorizationFilterContext context,
     ILogger? logger,
     IStringLocalizer<AppMessages> localizer,
     string messageKey,
     string? userRole = null)
        {
            // Get localized message
            var localizedMessage = localizer[messageKey].Value;

            var responseData = StandardAPIResponse<bool>.ErrorResponse(
                false,
                "Denied",
                StatusCodes.Status401Unauthorized,
                new List<string> { localizedMessage });

            context.Result = new UnauthorizedObjectResult(responseData);

            if (!string.IsNullOrEmpty(userRole))
            {
                logger?.LogError("Forbidden access attempt by user with role: {UserRole}", userRole);
            }
            else
            {
                logger?.LogError(localizedMessage);
            }
        }
    }
}