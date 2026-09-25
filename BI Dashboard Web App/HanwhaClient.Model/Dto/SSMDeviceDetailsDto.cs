using HanwhaClient.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Dto
{
    public class SSMDeviceDetailsDto
    {
    }
    public class SSMDeviceDetailsRequest : PagingSortingModel
    {
        public string ServerId { get; set; }
        public string SearchText { get; set; }
        public DateTime DateFilter { get; set; }
    }
    public class SSMDeviceDetailsResponse
    {
        public int TotalCount { get; set; }
        public List<SSMDeviceDetailsItem> SSMDeviceDetails { get; set; }        
    }
    public class SSMDeviceDetailsItem
    {        
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? CameraModel { get; set; }
        public string? IpAddress { get; set; }
        public string? Location { get; set; }
        public string? CameraStatus { get; set; }
        public string? RecordingStatus { get; set; }
    }

}
