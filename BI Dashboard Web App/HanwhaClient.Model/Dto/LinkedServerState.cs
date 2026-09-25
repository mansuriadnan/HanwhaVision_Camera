using HanwhaClient.Model.DbEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Dto
{
    public class LinkedServerCacheEntry
    {
        public bool IsAvailable { get; set; }
        public bool IsActive { get; set; }
        public List<FloorDataAccessPermission> Permissions { get; set; } = new();
    }
}
