using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Dto
{
    public class MaintenanceScheduleDto
    {
        public string? Id { get; set; }

        [Required]
        public string DeviceId { get; set; }

        public string? MaintenancePlanId { get; set; }

        [Required]
        public List<StatusHistoryItemDto>? StatusHistory { get; set; } = new List<StatusHistoryItemDto>();

        public string? LatestStatus { get; set; }

        public DateTime? DueDate { get; set; }

        public string? BeforeCameraImage { get; set; }

        public string? AfterCameraImage { get; set; }

        public double? ImageMatchingPercentage { get; set; }

        public bool IsScheduleManually { get; set; } = true;
        public string? FloorId { get; set; } = string.Empty;
        public string? ZoneId { get; set; } = string.Empty;
    }

    public class StatusHistoryItemDto
    {
        public string? Status { get; set; }

        public string? Notes { get; set; }

        public DateTime? StatusDatetime { get; set; }
    }
}
