using HanwhaClient.Model.DbEntities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Dto
{

    public class MaintenanceScheduleSearchResponseDto
    {
        public string? Id { get; set; }           // Unique identifier
        public string? DeviceName { get; set; }   // Name of the device
        public string? DeviceId { get; set; }   // Name of the device
        public string? DeviceType { get; set; }   // Type of the device
        public string? Model { get; set; }        // Model of the device
        public string? Location { get; set; }     // Location of the device
        public string? SerialNumber { get; set; } // Serial number of the device
        public string? IpAddress { get; set; } // Serial number of the device
        public string? PlanName { get; set; } // Last Plan Name
        public string? BeforeCameraImage { get; set; } 
        public string? AfterCameraImage { get; set; } 
        public DateTime? MaintenanceDueDate { get; set; }  // Next due date for maintenance
        public string Status { get; set; }
        public IEnumerable<StatusHistoryItem> StatusHistory { get; set; } = Enumerable.Empty<StatusHistoryItem>();
       
    }

    public class MaintenanceStatusWidgetResponse
    {
        public string DeviceId { get; set; }

        public List<StatusHistoryItem> StatusHistory { get; set; } = new List<StatusHistoryItem>();

        public string LatestStatus { get; set; }

        public DateTime? DueDate { get; set; }

    }

}
