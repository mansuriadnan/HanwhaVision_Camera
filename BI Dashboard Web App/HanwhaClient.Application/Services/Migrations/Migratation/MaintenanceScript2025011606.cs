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
    [Migration(2025011605, "Performing Maintenance Screen and Widget Permission during the first-time application execution")]
    public class MaintenanceScript2025011606 : IMigration
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUsersRepository _usersRepository;

        public MaintenanceScript2025011606(IRoleRepository roleRepository, IUsersRepository usersRepository)
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

            #region Insert Maintenance

            var getMaintenanceScreen = MaintenanceScreenName.GetAllMaintenanceScreen();

            if (getMaintenanceScreen == null || !getMaintenanceScreen.Any())
            {
                Console.WriteLine("No widgets found in setup. Nothing to process.");
                return;
            }

            if (superAdminId != null)
            {
                // Fetch existing stored screens
                var existingScreens = await screenCollection.Find(FilterDefinition<ScreenMaster>.Empty).ToListAsync();

                // Find screens that don't exist yet
                var newScreens = getMaintenanceScreen
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


            #region Add Maintenance Widget

            // Get all widget masters (categories with their widgets) from the setup
            var allWidgetMastersFromSetup = MaintenanceWidgetMasterList.GetAllMaintenanceWidgets();

            if (allWidgetMastersFromSetup == null || !allWidgetMastersFromSetup.Any())
            {
                // Optional: Log that no widgets were found in setup
                System.Console.WriteLine("No widgets found in setup. Nothing to process.");
                return;
            }

            foreach (var currentWidgetMasterFromSetup in allWidgetMastersFromSetup)
            {
                if (currentWidgetMasterFromSetup == null || string.IsNullOrEmpty(currentWidgetMasterFromSetup.CategoryName))
                {
                    // Optional: Log or handle invalid WidgetMaster from setup
                    System.Console.WriteLine("Skipping an invalid WidgetMaster (null or no CategoryName) from setup.");
                    continue;
                }

                // Try to find an existing WidgetMaster in the database by its CategoryName
                var existingWidgetMaster = await widgetMasterCollection
                                                .Find(wm => wm.CategoryName == currentWidgetMasterFromSetup.CategoryName)
                                                .FirstOrDefaultAsync();

                if (existingWidgetMaster != null)
                {
                    // Category exists, check for new widgets to add
                    if (currentWidgetMasterFromSetup.Widgets != null && currentWidgetMasterFromSetup.Widgets.Any())
                    {
                        // Ensure existingWidgetMaster.Widgets is not null
                        if (existingWidgetMaster.Widgets == null)
                        {
                            existingWidgetMaster.Widgets = new List<WidgetItem>();
                        }

                        // Filter out widgets from the setup that already exist in the database for this category (by WidgetName)
                        var newWidgetsToAdd = currentWidgetMasterFromSetup.Widgets.Where(newWidget =>
                                            !existingWidgetMaster.Widgets
                                                .Any(existingWidget => existingWidget.WidgetName == newWidget.WidgetName))
                                            .ToList();

                        if (newWidgetsToAdd.Any())
                        {
                            // Add the new widgets to the existing category's widget list
                            existingWidgetMaster.Widgets = existingWidgetMaster.Widgets.Concat(newWidgetsToAdd).ToList();

                            // Replace the existing document in the database with the updated one
                            // This requires WidgetMaster to have an Id property mapped to MongoDB's _id
                            await widgetMasterCollection.ReplaceOneAsync(
                                wm => wm.Id == existingWidgetMaster.Id, // Match by the document's unique Id
                                existingWidgetMaster);
                        }
                    }
                }
                else
                {
                    // Category does not exist, insert the new WidgetMaster from setup
                    // Ensure the Id is not set or is null if MongoDB is to generate it,
                    // or ensure it's a new valid ObjectId string if you generate it client-side.
                    // For this example, we assume MongoDB will generate the _id if currentWidgetMasterFromSetup.Id is null.
                    await widgetMasterCollection.InsertOneAsync(currentWidgetMasterFromSetup);
                }
            }


            // Step 7: Create Widget Permission 
            await InsertRoleScreenMappingFromWidgetsAsync(widgetMasterCollection, roleScreenMappingCollection, superAdminId);

            #endregion
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