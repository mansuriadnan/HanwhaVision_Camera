using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace HanwhaClient.Model.DbEntities
{
    public class ViMultiServerManagement : BaseModel
    {
        [BsonElement("serverName")]
        public string ServerName { get; set; }

        [BsonElement("databaseConnectionString")]
        public string DatabaseConnectionString { get; set; }

        [BsonElement("hostingAddress")]
        public string HostingAddress { get; set; }

        [BsonElement("username")]
        public string Username { get; set; }

        [BsonElement("password")]
        public string Password { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;
        
        [BsonElement("isAvailable")]
        public bool IsAvailable { get; set; }

    }
}
 