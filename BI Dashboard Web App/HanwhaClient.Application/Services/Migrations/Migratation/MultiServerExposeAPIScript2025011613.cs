using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HanwhaClient.Application.Services.Migrations.Migratation
{
    [Migration(2025011613, "Add Multiserver and ExposeAPI Permission during the first-time application execution")]
    public class MultiServerExposeAPIScript2025011613 : IMigration
    {
        private readonly IRoleRepository _roleRepository;

        public MultiServerExposeAPIScript2025011613(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;            
        }

        public async Task RunAsync(IMongoDatabase database)
        {
            var screenCollection = database.GetCollection<ScreenMaster>(AppDBConstants.ScreenMaster);
            var roleScreenMappingCollection = database.GetCollection<RoleScreenMapping>(AppDBConstants.RoleScreenMapping);

            var superAdminId = await _roleRepository.GetRoleIdByRoleName("Super Admin");

            #region Insert Multiserevr and expose API

            var getScreens = MultiserverScreenName.GetAllMultiServerScreenName();

            if (getScreens == null || !getScreens.Any())
            {
                Console.WriteLine("No screen found in setup. Nothing to process.");
                return;
            }

            if (superAdminId != null)
            {
                // Fetch existing stored screens
                var existingScreens = await screenCollection.Find(FilterDefinition<ScreenMaster>.Empty).ToListAsync();

                // Find screens that don't exist yet
                var newScreens1 = getScreens
                    .Where(screen => existingScreens.All(existing => existing.ScreenName != screen.ScreenName))
                    .ToList();

                if (newScreens1.Any())
                {
                    // Insert missing screens
                    await screenCollection.InsertManyAsync(newScreens1);
                }

                // 🔥 Reload screen list to include newly inserted records
                existingScreens = await screenCollection.Find(FilterDefinition<ScreenMaster>.Empty).ToListAsync();

                var existingMapping1 = await roleScreenMappingCollection
                    .Find(Builders<RoleScreenMapping>.Filter.Eq(m => m.RoleId, superAdminId))
                    .FirstOrDefaultAsync();

                // Find screens not yet mapped
                var missingScreens1 = existingScreens
                    .Where(screen => existingMapping1.ScreenMappings.All(m => m.ScreenId != screen.Id))
                    .Select(screen => new ScreenMapping
                    {
                        ScreenId = screen.Id,
                        AccessAllowed = true,
                    })
                    .ToList();

                if (missingScreens1.Any())
                {
                    // Append missing screens
                    var appaendMissing = Builders<RoleScreenMapping>.Update
                        .PushEach(m => m.ScreenMappings, missingScreens1);

                    await roleScreenMappingCollection.UpdateOneAsync(
                        Builders<RoleScreenMapping>.Filter.Eq(m => m.Id, existingMapping1.Id),
                        appaendMissing
                    );

                    Console.WriteLine($"Updated existing mapping: added {missingScreens1.Count} new screens.");
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

            #region Add and Update General and audit log

            var getGeneral = screenCollection.Find(x => x.ScreenName == "General").ToList();
            var getAuditLogs = await screenCollection.Find(x => x.ScreenName == "Audit Logs").FirstOrDefaultAsync();

            if (getGeneral == null)
                return;
            if (getAuditLogs == null)
                return;

            var addPermission = new List<ScreenMaster>()
            {
                 new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.View_and_Config_Expose_API, IsActive = true, ParentsScreenId = getGeneral.FirstOrDefault().Id, SequenceNo = 114 },

                  // Audit Logs
                 new ScreenMaster
                 {
                     Id = ObjectId.GenerateNewId().ToString(),
                     ScreenName = ScreenNames.ViewAuditLogMultiServers,
                     IsActive = true,
                     ParentsScreenId = getAuditLogs.Id,
                     SequenceNo = 115
                 },
                 new ScreenMaster
                 {
                     Id = ObjectId.GenerateNewId().ToString(),
                     ScreenName = ScreenNames.ViewAuditLogSSMServer,
                     IsActive = true,
                     ParentsScreenId = getAuditLogs.Id,
                     SequenceNo = 116
                 },
                 new ScreenMaster
                 {
                     Id = ObjectId.GenerateNewId().ToString(),
                     ScreenName = ScreenNames.ViewAuditLogIdracServer,
                     IsActive = true,
                     ParentsScreenId = getAuditLogs.Id,
                     SequenceNo = 117
                 }
            };
            // Get all existing screen names once
            var existingScreenNames = await screenCollection
                .Find(_ => true)
                .Project(x => x.ScreenName)
                .ToListAsync();

            var newScreens = addPermission
                .Where(screen => !existingScreenNames.Contains(screen.ScreenName))
                .ToList();

            if (newScreens.Any())
            {
                // Insert missing screens
                await screenCollection.InsertManyAsync(newScreens);
            }

            // 🔥 Reload screen list to include newly inserted records
            getGeneral = await screenCollection.Find(FilterDefinition<ScreenMaster>.Empty).ToListAsync();

            var existingMapping = await roleScreenMappingCollection
                .Find(Builders<RoleScreenMapping>.Filter.Eq(m => m.RoleId, superAdminId))
                .FirstOrDefaultAsync();

            // Find screens not yet mapped
            var missingScreens = getGeneral
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

            #endregion
        }
    }
}
