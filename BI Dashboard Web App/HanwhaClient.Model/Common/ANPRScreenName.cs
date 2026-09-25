using HanwhaClient.Model.DbEntities;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Common
{
    public class ANPRScreenName
    {
        public static List<ScreenMaster> GetAllANPRScreen()
        {
            List<ScreenMaster> screens = new List<ScreenMaster>
            {
                     // ANPR Owner Master screens
                      new ScreenMaster { Id = "693005356e6f6f8d912151c6", ScreenName = ScreenNames.VehicleOwner, IsActive = true, ParentsScreenId = null, SequenceNo = 64 },
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.ViewVehicleOwner, IsActive = true, ParentsScreenId = "693005356e6f6f8d912151c6", SequenceNo = 65 },  // Dashboard Preference Master screens
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.AddOrUpdateVehicleOwner, IsActive = true, ParentsScreenId = "693005356e6f6f8d912151c6", SequenceNo = 66 },  // Dashboard Preference Master screens
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.DeleteVehicleOwner, IsActive = true, ParentsScreenId = "693005356e6f6f8d912151c6", SequenceNo = 67 },  // Dashboard Preference Master screens   
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.ViewANPRVehicle, IsActive = true, ParentsScreenId = "693005356e6f6f8d912151c6", SequenceNo = 68 },  // Dashboard Preference Master screens   
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.AddOrUpdateANPRVehicle, IsActive = true, ParentsScreenId = "693005356e6f6f8d912151c6", SequenceNo = 69 },  // Dashboard Preference Master screens   
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.DeleteANPRVehicle, IsActive = true, ParentsScreenId = "693005356e6f6f8d912151c6", SequenceNo = 70 },  // Dashboard Preference Master screens   
            };

            return screens;
        }
    }
}
