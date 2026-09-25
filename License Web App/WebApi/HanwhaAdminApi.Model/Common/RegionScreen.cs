using HanwhaAdminApi.Model.DbEntities;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaAdminApi.Model.Common
{
    public class RegionScreen
    {
        public static List<ScreenMaster> GetAllRegionScreen()
        {
            List<ScreenMaster> screens = new List<ScreenMaster>
            {
                      new ScreenMaster { Id = "696f2c815a1be547fde13062", ScreenName = ScreenNames.RegionMaster, IsActive = true, ParentsScreenId = null, SequenceNo = 21 },
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.CanViewRegion, IsActive = true, ParentsScreenId = "696f2c815a1be547fde13062", SequenceNo = 22 },
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.CanAddOrUpdateRegion, IsActive = true, ParentsScreenId = "696f2c815a1be547fde13062", SequenceNo = 23 },
                      new ScreenMaster { Id = ObjectId.GenerateNewId().ToString(), ScreenName = ScreenNames.CanDeleteRegion, IsActive = true, ParentsScreenId = "696f2c815a1be547fde13062", SequenceNo = 24 },
                    
            };

            return screens;
        }
    }
}
