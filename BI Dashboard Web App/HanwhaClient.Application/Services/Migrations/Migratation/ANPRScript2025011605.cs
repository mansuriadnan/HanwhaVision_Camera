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
    [Migration(2025011607, "Performing ANPR Screen Permission during the first-time application execution")]
    public class ANPRScript2025011605 : IMigration
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUsersRepository _usersRepository;

        public ANPRScript2025011605(IRoleRepository roleRepository, IUsersRepository usersRepository)
        {
            _roleRepository = roleRepository;
            _usersRepository = usersRepository;
        }

        public async Task RunAsync(IMongoDatabase database)
        {
            var screenCollection = database.GetCollection<ScreenMaster>(AppDBConstants.ScreenMaster);
            var roleScreenMappingCollection = database.GetCollection<RoleScreenMapping>(AppDBConstants.RoleScreenMapping);
            var widgetMasterCollection = database.GetCollection<WidgetMaster>(AppDBConstants.WidgetMaster);

            var superAdminId = await _roleRepository.GetRoleIdByRoleName("Super Admin");

            #region Insert ANPR

            var getANPRScreen = ANPRScreenName.GetAllANPRScreen();

            if (getANPRScreen == null || !getANPRScreen.Any())
            {
                Console.WriteLine("No widgets found in setup. Nothing to process.");
                return;
            }

            if (superAdminId != null)
            {
                // Fetch existing stored screens
                var existingScreens = await screenCollection.Find(FilterDefinition<ScreenMaster>.Empty).ToListAsync();

                // Find screens that don't exist yet
                var newScreens = getANPRScreen
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

            // 1. Fetch the widget master for the category "People"
            var existingWidgetMaster = await widgetMasterCollection
                .Find(w => w.CategoryName == "People")
                .FirstOrDefaultAsync();

            if (existingWidgetMaster == null)
                return; // or handle as needed

            // 2. Widgets that *should* exist for this category
            var requiredWidgets = new List<WidgetItem>
                                  {
                                      new WidgetItem
                                      {
                                          WidgetId = ObjectId.GenerateNewId().ToString(),
                                          WidgetName = ScreenNames.PeopleCountByAge
                                      }
                                  };

            // 3. Identify widgets that do NOT already exist
            var missingWidgets = requiredWidgets
                .Where(required =>
                    !existingWidgetMaster.Widgets.Any(existing =>
                        existing.WidgetName == required.WidgetName))
                .ToList();

            if (!missingWidgets.Any())
                return; // Nothing new to add

            // 4. Create the MongoDB update operation (Push multiple items)
            var update = Builders<WidgetMaster>.Update
                .PushEach(w => w.Widgets, missingWidgets);

            // 5. Apply only the partial update (efficient)
            await widgetMasterCollection.UpdateOneAsync(
                w => w.Id == existingWidgetMaster.Id,
                update);


            // Step 7: Create Widget Permission 
            await InsertRoleScreenMappingFromWidgetsAsync(widgetMasterCollection, roleScreenMappingCollection, superAdminId);
        }


        private async Task InsertRoleScreenMappingFromWidgetsAsync(
      IMongoCollection<WidgetMaster> widgetMasterCollection,
      IMongoCollection<RoleScreenMapping> roleScreenMappingCollection,
      string roleId)
        {
            // Step 1: Get all WidgetMaster records
            var widgetMasters = await widgetMasterCollection.Find(_ => true).ToListAsync();

            // Step 2: Transform WidgetMaster -> WidgetAccessPermission
            var widgetAccessPermissions = widgetMasters
                .Where(w => w.Widgets != null && w.Widgets.Any())
                .Select(w => new WidgetAccessPermission
                {
                    WidgetCategoryId = w.Id,
                    WidgetIds = w.Widgets.Select(widget => widget.WidgetId)
                })
                .ToList();

            // Step 3: Update the existing RoleScreenMapping for the given roleId
            var update = Builders<RoleScreenMapping>.Update
                .Set(x => x.WidgetAccessPermissions, widgetAccessPermissions);

            var filter = Builders<RoleScreenMapping>.Filter
                .Eq(x => x.RoleId, roleId);

            var result = await roleScreenMappingCollection.UpdateOneAsync(filter, update);

            // Optional: handle case where document doesn't exist
            if (result.MatchedCount == 0)
            {
                // Optionally create a new one (if you want upsert)
                var roleScreenMapping = new RoleScreenMapping
                {
                    RoleId = roleId,
                    WidgetAccessPermissions = widgetAccessPermissions
                };
                await roleScreenMappingCollection.InsertOneAsync(roleScreenMapping);
            }
        }
    }
}