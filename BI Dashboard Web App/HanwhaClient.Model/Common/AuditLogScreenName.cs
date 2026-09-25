using HanwhaClient.Model.DbEntities;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Common
{
    public class AuditLogScreenName
    {
        public static List<ScreenMaster> GetAllAuditLogcreen()
        {
            List<ScreenMaster> screens = new List<ScreenMaster>
            {
                     // ANPR Owner Master screens
                      new ScreenMaster { Id = "698db72e764d52753e8e56a2", ScreenName = ScreenNames.AuditLogMaster, IsActive = true, ParentsScreenId = null, SequenceNo = 81 },
                      //new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.ViewAuditLogs, IsActive = true, ParentsScreenId = "698db72e764d52753e8e56a2", SequenceNo = 82 },            
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.ViewAuditLogsUserMaster, IsActive = true, ParentsScreenId = "698db72e764d52753e8e56a2", SequenceNo = 82},   
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.ViewAuditLogsDeviceMaster, IsActive = true, ParentsScreenId = "698db72e764d52753e8e56a2", SequenceNo = 83 },   
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.ViewAuditLogsANPRVehicle, IsActive = true, ParentsScreenId = "698db72e764d52753e8e56a2", SequenceNo = 84 },   
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.ViewAuditLogsvehicleOwner, IsActive = true, ParentsScreenId = "698db72e764d52753e8e56a2", SequenceNo = 85 },   
            };

            return screens;
        }
    }
}
