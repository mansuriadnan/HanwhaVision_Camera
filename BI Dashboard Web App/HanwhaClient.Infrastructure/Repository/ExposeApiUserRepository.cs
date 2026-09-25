using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HanwhaClient.Infrastructure.Repository
{
    public class ExposeApiUserRepository : RepositoryBase<ExposeApiUser>, IExposeApiUserRepository
    {
        public ExposeApiUserRepository(MongoDbConnectionService mongoDbConnectionService) : base(mongoDbConnectionService, AppDBConstants.ExposeApiUser)
        {

        }

        public async Task<bool> IsUsernameExistsAsync(string username, string excludeId = null)
        {
            var filterBuilder = Builders<ExposeApiUser>.Filter;
            var filter = filterBuilder.Regex(u => u.Username, new BsonRegularExpression($"^{username}$", "i")) &
                         filterBuilder.Eq(u => u.IsDeleted, false);

            if (!string.IsNullOrEmpty(excludeId))
            {
                filter &= filterBuilder.Ne(u => u.Id, excludeId);
            }

            var count = await dbEntity.CountDocumentsAsync(filter);
            return count > 0;
        }

        public async Task<ExposeApiUser> GetUserByUsernameAsync(string username)
        {
            var filterBuilder = Builders<ExposeApiUser>.Filter;
            // Since username authentication is case-sensitive as per existing code
            var filter = filterBuilder.Eq(u => u.Username, username) &
                         filterBuilder.Eq(u => u.IsDeleted, false);

            return await dbEntity.Find(filter).FirstOrDefaultAsync();
        }
    }
}
