using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using HanwhaClient.Model.Common;

namespace HanwhaClient.Model.Dto
{
    public class VehicleOwnerRequest
    {
        public string? Id { get; set; }
        public string RegistrationType { get; set; }        
        public string OwnerName { get; set; }        
        public string Building { get; set; }        
        public string BuildingUnit { get; set; }        
        public string Email { get; set; }        
        public string ContactNumber { get; set; }        
        public int AllowedVehicle { get; set; }
        public DateTime OwnerValidTo { get; set; }
        public DateTime? AllowedFromTime { get; set; }        
        public DateTime? AllowedToTime { get; set; }
        public string[] AllowedGates { get; set; }
        public bool EnabledAlarmForOverstay { get; set; }
        public bool EnabledAlarmFor24HStay { get; set; }
    }
    public class AllVehicleOwnerList
    {
        public string? Id { get; set; }
        public string RegistrationType { get; set; }
        public string OwnerName { get; set; }
        public string Building { get; set; }
        public string BuildingUnit { get; set; }
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public int AllowedVehicle { get; set; }
        public DateTime? OwnerValidTo { get; set; }
        public DateTime? AllowedFromTime { get; set; }
        public DateTime? AllowedToTime { get; set; }
        public IEnumerable<string> AllowedGates { get; set; }
        public IEnumerable<string> AllowedGateNames { get; set; }
        public bool EnabledAlarmForOverstay { get; set; }
        public bool EnabledAlarmFor24HStay { get; set; }
    }
    public class AllVehicleOwnerListResponse
    {
        public int TotalCount { get; set; }
        public List<AllVehicleOwnerList> allVehicleOwnerLists { get; set; }
    }
    public class AllVehicleOwnerRequest : PagingSortingModel
    {
        public string? SearchText { get; set; }
    }
    public class VehicleOwneDeleteRequest
    {
        public string Id { get; set; }
    }
}
