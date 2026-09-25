using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Dto
{
    public class MaintenancePlanDto
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "Plan name is required")]
        public string PlanName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Duration is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Duration must be greater than 0")]
        public int Duration { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; } = null;

        [Required]
        public List<string> DeviceIds { get; set; } = new List<string>();
        public List<string> DeviceNames { get; set; } = new();

        public DateTime? NextExecutionDate { get; set; } = null;
        public List<string> FloorIds { get; set; } = new List<string>();
        public List<string> ZoneIds { get; set; } = new List<string>();
    }
}
