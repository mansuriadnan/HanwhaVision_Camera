using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using MongoDB.Driver;

namespace HanwhaClient.Infrastructure.Repository
{
    public class UsersRepository : RepositoryBase<UserMaster>, IUsersRepository
    {
        public UsersRepository(MongoDbConnectionService mongoDbConnectionService) : base(mongoDbConnectionService, AppDBConstants.UserMaster)
        {

        }

        //public async Task<UserMaster> GetUserByUsernameAsync(string username)
        //{
        //    var filter = Builders<UserMaster>.Filter.And(
        //                 Builders<UserMaster>.Filter.Eq(x => x.IsDeleted, false),
        //                 Builders<UserMaster>.Filter.Or(
        //                 Builders<UserMaster>.Filter.Eq(x => x.Username, username),
        //                 Builders<UserMaster>.Filter.Eq(x => x.Email, username)));

        //    var projection = Builders<UserMaster>.Projection
        //                    .Include(u => u.Id) // include only needed fields
        //                    .Include(u => u.Username) // include only needed fields
        //                    .Include(u => u.Password)
        //                    .Include(u => u.RoleIds);   // example

        //    var excludeSysadminFilter = Builders<UserMaster>.Filter.And(
        //                                Builders<UserMaster>.Filter.Ne(x => x.Username, "sysadmin"),
        //                                Builders<UserMaster>.Filter.Ne(x => x.Email, "systemadmin@example.com"));

        //    var combinedFilter = Builders<UserMaster>.Filter.And(filter, excludeSysadminFilter);

        //    var result = await dbEntity
        //        .Find(combinedFilter)
        //        .Project<UserMaster>(projection)
        //        .FirstOrDefaultAsync();


        //    //var data = await dbEntity.Find(filter).FirstOrDefaultAsync();
        //    return result;
        //}

        public async Task<UserMaster> GetUserByUsernameAsync(string username, CancellationToken cancellationToken)
        {
            var filter = Builders<UserMaster>.Filter.And(
                Builders<UserMaster>.Filter.Eq(x => x.IsDeleted, false),
                Builders<UserMaster>.Filter.Or(
                    Builders<UserMaster>.Filter.Eq(x => x.Username, username),
                    Builders<UserMaster>.Filter.Eq(x => x.Email, username)));

            var projection = Builders<UserMaster>.Projection
                .Include(u => u.Id)
                .Include(u => u.Username)
                .Include(u => u.Password)
                .Include(u => u.RoleIds);

            var excludeSysadminFilter = Builders<UserMaster>.Filter.And(
                Builders<UserMaster>.Filter.Ne(x => x.Username, "sysadmin"),
                Builders<UserMaster>.Filter.Ne(x => x.Email, "systemadmin@example.com"));

            var combinedFilter = Builders<UserMaster>.Filter.And(filter, excludeSysadminFilter);

            return await dbEntity
                .Find(combinedFilter)
                .Project<UserMaster>(projection)
                .FirstOrDefaultAsync(cancellationToken); // ✅ PASS TOKEN HERE
        }

        public async Task<bool> IsUsernameExistAsync(string username, string userId = null)
        {
            //var filter = Builders<UserMaster>.Filter.Eq(x => x.Username, username.ToLower());
            var filter = Builders<UserMaster>.Filter.Where(x => x.Username == username && x.IsDeleted != true);

            var options = new FindOptions
            {
                Collation = new Collation("en", strength: CollationStrength.Secondary) // Case-insensitive issue
            };

            if (!string.IsNullOrEmpty(userId))
            {
                filter = Builders<UserMaster>.Filter.And(
                    filter,
                    Builders<UserMaster>.Filter.Ne(x => x.Id, userId));
            }
            var data = await dbEntity.Find(filter, options).AnyAsync();
            return data;
        }
        public async Task<bool> IsEmailnameExistAsync(string email, string userId = null)
        {
            //var filter = Builders<UserMaster>.Filter.Eq(x => x.Email, email);
            var filter = Builders<UserMaster>.Filter.And(
                         Builders<UserMaster>.Filter.Eq(x => x.Email, email),
                         Builders<UserMaster>.Filter.Or(
                         Builders<UserMaster>.Filter.Eq(x => x.IsDeleted, false),
                         Builders<UserMaster>.Filter.Eq(x => x.IsDeleted, (bool?)null)));


            var options = new FindOptions
            {
                Collation = new Collation("en", strength: CollationStrength.Secondary) // Case-insensitive issue
            };

            if (!string.IsNullOrEmpty(userId))
            {
                filter = Builders<UserMaster>.Filter.And(
                    filter,
                    Builders<UserMaster>.Filter.Ne(x => x.Id, userId));
            }
            var data = await dbEntity.Find(filter, options).AnyAsync();
            return data;
        }

        public async Task<UserMaster> GetUserByEmailAsync(string email)
        {
            var filter = Builders<UserMaster>.Filter.Eq(x => x.Email, email);
            var data = await dbEntity.Find(filter).FirstOrDefaultAsync();
            return data;
        }

        public async Task UpdatePasswordAsync(string userId, string newPasswordHash)
        {
            var filter = Builders<UserMaster>.Filter.Eq(x => x.Id, userId);
            var update = Builders<UserMaster>.Update
                .Set(x => x.Password, newPasswordHash)
                .Set(x => x.UpdatedOn, DateTime.UtcNow)
                .Set(x => x.UpdatedBy, userId)
                .Set(x => x.IsPasswordReset, true);

            await dbEntity.UpdateOneAsync(filter, update);
        }

        public async Task<UserMaster> GetUserByUserIdAsync(string userId)
        {
            var filter = Builders<UserMaster>.Filter.Eq(x => x.Id, userId);
            var data = await dbEntity.Find(filter).FirstOrDefaultAsync();
            return data;
        }

        public async Task<bool> IsRoleExistAsync(string roleId)
        {
            var filter = Builders<UserMaster>.Filter.And(
                         Builders<UserMaster>.Filter.AnyEq(x => x.RoleIds, roleId),
                         Builders<UserMaster>.Filter.Or(
                         Builders<UserMaster>.Filter.Eq(x => x.IsDeleted, false),
                         Builders<UserMaster>.Filter.Eq(x => x.IsDeleted, (bool?)null)));

            var data = await dbEntity.Find(filter).FirstOrDefaultAsync();
            return data != null;
        }

        public async Task<int> GetUserCountAsync(string username)
        {
            
            var excludeAdmins = username == "superadmin"
                ? new[] { "sysadmin", "superadmin" }
                : new[] { "sysadmin", "superadmin", "viadmin" };
            var filter = Builders<UserMaster>.Filter.And(
                Builders<UserMaster>.Filter.Eq(x => x.IsDeleted, false),
                Builders<UserMaster>.Filter.Nin(x => x.Username, excludeAdmins)
            );
            var result = await dbEntity.CountDocumentsAsync(filter);
            return (int)result;
        }

        public async Task<IEnumerable<UserMaster>> GetAllUserAsync(string username)
        {
            var excludeAdmins = username == "superadmin"
                ? new[] { "sysadmin", "superadmin" }
                : new[] { "sysadmin", "superadmin", "viadmin" };

            var filter = Builders<UserMaster>.Filter.And(
                Builders<UserMaster>.Filter.Eq(x => x.IsDeleted, false),
                Builders<UserMaster>.Filter.Nin(x => x.Username, excludeAdmins)
            );
            var data = await dbEntity.Find(filter).SortByDescending(x => x.CreatedOn).ToListAsync();
            return data;
        }
    }
}
