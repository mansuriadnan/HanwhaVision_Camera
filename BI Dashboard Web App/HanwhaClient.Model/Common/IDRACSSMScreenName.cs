using HanwhaClient.Model.DbEntities;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Common
{
    public class IDRACSSMScreenName
    {
        public static List<ScreenMaster> GetAllIDRACSSMScreenName()
        {
            List<ScreenMaster> screens = new List<ScreenMaster>
            {
                     // SSM Master screens
                      new ScreenMaster { Id = "6a0af46fbe617abd9ae3f565", ScreenName = ScreenNames.SsmMaster, IsActive = true, ParentsScreenId = null, SequenceNo = 91 },
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.ViewListofSsmServers, IsActive = true, ParentsScreenId = "6a0af46fbe617abd9ae3f565", SequenceNo = 92 },
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.AddOrUpdateSsmServers, IsActive = true, ParentsScreenId = "6a0af46fbe617abd9ae3f565", SequenceNo = 93 },
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.DeleteSsmServers, IsActive = true, ParentsScreenId = "6a0af46fbe617abd9ae3f565", SequenceNo = 94 },
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.ViewSsmDashboard, IsActive = true, ParentsScreenId = "6a0af46fbe617abd9ae3f565", SequenceNo = 95 },
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.ViewSsmReport, IsActive = true, ParentsScreenId = "6a0af46fbe617abd9ae3f565", SequenceNo = 96 },
                      
                      // iDRAC Master screens
                      new ScreenMaster { Id = "6a0af500be617abd9ae3f566", ScreenName = ScreenNames.IdracMaster, IsActive = true, ParentsScreenId = null, SequenceNo = 97 },
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.ViewListofIdracServers, IsActive = true, ParentsScreenId = "6a0af500be617abd9ae3f566", SequenceNo = 98 },
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.AddOrUpdateIdracServers, IsActive = true, ParentsScreenId = "6a0af500be617abd9ae3f566", SequenceNo = 99 },
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.DeleteIdracServers, IsActive = true, ParentsScreenId = "6a0af500be617abd9ae3f566", SequenceNo = 100 },
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.ViewIdracDashboard, IsActive = true, ParentsScreenId = "6a0af500be617abd9ae3f566", SequenceNo = 101 },
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.IdracServerLedIndicator, IsActive = true, ParentsScreenId = "6a0af500be617abd9ae3f566", SequenceNo = 102 },
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.IdracServerPowerAction, IsActive = true, ParentsScreenId = "6a0af500be617abd9ae3f566", SequenceNo = 103 },
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.IdracServerLaunchRemoteConsole, IsActive = true, ParentsScreenId = "6a0af500be617abd9ae3f566", SequenceNo = 104 },
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.ViewEventLogsIdracServers, IsActive = true, ParentsScreenId = "6a0af500be617abd9ae3f566", SequenceNo = 105 },
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.ViewSystemLogsIdracServers, IsActive = true, ParentsScreenId = "6a0af500be617abd9ae3f566", SequenceNo = 106 },
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.AddOrEditAlarmEventIdracServers, IsActive = true, ParentsScreenId = "6a0af500be617abd9ae3f566", SequenceNo = 107 },
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.DeleteAlarmEventIdracServers, IsActive = true, ParentsScreenId = "6a0af500be617abd9ae3f566", SequenceNo = 108 },
            };

            return screens;
        }
    }
}
