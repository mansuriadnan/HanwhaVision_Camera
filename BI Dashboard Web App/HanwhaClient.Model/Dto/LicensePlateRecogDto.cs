using HanwhaClient.Model.Common;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Dto
{
    public class LicensePlateRecogRequest
    {  
        public DateTime CreatedAt { get; set; }
        public BsonDocument DynamicFields { get; set; } = new BsonDocument();
    }
    public class AllLprRequest : PagingSortingModel
    {
        public IEnumerable<string>? FloorIds { get; set; }
        public IEnumerable<string>? ZoneIds { get; set; }
        public IEnumerable<string>? DeviceIds { get; set; }
        public string? CountryName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SearchText { get; set; } // State,Email,OwnerName,RegistartionType
    }
    public class AllLprDetailsResponse
    {
        public string Id { get; set; }
        public string Plate { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public string? Color { get; set; }
        public string RegistrationType { get; set; }
        public string OwnerName { get; set; }
        public string? Building { get; set; }
        public string? buildingUnit { get; set; }
        public string? Email { get; set; }
        public string? Contact { get; set; }
        public string? EntryGate { get; set; }
        public string? ExitGate { get; set; }
        public DateTime? EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }
        public string? Message { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? SmallImage { get; set; }
    }
    public class AnprImageRequest
    {
        public string ImageName { get; set; }
        public string Size { get; set; } = "small";
    }
}
