using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.DbEntities
{
    public class ExposeApiUser : BaseModel
    {
        [BsonElement("Username")]
        public string Username { get; set; }

        [BsonElement("password")]
        public string Password { get; set; }
        
        [BsonElement("role")]
        public string Role { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;
    }
}
