using HanwhaClient.Model.DbEntities;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Common
{
    public class MultiserverScreenName
    {
        public static List<ScreenMaster> GetAllMultiServerScreenName()
        {
            List<ScreenMaster> screens = new List<ScreenMaster>
            {
                     // Multiserver screens
                      new ScreenMaster { Id = "68555f3f9b7e4c2d8a1f7b91", ScreenName = ScreenNames.MultiServerMaster, IsActive = true, ParentsScreenId = null, SequenceNo = 109 },
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.ViewListofMultiServers, IsActive = true, ParentsScreenId = "68555f3f9b7e4c2d8a1f7b91", SequenceNo = 110 },
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.AddOrUpdateMultiServers, IsActive = true, ParentsScreenId = "68555f3f9b7e4c2d8a1f7b91", SequenceNo = 111 },
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.DeleteMultiServers, IsActive = true, ParentsScreenId = "68555f3f9b7e4c2d8a1f7b91", SequenceNo = 112 },
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.EnabledMultiServers, IsActive = true, ParentsScreenId = "68555f3f9b7e4c2d8a1f7b91", SequenceNo = 113 }
                      
            };

            return screens;
        }
    }
}
