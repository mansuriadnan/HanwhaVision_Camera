using HanwhaClient.Model.DbEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Dto
{
    public class RMAResponseDto
    {
        public string? Id { get; set; }
        public string? DeviceName { get; set; }
        public string? IpAddress { get; set; }
        public string? DeviceId { get; set; }
        public string? DeviceType { get; set; }
        public string? Model { get; set; }
        public string? Location { get; set; }
        public string? SerialNumber { get; set; }
        public string? RMAStatus { get; set; }
        public string? InProgressNotes { get; set; }
        public string? CompletedNotes { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? FloorId { get; set; } = null;
        public string? ZoneId { get; set; } = null;
    }

    public class RMAWidgetResponse
    {
        public string? Id { get; set; }
        public string? DeviceId { get; set; }
        public string? RMAStatus { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

    }

    public class RMAWidgetRequest
    {
        public IEnumerable<string> FloorIds { get; set; }
        public IEnumerable<string>? ZoneIds { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }        
        public IEnumerable<string> DeviceIds { get; set; }
    }

}
