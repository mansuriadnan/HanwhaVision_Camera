using HanwhaClient.Application;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Helper;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CacheController : Controller
    {
        private readonly ICacheService _cacheService;
        private readonly IStringLocalizer<AppMessages> _localizer;

        public CacheController(ICacheService cacheService, IStringLocalizer<AppMessages> localizer)
        {
            _cacheService = cacheService;
            _localizer = localizer;
        }

        [HttpGet]
        [Route("ClearCache")]
        public async Task<ActionResult<StandardAPIResponse<bool>>> ClearCacheAsync()
        {
            await _cacheService.ClearAsync();
            return StandardAPIResponse<bool>.SuccessResponse(true, _localizer[MessageKeys.ApplicationCacheClear]);
        }
    }
}
