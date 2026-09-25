using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Dto
{
    public class ViMultiServerManagementDTO
    {
        public string? Id { get; set; }
        public string ServerName { get; set; }
        public string DatabaseConnectionString { get; set; }
        public string HostingAddress { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class DatabaseConnectionRequest
    {
        public string HostingAddress { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class DeleteServerManagement
    {
        public string Id { get; set; }
    }

    public class EnabledServerRequest
    {
        public string Id { get; set; }
        public bool IsActive { get; set; }
    }
}
