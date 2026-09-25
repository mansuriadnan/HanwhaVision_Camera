using HanwhaClient.Model.DbEntities;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Common
{
    public static class MaintenanceWidgetMasterList
    {

        // The method now returns a list of WidgetMaster objects
        public static List<WidgetMaster> GetAllMaintenanceWidgets()
        {
            // The list of categories from your image
            var categories = new List<string>
                            {
                                ScreenNames.Maintenance
                            };

            var allWidgetMasters = new List<WidgetMaster>();

            foreach (var categoryName in categories)
            {
                var widgetsForThisCategory = new List<WidgetItem>();

                // --- Populate widgets based on the category ---

                switch (categoryName)
                {
                    case "Maintenance":
                        widgetsForThisCategory.Add(new WidgetItem { WidgetId = ObjectId.GenerateNewId().ToString(), WidgetName = ScreenNames.MaintenanceMode });
                        widgetsForThisCategory.Add(new WidgetItem { WidgetId = ObjectId.GenerateNewId().ToString(), WidgetName = ScreenNames.CamerasInMaintenance });
                        widgetsForThisCategory.Add(new WidgetItem { WidgetId = ObjectId.GenerateNewId().ToString(), WidgetName = ScreenNames.CameraMaintenanceStatus });
                        widgetsForThisCategory.Add(new WidgetItem { WidgetId = ObjectId.GenerateNewId().ToString(), WidgetName = ScreenNames.CameraDisconnectionTracker });
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

            return allWidgetMasters;
        }
    }
}
