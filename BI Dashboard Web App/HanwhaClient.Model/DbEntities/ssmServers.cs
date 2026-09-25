using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HanwhaClient.Model.DbEntities
{
    public class SsmServers : BaseModel
    {
        [BsonElement("ssmSiteId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string SsmSiteId { get; set; }

        [BsonElement("status")]
        public int Status { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("ipAddress")]
        public string IpAddress { get; set; }

        [BsonElement("port")]
        public int Port { get; set; }

        [BsonElement("serverStatus")]
        public string ServerStatus { get; set; }

    }
}
