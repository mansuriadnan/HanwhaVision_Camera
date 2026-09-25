using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaAdminApi.Model.Dto
{
    public class RegionPermissionResponse
    {
        public string Id { get; set; }
        public string RegionName { get; set; }
        public bool AccessAllowed { get; set; }
    }
}
