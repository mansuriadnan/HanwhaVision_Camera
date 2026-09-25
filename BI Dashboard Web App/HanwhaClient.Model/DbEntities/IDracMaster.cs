using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.DbEntities
{
    public class IDracMaster : BaseModel
    {
        [BsonElement("ipAddress")]
        public string IPAddress { get; set; }

        [BsonElement("port")]
        public string Port { get; set; }

        [BsonElement("parentSiteId")]
        public string? ParentSiteId { get; set; }

        [BsonElement("childSiteId")]
        public string? ChildSiteId { get; set; }

        [BsonElement("serverName")]
        public string ServerName { get; set; }

        [BsonElement("userName")]
        public string UserName { get; set; }

        [BsonElement("password")]
        public string Password { get; set; }        

        [BsonElement("cpuLoad")]
        public double CpuLoad { get; set; }

        [BsonElement("temperature")]
        public double Temperature { get; set; }

        [BsonElement("memoryUsage")]
        public double MemoryUsage { get; set; }

        [BsonElement("ledState")]
        public bool LedState { get; set; }

        [BsonElement("powerState")]
        public string? PowerState { get; set; }

        [BsonElement("eventSubscriptionId")]
        public string? EventSubscriptionId { get; set; }

        [BsonElement("alarms")]
        public List<AlarmDetails> Alarms { get; set; } = [];
    }
    public class AlarmDetails
    {
        [BsonElement("alarmId")]
        public string AlarmId { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("event")]
        public string Event { get; set; }

        [BsonElement("timeLimit")]
        public int TimeLimit { get; set; }        
    }
}
