using HanwhaClient.Model.Common;
using HanwhaClient.Model.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Interfaces
{
    public interface IExposeApiUserService
    {
        Task<StandardAPIResponse<ExposeApiUserDto>> AddOrUpdateUserAsync(ExposeApiUserDto userDto, string currentUserId);
        Task<StandardAPIResponse<bool>> DeleteUserAsync(string id, string currentUserId);
        Task<StandardAPIResponse<List<ExposeApiUserDto>>> GetAllUsersAsync();
        Task<StandardAPIResponse<ExposeApiUserDto>> GetUserByUsernameAsync(string username);
    }
}
