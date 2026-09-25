using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Dto
{
    public class MonitoringGroupReqDto
    {
        public string MonitoringId { get; set; }
        public string? MonitoringGroupId { get; set; }
        public string? MonitoringGroupItemId { get; set; }
    }
}
