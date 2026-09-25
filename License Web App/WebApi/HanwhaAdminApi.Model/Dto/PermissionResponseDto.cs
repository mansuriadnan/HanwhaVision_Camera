using HanwhaAdminApi.Model.DbEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaAdminApi.Model.Dto
{
    public class PermissionResponseDto
    {
        public IEnumerable<ScreenMaster> ScreenPermission { get; set; }
        public IEnumerable<RegionPermissionResponse> RegionPermission { get; set; }

    }
}
