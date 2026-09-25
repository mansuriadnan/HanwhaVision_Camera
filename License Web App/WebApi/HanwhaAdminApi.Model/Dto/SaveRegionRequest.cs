using HanwhaAdminApi.Model.DbEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaAdminApi.Model.Dto
{
    public class SaveRegionRequest
    {
        public string RoleId { get; set; }
        public IEnumerable<RegionPermission> RegionPermissions { get; set; } = [];
    }
}
