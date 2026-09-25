using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaAdminApi.Model.Dto
{
    public class RegionMasterResponseDto
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }

    }
}
