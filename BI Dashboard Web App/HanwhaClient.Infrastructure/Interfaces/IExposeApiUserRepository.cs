using HanwhaClient.Model.DbEntities;

namespace HanwhaClient.Infrastructure.Interfaces
{
    public interface IExposeApiUserRepository : IRepositoryBase<ExposeApiUser>
    {
        Task<bool> IsUsernameExistsAsync(string username, string excludeId = null);
        Task<ExposeApiUser> GetUserByUsernameAsync(string username);
    }
}
