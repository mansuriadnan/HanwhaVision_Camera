using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace HanwhaClient.Model.DbEntities
{
    public class Country : BaseModel
    {
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("hasStates")]
        public bool HasStates { get; set; }

        [BsonElement("code")]
        public string Code { get; set; }
        public bool IsDeleted { get; set; }
    }
}
