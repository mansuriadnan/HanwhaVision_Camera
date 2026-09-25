using DocumentFormat.OpenXml.Office2010.Excel;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Infrastructure.Repository;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Services.Migrations.Migratation
{

    [Migration(2025011606, "Add and Update Blocked Exit Detection and Api Error")]
    public class AddUpdateWidgetPermission : IMigration
    {
        private readonly IRoleRepository _roleRepository;

        public AddUpdateWidgetPermission(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }
        public async Task RunAsync(IMongoDatabase database)
        {
            var screenCollection = database.GetCollection<ScreenMaster>(AppDBConstants.ScreenMaster);
            var roleScreenMappingCollection = database.GetCollection<RoleScreenMapping>(AppDBConstants.RoleScreenMapping);
            var superAdminId = await _roleRepository.GetRoleIdByRoleName("Super Admin");

            #region Update Factory Widget Name

            var widgetMasterCollection = database.GetCollection<WidgetMaster>(AppDBConstants.WidgetMaster);
            var factoryWidgetData = widgetMasterCollection.Find(x => x.CategoryName == "Factory").FirstOrDefault();

            if (factoryWidgetData != null)
            {

                var widgetToUpdate = factoryWidgetData.Widgets.FirstOrDefault(x => x.WidgetName == "Blocked Exit Detection");
                if (widgetToUpdate != null)
                {
                    widgetToUpdate.WidgetName = "Factory Blocked Exit Detection";

                    var filter = Builders<WidgetMaster>.Filter.Eq(x => x.Id, factoryWidgetData.Id);
                    var update = Builders<WidgetMaster>.Update.Set(x => x.Widgets, factoryWidgetData.Widgets);

                    widgetMasterCollection.UpdateOne(filter, update);
                }
            }

            #endregion

            #region Site

            var siteWidgetData = widgetMasterCollection.Find(x => x.CategoryName == "Site").FirstOrDefault();

            if (siteWidgetData != null)
            {

                var siteWidget = new List<WidgetItem>
                                  {
                                      new WidgetItem
                                      {
                                          WidgetId = ObjectId.GenerateNewId().ToString(),
                                          WidgetName = ScreenNames.SiteApiError
                                      }
                                  };


                var missingWidgets = siteWidget
                .Where(required =>
                    !siteWidgetData.Widgets.Any(existing =>
                        existing.WidgetName == required.WidgetName))
                .ToList();

                if (!missingWidgets.Any())
                    return; // Nothing new to add

                // 4. Create the MongoDB update operation (Push multiple items)
                var update = Builders<WidgetMaster>.Update
                    .PushEach(w => w.Widgets, missingWidgets);

                // 5. Apply only the partial update (efficient)
                await widgetMasterCollection.UpdateOneAsync(
                    w => w.Id == siteWidgetData.Id,
                    update);
            }

            #endregion

            #region Maintenance

            var maintenanceWidgetData = widgetMasterCollection.Find(x => x.CategoryName == "Maintenance").FirstOrDefault();

            if (maintenanceWidgetData != null)
            {

                var maintenanceWidget = new List<WidgetItem>
                                  {
                                      new WidgetItem
                                      {
                                          WidgetId = ObjectId.GenerateNewId().ToString(),
                                          WidgetName = ScreenNames.MaintenanceApiError
                                      }
                                  };


                var missingWidgets = maintenanceWidget
                .Where(required =>
                    !maintenanceWidgetData.Widgets.Any(existing =>
                        existing.WidgetName == required.WidgetName))
                .ToList();

                if (!missingWidgets.Any())
                    return; // Nothing new to add

                // 4. Create the MongoDB update operation (Push multiple items)
                var update = Builders<WidgetMaster>.Update
                    .PushEach(w => w.Widgets, missingWidgets);

                // 5. Apply only the partial update (efficient)
                await widgetMasterCollection.UpdateOneAsync(
                    w => w.Id == maintenanceWidgetData.Id,
                    update);
            }

            #endregion

            await InsertRoleScreenMappingFromWidgetsAsync(widgetMasterCollection, roleScreenMappingCollection, superAdminId);

            #region Add and Update General

            var getGeneral  =  screenCollection.Find(x => x.ScreenName == "General").ToList();
            if (getGeneral == null)
                return;

            var addGeneralPermission = new List<ScreenMaster>()
            {
                 new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.View_and_Configure_Retention_Setup_Details, IsActive = true, ParentsScreenId = getGeneral.FirstOrDefault().Id, SequenceNo = 45 },
                 new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.View_and_Configure_Scheduled_Database_Backup, IsActive = true, ParentsScreenId = getGeneral.FirstOrDefault().Id, SequenceNo = 46 },
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
