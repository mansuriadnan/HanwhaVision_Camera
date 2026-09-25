using HanwhaClient.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Dto
{
    public class RMASerachModel : PagingSortingModel
    {
        public IEnumerable<string>? FloorIds { get; set; }
        public IEnumerable<string>? ZoneIds { get; set; }
        public string? StartDate { get; set; }
        public string? RMAStatus { get; set; }
        public IEnumerable<string>? DeviceIds { get; set; }
    }
}
