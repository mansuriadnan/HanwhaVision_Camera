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
    [Migration(2025011608, "Email template for exception log and Audit log screen permission")]
    public class ExceptionLogEmailAndAuditLog2026020908 : IMigration
    {
        private readonly IRoleRepository _roleRepository;

        public ExceptionLogEmailAndAuditLog2026020908(IRoleRepository roleRepository)
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

            await CreateInitialEmailTemplatesAsync(emailTemplateCollection, superAdminId);

            #region Add and Update General

            var getGeneral = screenCollection.Find(x => x.ScreenName == "General").ToList();
            if (getGeneral == null)
                return;

            var addGeneralPermission = new List<ScreenMaster>()
            {
                 new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.View_and_Configure_ANPR, IsActive = true, ParentsScreenId = getGeneral.FirstOrDefault().Id, SequenceNo = 47 },
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

            #region Update Floor Plan Heatmap Widget Name

            var mapFloorPlanWidgetData = widgetMasterCollection.Find(x => x.CategoryName == "Map Floor Plan").FirstOrDefault();

            if (mapFloorPlanWidgetData != null)
            {

                var floorPlanHeatmapWidget = new List<WidgetItem>
                                  {
                                      new WidgetItem
                                      {
                                          WidgetId = ObjectId.GenerateNewId().ToString(),
                                          WidgetName = ScreenNames.FloorPlanHeatmap
                                      }
                                  };


                var missingWidgets = floorPlanHeatmapWidget
                .Where(required =>
                    !mapFloorPlanWidgetData.Widgets.Any(existing =>
                        existing.WidgetName == required.WidgetName))
                .ToList();

                if (!missingWidgets.Any())
                    return; // Nothing new to add

                // 4. Create the MongoDB update operation (Push multiple items)
                var update = Builders<WidgetMaster>.Update
                    .PushEach(w => w.Widgets, missingWidgets);

                // 5. Apply only the partial update (efficient)
                await widgetMasterCollection.UpdateOneAsync(
                    w => w.Id == mapFloorPlanWidgetData.Id,
                    update);
            }

            #endregion

            #region Insert Audit Logs

            var getAuditLogsScreen = AuditLogScreenName.GetAllAuditLogcreen();

            if (getAuditLogsScreen == null || !getAuditLogsScreen.Any())
            {
                Console.WriteLine("No screen found in setup. Nothing to process.");
                return;
            }

            if (superAdminId != null)
            {
                // Fetch existing stored screens
                var existingScreens = await screenCollection.Find(FilterDefinition<ScreenMaster>.Empty).ToListAsync();

                // Find screens that don't exist yet
                var newScreen = getAuditLogsScreen
                    .Where(screen => existingScreens.All(existing => existing.ScreenName != screen.ScreenName))
                    .ToList();

                if (newScreen.Any())
                {
                    // Insert missing screens
                    await screenCollection.InsertManyAsync(newScreen);
                }

                // 🔥 Reload screen list to include newly inserted records
                existingScreens = await screenCollection.Find(FilterDefinition<ScreenMaster>.Empty).ToListAsync();

                var existingMappings = await roleScreenMappingCollection
                    .Find(Builders<RoleScreenMapping>.Filter.Eq(m => m.RoleId, superAdminId))
                    .FirstOrDefaultAsync();

                // Find screens not yet mapped
                var missingScreen = existingScreens
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

            await InsertRoleScreenMappingFromWidgetsAsync(widgetMasterCollection, roleScreenMappingCollection, superAdminId);

        }

        private async Task CreateInitialEmailTemplatesAsync(
        IMongoCollection<EmailTemplate> emailTemplateCollection,
        string createdById)
        {

            var emailTemplates = new List<EmailTemplate>
            {
                new EmailTemplate
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    EmailTemplateName = "Exception Alert",
                    EmailTemplateTitle = "Exception Alert from Vision Insight Application",
                    EmailTemplateDescription = "Exception Alert from Vision Insight Application",
                    EmailTemplateHtml = "<body style=\"width: 100%; font-family: 'Calibri', sans-serif; font-size: 11pt; line-height: 1.6; color: #333;\">\r\n    <p><strong>Hello Team,</strong></p>\r\n\r\n    <p>An exception has occurred in the <strong>Vision Insight Application</strong>. Please find the details below:</p>\r\n\r\n    <p><strong>Exception Details:</strong></p>\r\n\r\n    <ul>\r\n        <li><strong>Time:</strong> [[Time]]</li>\r\n        <li><strong>Exception Type:</strong> [[ExceptionType]]</li>\r\n        <li><strong>Request Path:</strong> [[RequestPath]]</li>\r\n        <li><strong>Exception Message:</strong> [[ExceptionMessage]]</li>\r\n        <li><strong>Stack Trace:</strong> [[StackTrace]]</li>\r\n    </ul>\r\n\r\n    <p><strong>Regards,<br>\r\n    Hanwha Vision</strong></p>\r\n</body>",
                    CreatedBy = createdById,
                    CreatedOn = DateTime.UtcNow,
                    UpdatedBy = createdById,
                    UpdatedOn = DateTime.UtcNow,
                    IsDeleted = false
                }
            };

            // Prepare bulk write operations
            var bulkOps = new List<WriteModel<EmailTemplate>>();

            foreach (var template in emailTemplates)
            {
                // Check if a template with the same name already exists
                var filter = Builders<EmailTemplate>.Filter.Eq(t => t.EmailTemplateName, template.EmailTemplateName);

                // Upsert operation: insert if not exists, update if exists
                var upsertModel = new ReplaceOneModel<EmailTemplate>(filter, template)
                {
                    IsUpsert = true
                };

                bulkOps.Add(upsertModel);
            }

            // Execute bulk write
            if (bulkOps.Any())
            {
                await emailTemplateCollection.BulkWriteAsync(bulkOps);
            }
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
