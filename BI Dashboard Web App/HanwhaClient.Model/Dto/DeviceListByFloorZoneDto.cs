using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Dto
{
    public class DeviceListByFloorZoneDto
    {
        public string? DeviceId { get; set; }
        public string? DeviceName { get; set; }
        public string? FloorId { get; set; }
        public string? ZoneId { get; set; }
    }
}
