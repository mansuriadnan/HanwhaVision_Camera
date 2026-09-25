using DocumentFormat.OpenXml.InkML;
using HanwhaClient.Application;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestingController : ControllerBase
    {
        public TestingController()
        {
            
        }

        //Testing controlller for BasicAuthentication
        [Authorize(AuthenticationSchemes = "BasicAuthentication", Roles = "Admin,Editor")]
        [HttpGet("test")]
        public async Task<ActionResult<StandardAPIResponse<bool>>> GetUsers()
        {

            return StandardAPIResponse<bool>.SuccessResponse(true, "", StatusCodes.Status200OK);
        }
    }
}

    
