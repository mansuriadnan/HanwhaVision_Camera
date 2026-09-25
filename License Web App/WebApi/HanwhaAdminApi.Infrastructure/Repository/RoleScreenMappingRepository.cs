using HanwhaAdminApi.Infrastructure.Connection;
using HanwhaAdminApi.Infrastructure.Interfaces;
using HanwhaAdminApi.Model.Common;
using HanwhaAdminApi.Model.DbEntities;
using HanwhaAdminApi.Model.Dto;
using MongoDB.Driver;

namespace HanwhaAdminApi.Infrastructure.Repository
{
    public class RoleScreenMappingRepository : RepositoryBase<RoleScreenMapping>, IRoleScreenMappingRepository
    {
        public RoleScreenMappingRepository(MongoDbConnectionService mongoDbConnectionService) : base(mongoDbConnectionService, AppDBConstants.RoleScreenMapping)
        {
        }

        public async Task<long> DeleteRoleScreenMappingByRoleIdAsync(string roleId, string userId)
        {
            var filter = Builders<RoleScreenMapping>.Filter.Eq(x => x.RoleId, roleId);
            var update = Builders<RoleScreenMapping>.Update
                     .Set(c => c.IsDeleted, true)
                     .Set(c => c.UpdatedOn, DateTime.Now)
                     .Set(c => c.UpdatedBy, userId)
                     .Set(c => c.DeletedOn, DateTime.Now);

            var updatedData = await dbEntity.UpdateOneAsync(filter, update);
            return updatedData.ModifiedCount;
        }

        public async Task<RoleScreenMapping> GetRoleScreenMappingAsync(string roleId)
        {
            var filter = Builders<RoleScreenMapping>.Filter.Eq(x => x.RoleId, roleId);
            var data = await dbEntity.Find(filter).FirstOrDefaultAsync();
            return data;
        }

        public async Task<long> SaveRoleScreenMappingAsync(string roleId, IEnumerable<ScreenMapping> obj, string userId)
        {
            var filter = Builders<RoleScreenMapping>.Filter.Eq(x => x.RoleId, roleId);

            var update = Builders<RoleScreenMapping>.Update
            .Set(c => c.ScreenMappings, obj)
                    .Set(c => c.UpdatedOn, DateTime.Now)
                    .Set(c => c.UpdatedBy, userId);
            var updatedData = await dbEntity.UpdateOneAsync(filter, update);
            return updatedData.ModifiedCount;
        }

        public async Task<long> UpdateRegionScreenMappingAsync(SaveRegionRequest saveRegionRequest, string userId)
        {
            var filter = Builders<RoleScreenMapping>.Filter.Eq(x => x.RoleId, saveRegionRequest.RoleId);

            var update = Builders<RoleScreenMapping>.Update
            .Set(c => c.RegionPermissions, saveRegionRequest.RegionPermissions)
                    .Set(c => c.UpdatedOn, DateTime.Now)
                    .Set(c => c.UpdatedBy, userId);
            var updatedData = await dbEntity.UpdateOneAsync(filter, update);
            return updatedData.ModifiedCount;
        }

        public async Task<long> DeleteRegionPermissionByRoleIdAsync(string roleId,string regionId,string userId)
        {
            var filter = Builders<RoleScreenMapping>.Filter.Eq(x => x.RoleId, roleId);

            var update = Builders<RoleScreenMapping>.Update
                .PullFilter(
                    x => x.RegionPermissions,
                    rp => rp.RegionIds == regionId
                )
                .Set(x => x.UpdatedOn, DateTime.Now)
                .Set(x => x.UpdatedBy, userId);

            var result = await dbEntity.UpdateOneAsync(filter, update);
            return result.ModifiedCount;
        }


    }
}
