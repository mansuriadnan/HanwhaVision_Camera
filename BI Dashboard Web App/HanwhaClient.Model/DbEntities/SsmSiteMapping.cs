using MongoDB.Bson.Serialization.Attributes;

namespace HanwhaClient.Model.DbEntities
{
    public class SsmSiteMapping : BaseModel
    {
        [BsonElement("ipAddress")]
        public string IPAddress { get; set; }

        [BsonElement("port")]
        public string? Port { get; set; }

        [BsonElement("parentSiteId")]
        public string? ParentSiteId { get; set; }

        [BsonElement("childSiteId")]
        public string? ChildSiteId { get; set; }

        [BsonElement("userName")]
        public string Username { get; set; }

        [BsonElement("password")]
        public string Password { get; set; }

        [BsonElement("isHttps")]
        public bool IsHttps { get; set; }
    }
}
