using HanwhaClient.Model.Common;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Dto
{
    public class IDracDto
    {
    }
    public class IDracServerManagementRequest
    {
        public string? Id { get; set; }
        public string ParentSiteId { get; set; }
        public string? ChildSiteId { get; set; }
        public string ServerName { get; set; }
        public string IPAddress { get; set; }
        public string Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public double CpuLoad { get; set; }
        public double Temperature { get; set; }
        public double MemoryUsage { get; set; }

    }
    public class DeleteIDracServerRequest
    {
        public string Id { get; set; }
    }
    public class IDracEventAlarmRequest
    {     
        public string ServerId { get; set; }
        public string IpAddress { get; set; }
        public string Name { get; set; }
        public string Event { get; set; }
        public int TimeLimit { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

    }
    public class IdracSubscriptionRequest
    {
        [JsonPropertyName("Destination")]
        public string? Destination { get; set; }

        [JsonPropertyName("Protocol")]
        public string? Protocol { get; set; }

        [JsonPropertyName("EventTypes")]
        public List<string>? EventTypes { get; set; }

        [JsonPropertyName("Context")]
        public string? Context { get; set; }

        [JsonPropertyName("HttpHeaders")]
        public List<Dictionary<string, string>> HttpHeaders { get; set; }
    }
    public class HttpHeader
    {
        [JsonPropertyName("Name")]
        public string? Name { get; set; }

        [JsonPropertyName("Value")]
        public string? Value { get; set; }
    }
    public class DeleteIdracSubscribedEventRequest
    {
        public string AlarmEventId { get; set; }
        public string ServerId { get; set; }
    }    
}
