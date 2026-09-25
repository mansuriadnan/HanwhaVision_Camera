using HanwhaAdminApi.Infrastructure.Interfaces;
using HanwhaAdminApi.Model.Common;
using HanwhaAdminApi.Model.DbEntities;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaAdminApi.Application.Services.Migrations.Migratation
{
    [Migration(2025011603, "Performing Region Screen Permission during the first-time application execution")]
    public class RegionPermissionScript : IMigration
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUsersRepository _usersRepository;

        public RegionPermissionScript(IRoleRepository roleRepository, IUsersRepository usersRepository)
        {
            _roleRepository = roleRepository;
            _usersRepository = usersRepository;
        }

        public async Task RunAsync(IMongoDatabase database)
        {
            var screenCollection = database.GetCollection<ScreenMaster>(AppDBConstants.ScreenMaster);
            var roleScreenMappingCollection = database.GetCollection<RoleScreenMapping>(AppDBConstants.RoleScreenMapping);

            var superAdminId = await _roleRepository.GetRoleIdByRoleName("Super Admin");
            if (superAdminId is null)
            {
                return;
            }

            // Seed region screens for Super Admin
            await SeedScreensAndMapToRoleAsync(screenCollection, roleScreenMappingCollection, superAdminId, RegionScreen.GetAllRegionScreen());

            // Seed additional Customers & Licenses sub-screen
            var customersLicensesParent = await screenCollection.Find(x => x.ScreenName == "Customers & Licenses").FirstOrDefaultAsync();
            if (customersLicensesParent is not null)
            {
                var additionalScreens = new List<ScreenMaster>
                                        {
                                            new ScreenMaster
                                            {
                                                Id = ObjectId.GenerateNewId().ToString(),
                                                ScreenName = ScreenNames.CanDeleteLicense,
                                                IsActive = true,
                                                ParentsScreenId = customersLicensesParent.Id,
                                                SequenceNo = 60
                                            }
                                        };

                await SeedScreensAndMapToRoleAsync(screenCollection, roleScreenMappingCollection, superAdminId, additionalScreens);
            }
        }

        private async Task SeedScreensAndMapToRoleAsync(
            IMongoCollection<ScreenMaster> screenCollection,
            IMongoCollection<RoleScreenMapping> mappingCollection,
            string roleId,
            IEnumerable<ScreenMaster> screensToSeed)
        {
            if (!screensToSeed?.Any() == true)
            {
                return;
            }

            // Insert new screens only (idempotent)
            var existingScreenNames = await screenCollection
                .Distinct(x => x.ScreenName, FilterDefinition<ScreenMaster>.Empty)
                .ToListAsync();

            var newScreens = screensToSeed
                .Where(s => !existingScreenNames.Contains(s.ScreenName))
                .ToList();

            if (newScreens.Any())
            {
                await screenCollection.InsertManyAsync(newScreens);
            }

            // Get or create role mapping and append missing screen mappings
            var filter = Builders<RoleScreenMapping>.Filter.Eq(m => m.RoleId, roleId);
            var existingMapping = await mappingCollection.Find(filter).FirstOrDefaultAsync();

            if (existingMapping is null)
            {
                return;
            }

            var allScreens = await screenCollection.Find(FilterDefinition<ScreenMaster>.Empty).ToListAsync();
            var existingScreenIds = existingMapping.ScreenMappings.Select(m => m.ScreenId).ToHashSet();

            var missingMappings = allScreens
                .Where(s => !existingScreenIds.Contains(s.Id))
                .Select(s => new ScreenMapping { ScreenId = s.Id, AccessAllowed = true })
                .ToList();

            if (missingMappings.Any())
            {
                var update = Builders<RoleScreenMapping>.Update.PushEach(m => m.ScreenMappings, missingMappings);
                await mappingCollection.UpdateOneAsync(filter, update);
            }
        }
    }
}
