using HanwhaClient.Application;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Helper;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SiteController : ControllerBase
    {
        private readonly ISiteService _siteService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<AppMessages> _localizer;


        public SiteController(ISiteService siteService, ICurrentUserService currentUserService, IStringLocalizer<AppMessages> localizer)
        {
            _siteService = siteService;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        [HttpGet]
        [CustomAuthorize([ScreenNames.ViewChildSubChildSite])]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<SiteDto>>>> GetAll()
        {

            var result = await _siteService.GetAllSitesAsync();
            if (result.data.Count() == 0)
            {
                return StandardAPIResponse<IEnumerable<SiteDto>>.SuccessResponse(result.data, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status200OK, ReferenceData: result.referenceData);
            }
            else if (result.data.Count() > 0)
            {
                return StandardAPIResponse<IEnumerable<SiteDto>>.SuccessResponse(result.data, "", StatusCodes.Status200OK, ReferenceData: result.referenceData);
            }
            return StandardAPIResponse<IEnumerable<SiteDto>>.ErrorResponse(null, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status200OK);

        }

        [HttpPost("AddOrUpdateChildSite")]
        [CustomAuthorize([ScreenNames.AddOrUpdateChildSubChildSite])]
        public async Task<ActionResult<StandardAPIResponse<string>>> AddOrUpdateChildSite([FromBody] SiteChileRequestDto siteDto)
        {
            var userId = _currentUserService.UserId;
            var data = await _siteService.AddOrUpdateSiteAsync(siteDto, userId);

            if (data.data == "")
            {
                return StandardAPIResponse<string>.ErrorResponse(null, data.errorMessage, StatusCodes.Status404NotFound);
            }
            return StandardAPIResponse<string>.SuccessResponse("", data.errorMessage, StatusCodes.Status200OK);
        }

        [HttpPost("AddOrUpdateSubChildSite")]
        [CustomAuthorize([ScreenNames.AddOrUpdateChildSubChildSite])]
        public async Task<ActionResult<StandardAPIResponse<string>>> AddOrUpdateSubChildSite([FromBody] ChildSiteDto childSiteDto)
        {
            if (childSiteDto != null)
            {
                var userId = _currentUserService.UserId;
                var data = await _siteService.AddOrUpdateChildSiteAsync(childSiteDto, userId);
                if (!string.IsNullOrEmpty(data.errorMessage))
                {
                    return StandardAPIResponse<string>.ErrorResponse(null, data.errorMessage, StatusCodes.Status500InternalServerError);
                }
                return StandardAPIResponse<string>.SuccessResponse("", string.IsNullOrEmpty(childSiteDto.Id) ? _localizer[MessageKeys.RecordAdded] : _localizer[MessageKeys.RecordUpdated], StatusCodes.Status200OK);
            }
            return StandardAPIResponse<string>.ErrorResponse(null, _localizer[MessageKeys.InvalidDataModel], StatusCodes.Status404NotFound);

        }

        [HttpPost]
        [Route("DeleteChildSite")]
        [CustomAuthorize([ScreenNames.DeleteChildSubChildSite])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> DeleteChildSite(SiteDeleteReqDto dto)
        {
            try
            {
                var userId = _currentUserService.UserId;
                var isDeleted = await _siteService.DeleteChildSiteAsync(dto.ParentSiteId, userId);
                if (!isDeleted)
                {
                    return StandardAPIResponse<bool>.ErrorResponse(false, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);
                }
                return StandardAPIResponse<bool>.SuccessResponse(true, _localizer[MessageKeys.RecordDeleted], StatusCodes.Status200OK);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [Route("DeleteSubChildSite")]
        [CustomAuthorize([ScreenNames.DeleteChildSubChildSite])]
        public async Task<ActionResult<StandardAPIResponse<bool>>> DeleteSubChildSite(SiteDeleteReqDto dto)
        {
            try
            {
                var userId = _currentUserService.UserId;
                var isDeleted = await _siteService.DeleteSubChildSiteAsync(dto.ParentSiteId, dto.ChildSiteId, userId);
                if (!isDeleted)
                {
                    return StandardAPIResponse<bool>.ErrorResponse(false, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);
                }
                return StandardAPIResponse<bool>.SuccessResponse(true, _localizer[MessageKeys.RecordDeleted], StatusCodes.Status200OK);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
