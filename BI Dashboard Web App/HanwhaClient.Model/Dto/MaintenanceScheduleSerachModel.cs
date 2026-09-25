using HanwhaClient.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Dto
{
    public class MaintenanceScheduleSerachModel : PagingSortingModel
    {
        public IEnumerable<string>? FloorIds { get; set; }
        public IEnumerable<string>? ZoneIds { get; set; }
        public string? DueFilter { get; set; }
        public string? StatusFilter { get; set; }
        public IEnumerable<string>? DeviceIds { get; set; }
    }

    public class CompareImageResponse
    {
        public string OverallSimilarity { get; set; }
        public string FeatureDisplacement { get; set; }
        public string EdgeOverlap { get; set; }
        public string BrightnessDifference { get; set; }
        public string ContrastDifference { get; set; }
        public string HomographyShift { get; set; }
        public string HorizonTiltDifference { get; set; }
        public string StructuralSimilarity { get; set; }
    }
}
