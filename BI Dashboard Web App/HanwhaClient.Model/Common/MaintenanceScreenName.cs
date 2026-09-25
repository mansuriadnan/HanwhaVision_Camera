using HanwhaClient.Model.DbEntities;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Common
{
    public class MaintenanceScreenName
    {
        public static List<ScreenMaster> GetAllMaintenanceScreen()
        {
            List<ScreenMaster> screens = new List<ScreenMaster>
            {
                     // ANPR Owner Master screens
                      new ScreenMaster { Id = "694bdc3218409cb1763833c1", ScreenName = ScreenNames.MaintenanceMaster, IsActive = true, ParentsScreenId = null, SequenceNo = 71 },
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.ViewMaintenancePlan, IsActive = true, ParentsScreenId = "694bdc3218409cb1763833c1", SequenceNo = 72 },  
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.AddOrUpdateMaintenancePlan, IsActive = true, ParentsScreenId = "694bdc3218409cb1763833c1", SequenceNo = 73 },  
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.DeleteMaintenancePlan, IsActive = true, ParentsScreenId = "694bdc3218409cb1763833c1", SequenceNo = 74 },     

                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.ViewMaintenanceSchedule, IsActive = true, ParentsScreenId = "694bdc3218409cb1763833c1", SequenceNo = 75 },     
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.AddOrUpdateMaintenanceSchedule, IsActive = true, ParentsScreenId = "694bdc3218409cb1763833c1", SequenceNo = 76 },     
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.DeleteMaintenanceSchedule, IsActive = true, ParentsScreenId = "694bdc3218409cb1763833c1", SequenceNo = 77 },  

                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.ViewRMA, IsActive = true, ParentsScreenId = "694bdc3218409cb1763833c1", SequenceNo = 78 },     
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.AddOrUpdateRMA, IsActive = true, ParentsScreenId = "694bdc3218409cb1763833c1", SequenceNo = 79 },     
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.DeleteRMA, IsActive = true, ParentsScreenId = "694bdc3218409cb1763833c1", SequenceNo = 80 },     
            };

            return screens;
        }
    }
}
