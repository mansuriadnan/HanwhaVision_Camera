using HanwhaClient.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Dto
{
    public class MaintenancePlanSerachModel : PagingSortingModel
    {
        public string? SearchText { get; set; }          // optional search
    }
}
