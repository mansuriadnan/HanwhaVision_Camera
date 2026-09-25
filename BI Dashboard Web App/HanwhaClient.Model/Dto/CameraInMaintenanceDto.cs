using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Dto
{
    public class CameraInMaintenanceSearchDto : StatusHistoryItemDto
    {
        public IEnumerable<string> DeviceId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

    }

    public class CameraInMaintenance : StatusHistoryItemDto
    {
        public string? Id { get; set; }
        public string? DeviceId { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class CameraInMaintenanceResDto
    {
        public int CameraInMaintenanceCount { get; set; }
        public IEnumerable<CameraInMaintenance>? CameraInMaintenance { get; set; }
    }

    public class StatusHistoryCSV
    {
        public string Status { get; set; } = string.Empty;
        public DateTime StatusDatetime { get; set; }
    }

    // Model for each item containing status history
    public class Item
    {
        public List<StatusHistoryCSV> StatusHistory { get; set; } = new List<StatusHistoryCSV>();
    }

    public class CameraMaintenanceStatusCSV
    {
        public DateTime? DateTime { get; set; }
        public int NotStartedCount { get; set; }
        public int InProgressCount { get; set; }
        public int ReworkCount { get; set; }
        public int DoneCount { get; set; }
    }
    public class CameraInMaintenanceCSV
    {
        public DateTime? DateTime { get; set; }
        public int InProgress { get; set; }
        public int Rework { get; set; }
    }
}
