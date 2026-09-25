using HanwhaClient.Core.Interfaces;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Infrastructure.Repository;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Services.Migrations.Migratation
{
    [Migration(2025011612, "Add iDRAC and SSM Permission during the first-time application execution")]
    public class IDRACSSMScript2025011612 : IMigration
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUsersRepository _usersRepository;

        public IDRACSSMScript2025011612(IRoleRepository roleRepository, IUsersRepository usersRepository)
        {
            _roleRepository = roleRepository;
            _usersRepository = usersRepository;
        }

        public async Task RunAsync(IMongoDatabase database)
        {
            var screenCollection = database.GetCollection<ScreenMaster>(AppDBConstants.ScreenMaster);
            var roleScreenMappingCollection = database.GetCollection<RoleScreenMapping>(AppDBConstants.RoleScreenMapping);

            var superAdminId = await _roleRepository.GetRoleIdByRoleName("Super Admin");

            #region Insert iDREC and SSM

            var getIDRECSSM = IDRACSSMScreenName.GetAllIDRACSSMScreenName();

            if (getIDRECSSM == null || !getIDRECSSM.Any())
            {
                Console.WriteLine("No screen found in setup. Nothing to process.");
                return;
            }

            if (superAdminId != null)
            {
                // Fetch existing stored screens
                var existingScreens = await screenCollection.Find(FilterDefinition<ScreenMaster>.Empty).ToListAsync();

                // Find screens that don't exist yet
                var newScreens = getIDRECSSM
                    .Where(screen => existingScreens.All(existing => existing.ScreenName != screen.ScreenName))
                    .ToList();

                if (newScreens.Any())
                {
                    // Insert missing screens
                    await screenCollection.InsertManyAsync(newScreens);
                }

                // 🔥 Reload screen list to include newly inserted records
                existingScreens = await screenCollection.Find(FilterDefinition<ScreenMaster>.Empty).ToListAsync();

                var existingMapping = await roleScreenMappingCollection
                    .Find(Builders<RoleScreenMapping>.Filter.Eq(m => m.RoleId, superAdminId))
                    .FirstOrDefaultAsync();

                // Find screens not yet mapped
                var missingScreens = existingScreens
                    .Where(screen => existingMapping.ScreenMappings.All(m => m.ScreenId != screen.Id))
                    .Select(screen => new ScreenMapping
                    {
                        ScreenId = screen.Id,
                        AccessAllowed = true,
                    })
                    .ToList();

                if (missingScreens.Any())
                {
                    // Append missing screens
                    var appaendMissing = Builders<RoleScreenMapping>.Update
                        .PushEach(m => m.ScreenMappings, missingScreens);

                    await roleScreenMappingCollection.UpdateOneAsync(
                        Builders<RoleScreenMapping>.Filter.Eq(m => m.Id, existingMapping.Id),
                        appaendMissing
                    );

                    Console.WriteLine($"Updated existing mapping: added {missingScreens.Count} new screens.");
                }
                else
                {
                    Console.WriteLine("No new screens to add — mapping already up to date.");
                }

            }
            else
            {
                Console.WriteLine("Sample screens or roles are null. Skipping insertion.");
            }

            #endregion

        }
    }
}