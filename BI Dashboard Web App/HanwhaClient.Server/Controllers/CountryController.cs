using HanwhaClient.Application;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Application.Services;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ICountryService _countryService;
        private readonly IStringLocalizer<AppMessages> _localizer;
        public CountryController(ICurrentUserService currentUserService,
            ICountryService countryService,
            IStringLocalizer<AppMessages> localizer)
        {
            _countryService = countryService;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        [HttpGet]
        [Route("")]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<CountryResponse>>>> GetCountryAsync()
        {
            var data = await _countryService.GetAllCountryAsync();
            if (data.Count() > 0)
            {
                return StandardAPIResponse<IEnumerable<CountryResponse>>.SuccessResponse(data, "", StatusCodes.Status200OK);
            }
            return StandardAPIResponse<IEnumerable<CountryResponse>>.SuccessResponse(null, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status404NotFound);
        }
    }
}
