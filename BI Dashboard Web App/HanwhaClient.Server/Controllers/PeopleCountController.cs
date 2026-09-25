using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using HanwhaClient.Application;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Application.Services;
using HanwhaClient.Helper;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.Dto;
using HanwhaClient.Model.Role;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeopleCountController : ControllerBase
    {
        private readonly IPeopleCountService _peopleCountService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<AppMessages> _localizer;


        public PeopleCountController(IPeopleCountService peopleCountService, ICurrentUserService currentUserService, IStringLocalizer<AppMessages> localizer)
        {
            _peopleCountService = peopleCountService;
            _currentUserService = currentUserService;
            _localizer = localizer;
        }

        [HttpGet]
        [Route("GetAllPeopleCount/{selectedDate}")]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<PeopleCountDto>>>> GetAllPeopleCount(string? selectedDate)
        {
            var result = await _peopleCountService.GetCamerasBySelectedDateAsync(selectedDate);
            if (result.Count() > 0)
            {
                return StandardAPIResponse<IEnumerable<PeopleCountDto>>.SuccessResponse(result, _localizer[MessageKeys.RecordRetrieved], StatusCodes.Status200OK);
            }
            return StandardAPIResponse<IEnumerable<PeopleCountDto>>.ErrorResponse(result, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status400BadRequest);
        }

        //[HttpPost]
        //public async Task<ActionResult<StandardAPIResponse<string>>> InsertPeople(PeopleCountCreateDto createDto)
        //{
        //    var userId = _currentUserService.UserId;

        //    if (createDto == null || createDto.Lines == null || createDto.Lines.Count == 0)
        //    {
        //        return BadRequest("Invalid data");
        //    }

        //    var data = await _peopleCountService.InsertPeople(createDto, userId ?? "");
        //    if (!string.IsNullOrEmpty(data))
        //    {
        //        return StandardAPIResponse<string>.SuccessResponse(data, _localizer[MessageKeys.InsertUpdateSuccess, StatusCodes.Status200OK);
        //    }
        //    return StandardAPIResponse<string>.ErrorResponse("", _localizer[MessageKeys.SomethingWentWrong, StatusCodes.Status400BadRequest);
        //}

        [HttpGet]
        [Route("")]
        [CustomAuthorize([ScreenNames.CanViewPeopleCameraCount])]
        public async Task<ActionResult<StandardAPIResponse<IEnumerable<PeopleCountDto>>>> GetPeopleCountByZoneIdOrCameraId(string? zoneId, string? cameraId)
        {
            var result = await _peopleCountService.GetPeopleCountByZoneIdOrCameraId(zoneId, cameraId);

            if (result.Count() > 0)
            {
                return StandardAPIResponse<IEnumerable<PeopleCountDto>>.SuccessResponse(result, _localizer[MessageKeys.RecordRetrieved], StatusCodes.Status200OK);
            }
            return StandardAPIResponse<IEnumerable<PeopleCountDto>>.ErrorResponse(result, _localizer[MessageKeys.RecordNotFound], StatusCodes.Status400BadRequest);
        }
    }
}
