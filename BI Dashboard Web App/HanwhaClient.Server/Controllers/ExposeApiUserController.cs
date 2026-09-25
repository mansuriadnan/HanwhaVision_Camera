using HanwhaClient.Application.Interfaces;
using HanwhaClient.Application.Services;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HanwhaClient.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExposeApiUserController : ControllerBase
    {
        private readonly IExposeApiUserService _exposeApiUserService;
        private readonly ICurrentUserService _currentUserService;

        public ExposeApiUserController(IExposeApiUserService exposeApiUserService,
            ICurrentUserService currentUserService)
        {
            _exposeApiUserService = exposeApiUserService;
            _currentUserService = currentUserService;
        }

        [HttpPost("add-edit")]
        public async Task<IActionResult> AddOrUpdateUser([FromBody] ExposeApiUserDto userDto)
        {
            // Fetch current user id if available in claims, otherwise default to "system" or empty
            string currentUserId = _currentUserService.UserId;

            var response = await _exposeApiUserService.AddOrUpdateUserAsync(userDto, currentUserId);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteUser(DeleteRequestDto id)
        {
            string currentUserId = _currentUserService.UserId;

            var response = await _exposeApiUserService.DeleteUserAsync(id.Id, currentUserId);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var response = await _exposeApiUserService.GetAllUsersAsync();
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
    }
}
