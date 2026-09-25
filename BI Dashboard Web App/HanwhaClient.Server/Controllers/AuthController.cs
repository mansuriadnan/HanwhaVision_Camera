using HanwhaClient.Application;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Model.Auth;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IStringLocalizer<AppMessages> _localizer;

        public AuthController(IAuthService authService, IStringLocalizer<AppMessages> localizer)
        {
            this._authService = authService;
            _localizer = localizer;
        }

        [HttpPost("login")]
        public async Task<ActionResult<StandardAPIResponse<TokenResponseModel>>> LoginAsync([FromBody] LoginRequestModel loginModel)
        {
            try
            {
                var result = await _authService.LoginAsync(loginModel.Username, loginModel.Password);
                if (string.IsNullOrEmpty(result.ErrorMessage))
                {
                    var response = StandardAPIResponse<TokenResponseModel>.SuccessResponse(result, _localizer[MessageKeys.UserLoggedIn]);
                    return response;
                }
                else
                {
                    var response = StandardAPIResponse<TokenResponseModel>.ErrorResponse(null, result.ErrorMessage);
                    return response;
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                throw;
            }
        }
        
        [HttpPost("GetFloorZoneDataAccessPermission")]
        public async Task<ActionResult<StandardAPIResponse<FloorZoneDataAccessPermissionForLinkedServer>>> GetFloorZoneDataAccessPermissionAsync([FromBody] LoginRequestModel loginModel,CancellationToken cancellationToken)
        {
            try
            {
                var result = await _authService.GetFloorZoneDataAccessPermissionAsync(loginModel.Username, loginModel.Password, cancellationToken);
                if (string.IsNullOrEmpty(result.ErrorMessage))
                {
                    var response = StandardAPIResponse<FloorZoneDataAccessPermissionForLinkedServer>.SuccessResponse(result, _localizer[MessageKeys.UserLoggedIn]);
                    return response;
                }
                else
                {
                    var response = StandardAPIResponse<FloorZoneDataAccessPermissionForLinkedServer>.ErrorResponse(null, result.ErrorMessage);
                    return response;
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                throw;
            }
        }

        [HttpPost("refreshToken")]
        public async Task<ActionResult<StandardAPIResponse<TokenResponseModel>>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            if (string.IsNullOrEmpty(result.ErrorMessage))
            {
                var response = StandardAPIResponse<TokenResponseModel>.SuccessResponse(result, _localizer[MessageKeys.NewTokenGenerated]);
                return response;
            }
            else
            {
                var response = StandardAPIResponse<TokenResponseModel>.ErrorResponse(null, result.ErrorMessage, 401);
                return Unauthorized(response);
            }
        }

    }
}
