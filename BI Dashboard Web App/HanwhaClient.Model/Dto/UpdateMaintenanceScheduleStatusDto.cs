using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Dto
{
    public class UpdateMaintenanceScheduleStatusDto : StatusHistoryItemDto
    {
        public string? MaintenanceScheduleId { get; set; }
    }

    public class CImages
    {
        public IFormFile img1 { get; set; } = null!;
        public IFormFile img2 { get; set; } = null!;
    }
}
