using HanwhaAdminApi.Application.Interfaces;
using HanwhaAdminApi.Helper;
using HanwhaAdminApi.Model.Common;
using HanwhaAdminApi.Model.DbEntities;
using HanwhaAdminApi.Model.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HanwhaAdminApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegionMasterController : ControllerBase
    {
        private readonly IRegionMasterService _regionMasterService;
        private readonly ICurrentUserService _currentUserService;


        public RegionMasterController(IRegionMasterService usersService,
            ICurrentUserService currentUserService)
        {
            this._regionMasterService = usersService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<RegionMasterResponseDto>>>> GetAllRegionAsync()
        {
            var result = await _regionMasterService.GetAllRegionAsync();
            var response = StandardAPIResponse<IEnumerable<RegionMasterResponseDto>>.SuccessResponse(result.data, AppMessageConstants.DataRetrieved, StatusCodes.Status200OK, ReferenceData: result.referenceData);
            return response;
        }

        [HttpPost]
        public async Task<ActionResult<StandardAPIResponse<string>>> CreateRegionAsync(RequestRegionMasterDto user)
        {
            var userId = _currentUserService.UserId;
            var data = await _regionMasterService.SaveRegionAsync(user, userId);
            if (string.IsNullOrEmpty(data.Id))
            {
                return StandardAPIResponse<string>.ErrorResponse(string.Empty, data.ErrorMessage, StatusCodes.Status400BadRequest);
            }
            else if (!string.IsNullOrEmpty(user.Id))
            {
                return StandardAPIResponse<string>.SuccessResponse(data.Id, AppMessageConstants.UpdateSuccess);
            }
            return StandardAPIResponse<string>.SuccessResponse(data.Id, AppMessageConstants.InsertSuccess);
        }


        [HttpDelete]
        [Route("{id}")]
        public async Task<ActionResult<StandardAPIResponse<bool>>> DeleteUser(string id)
        {
            var userId = _currentUserService.UserId;
            var data = await _regionMasterService.DeleteRegionAsync(id, userId);
            if (!data.IsSuccess)
            {
                return StandardAPIResponse<bool>.ErrorResponse(false, data.ErrorMessage, StatusCodes.Status404NotFound);
            }
            return StandardAPIResponse<bool>.SuccessResponse(data.IsSuccess, AppMessageConstants.DeleteSuccess);
        }

    }
}
