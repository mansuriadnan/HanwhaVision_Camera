using HanwhaClient.Core.Interfaces;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HanwhaClient.Application.Services.Migrations.Migratation
{
    [Migration(2025011604, "Create ViAdmin as a default user during the first-time application execution")]
    public class CreateViAdminUserDefault2025011604 : IMigration
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IPasswordHasher _passwordHasher;

        public CreateViAdminUserDefault2025011604(IRoleRepository roleRepository, IPasswordHasher passwordHasher)
        {
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task RunAsync(IMongoDatabase database)
        {
            var userCollection = database.GetCollection<UserMaster>(AppDBConstants.UserMaster);
                      
            var viAdminUserId = await CreateViAdminUserAsync(userCollection);
        }
        private async Task<string> CreateViAdminUserAsync(
        IMongoCollection<UserMaster> userCollection
        )
        {
            var systemAdminFilter = Builders<UserMaster>.Filter.Eq(u => u.Username, "sysadmin");
            var sysAdminDetails = await userCollection.Find(systemAdminFilter).FirstOrDefaultAsync();
            var superAdminRoleId = await _roleRepository.GetRoleIdByRoleName("Super Admin");

            var viAdminFilter = Builders<UserMaster>.Filter.Eq(u => u.Username, "viadmin");
            var existingViAdmin = await userCollection.Find(viAdminFilter).FirstOrDefaultAsync();

            if (existingViAdmin != null)
            {
                return existingViAdmin.Id;
            }
            var systemId = ObjectId.GenerateNewId().ToString();

            var viAdminUser = new UserMaster
            {
                Id = systemId, // Explicitly generate ID
                Firstname = "VI",
                Lastname = "Admin",
                Username = "viadmin",
                Email = "viadmin@example.com",
                RoleIds = new List<string> { superAdminRoleId },
                Password = _passwordHasher.HashPassword("viadmin"),
                ProfileImage = null,
                CreatedBy = sysAdminDetails.Id, // systemadmin id
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                UpdatedBy = sysAdminDetails.Id, // systemadmin id
                IsPasswordReset = false
            };

            await userCollection.InsertOneAsync(viAdminUser);
            return viAdminUser.Id;
        }
    }
}
