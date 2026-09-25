using HanwhaClient.Core.Interfaces;
using HanwhaClient.Infrastructure.Interfaces;
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
    [Migration(2025011609, "Add and update remining permission for ANPR,LPR and General setting page.")]
    public class ANPRGenLPRPermission2026020909 : IMigration
    {
        private readonly IRoleRepository _roleRepository;

        public ANPRGenLPRPermission2026020909(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task RunAsync(IMongoDatabase database)
        {
            var screenCollection = database.GetCollection<ScreenMaster>(AppDBConstants.ScreenMaster);
            var roleScreenMappingCollection = database.GetCollection<RoleScreenMapping>(AppDBConstants.RoleScreenMapping);
            var roleCollection = database.GetCollection<RoleMaster>(AppDBConstants.RoleMaster);
            var emailTemplateCollection = database.GetCollection<EmailTemplate>(AppDBConstants.EmailTemplates);
            var widgetMasterCollection = database.GetCollection<WidgetMaster>(AppDBConstants.WidgetMaster);
            var superAdminId = await _roleRepository.GetRoleIdByRoleName("Super Admin");


            #region Add and Update General

            var getGeneral = screenCollection.Find(x => x.ScreenName == "General").ToList();
            if (getGeneral == null)
                return;

            var addGeneralPermission = new List<ScreenMaster>()
            {
                 new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.View_and_Configure_Reset_Vehicle_Parking_Count, IsActive = true, ParentsScreenId = getGeneral.FirstOrDefault().Id, SequenceNo = 47 },
            };

            var newScreens = addGeneralPermission
                   .Where(screen => getGeneral.All(existing => existing.ScreenName != screen.ScreenName))
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

            #region Add and Update Manage Devices

            var getManageDevices = screenCollection.Find(x => x.ScreenName == "Manage Devices").ToList();
            if (getManageDevices == null)
                return;

            var addManageDevice = new List<ScreenMaster>()
            {
                 new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.UpdateMaintenanceDevice, IsActive = true, ParentsScreenId = getManageDevices.FirstOrDefault().Id, SequenceNo = 49 },
                 new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.ManuallyOpenGate, IsActive = true, ParentsScreenId = getManageDevices.FirstOrDefault().Id, SequenceNo = 49 },
            };

            var newManageDevicess = addManageDevice
                   .Where(screen => getManageDevices.All(existing => existing.ScreenName != screen.ScreenName))
                   .ToList();

            if (newManageDevicess.Any())
            {
                // Insert missing screens
                await screenCollection.InsertManyAsync(newManageDevicess);
            }

            // 🔥 Reload screen list to include newly inserted records
            getManageDevices = await screenCollection.Find(FilterDefinition<ScreenMaster>.Empty).ToListAsync();

            var existingManageDeviceMapping = await roleScreenMappingCollection
                .Find(Builders<RoleScreenMapping>.Filter.Eq(m => m.RoleId, superAdminId))
                .FirstOrDefaultAsync();

            // Find screens not yet mapped
            var missingManageDeviceScreens = getManageDevices
                .Where(screen => existingManageDeviceMapping.ScreenMappings.All(m => m.ScreenId != screen.Id))
                .Select(screen => new ScreenMapping
                {
                    ScreenId = screen.Id,
                    AccessAllowed = true,
                })
                .ToList();

            if (missingManageDeviceScreens.Any())
            {
                // Append missing screens
                var appaendMissing = Builders<RoleScreenMapping>.Update
                    .PushEach(m => m.ScreenMappings, missingManageDeviceScreens);

                await roleScreenMappingCollection.UpdateOneAsync(
                    Builders<RoleScreenMapping>.Filter.Eq(m => m.Id, existingManageDeviceMapping.Id),
                    appaendMissing
                );

                Console.WriteLine($"Updated existing mapping: added {missingManageDeviceScreens.Count} new screens.");
            }
            else
            {
                Console.WriteLine("No new screens to add — mapping already up to date.");
            }

            #endregion

            #region Update Vehicle Widget Name

            var vehicleparkingWidgetData = widgetMasterCollection.Find(x => x.CategoryName == "Vehicle").FirstOrDefault();

            if (vehicleparkingWidgetData != null)
            {

                var floorPlanHeatmapWidget = new List<WidgetItem>
                                  {
                                      new WidgetItem
                                      {
                                          WidgetId = ObjectId.GenerateNewId().ToString(),
                                          WidgetName = ScreenNames.Parking
                                      }
                                  };


                var missingWidgets = floorPlanHeatmapWidget
                .Where(required =>
                    !vehicleparkingWidgetData.Widgets.Any(existing =>
                        existing.WidgetName == required.WidgetName))
                .ToList();

                if (!missingWidgets.Any())
                    return; // Nothing new to add

                // 4. Create the MongoDB update operation (Push multiple items)
                var update = Builders<WidgetMaster>.Update
                    .PushEach(w => w.Widgets, missingWidgets);

                // 5. Apply only the partial update (efficient)
                await widgetMasterCollection.UpdateOneAsync(
                    w => w.Id == vehicleparkingWidgetData.Id,
                    update);

                await InsertRoleScreenMappingFromWidgetsAsync(widgetMasterCollection, roleScreenMappingCollection, superAdminId);
            }

            #endregion

            #region LPR

            List<ScreenMaster> getLPRScreen = new List<ScreenMaster>
            {
                      new ScreenMaster { Id = "69aac3575a440754181d2306", ScreenName = ScreenNames.LPRMaster, IsActive = true, ParentsScreenId = null, SequenceNo = 87 },
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.ViewLPR, IsActive = true, ParentsScreenId = "69aac3575a440754181d2306", SequenceNo = 88},
            };

            if (getLPRScreen == null || !getLPRScreen.Any())
            {
                Console.WriteLine("No screen found in setup. Nothing to process.");
                return;
            }

            if (superAdminId != null)
            {
                // Fetch existing stored screens
                var existingLPRScreens = await screenCollection.Find(FilterDefinition<ScreenMaster>.Empty).ToListAsync();

                // Find screens that don't exist yet
                var newScreen = getLPRScreen
                    .Where(screen => existingLPRScreens.All(existing => existing.ScreenName != screen.ScreenName))
                    .ToList();

                if (newScreen.Any())
                {
                    // Insert missing screens
                    await screenCollection.InsertManyAsync(newScreen);
                }

                // 🔥 Reload screen list to include newly inserted records
                existingLPRScreens = await screenCollection.Find(FilterDefinition<ScreenMaster>.Empty).ToListAsync();

                var existingMappings = await roleScreenMappingCollection
                    .Find(Builders<RoleScreenMapping>.Filter.Eq(m => m.RoleId, superAdminId))
                    .FirstOrDefaultAsync();

                // Find screens not yet mapped
                var missingScreen = existingLPRScreens
                    .Where(screen => existingMappings.ScreenMappings.All(m => m.ScreenId != screen.Id))
                    .Select(screen => new ScreenMapping
                    {
                        ScreenId = screen.Id,
                        AccessAllowed = true,
                    })
                    .ToList();

                if (missingScreen.Any())
                {
                    // Append missing screens
                    var appaendMissing = Builders<RoleScreenMapping>.Update
                        .PushEach(m => m.ScreenMappings, missingScreen);

                    await roleScreenMappingCollection.UpdateOneAsync(
                        Builders<RoleScreenMapping>.Filter.Eq(m => m.Id, existingMapping.Id),
                        appaendMissing
                    );

                    Console.WriteLine($"Updated existing mapping: added {missingScreen.Count} new screens.");
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

            #region Add ANPR Widget


            var categories = new List<string>
                            {
                                ScreenNames.ANPR
                            };

            var allWidgetMasters = new List<WidgetMaster>();

            foreach (var categoryName in categories)
            {
                var widgetsForThisCategory = new List<WidgetItem>();

                // --- Populate widgets based on the category ---

                switch (categoryName)
                {
                    case "ANPR":
                        widgetsForThisCategory.Add(new WidgetItem { WidgetId = ObjectId.GenerateNewId().ToString(), WidgetName = ScreenNames.ANPRParking });
                        break;

                    default:
                        // Fallback for any other unhandled category, though all from your list are covered
                        widgetsForThisCategory.Add(new WidgetItem
                        {
                            WidgetId = ObjectId.GenerateNewId().ToString(),
                            WidgetName = $"Generic {categoryName} Widget"
                        });
                        break;
                }

                // If a category must have at least one widget, and the switch doesn't add one,
                if (widgetsForThisCategory.Count == 0)
                {
                    widgetsForThisCategory.Add(new WidgetItem
                    {
                        WidgetId = ObjectId.GenerateNewId().ToString(),
                        WidgetName = $"Placeholder for {categoryName}"
                    });
                }


                var widgetMaster = new WidgetMaster
                {
                    CategoryName = categoryName,
                    Widgets = widgetsForThisCategory
                };
                allWidgetMasters.Add(widgetMaster);
            }


            // Get all widget masters (categories with their widgets) from the setup
            var allWidgetMastersFromSetup = allWidgetMasters;

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


            #endregion

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
