using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface IUsersRepository : IRepositoryBase<UserMaster>
    {
        Task<UserMaster> GetUserByUsernameAsync(string username,CancellationToken cancellationToken);
        Task<bool> IsUsernameExistAsync(string username, string userId = null);
        Task<bool> IsEmailnameExistAsync(string username, string userId = null);
        Task<UserMaster> GetUserByEmailAsync(string email);
        Task UpdatePasswordAsync(string userId, string newPasswordHash);
        Task<UserMaster> GetUserByUserIdAsync(string userId);
        Task<bool> IsRoleExistAsync(string roleId);
        Task<int> GetUserCountAsync(string username);
        Task<IEnumerable<UserMaster>> GetAllUserAsync(string username);

    }
}
