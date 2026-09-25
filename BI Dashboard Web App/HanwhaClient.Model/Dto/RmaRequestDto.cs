using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Dto
{
    public class RmaRequestDto
    {
        public string? Id { get; set; }
        public string DeviceId { get; set; } = null!;
        public string? RMAStatus { get; set; }
        public string? InProgressNotes { get; set; }
        public string? CompletedNotes { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? FloorId { get; set; } = null;
        public string? ZoneId { get; set; } = null;
    }
}
