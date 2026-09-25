using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace HanwhaClient.Model.Dto
{
    public class CountryResponse
    {
        public string Id { get; set; }
        public string CountryName { get; set; }
        public bool HasState { get; set; }
        public string CountryCode { get; set; }
    }

    public class CountryInfoModel
    {
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
        public bool HasState { get; set; }
    }

}
