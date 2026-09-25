using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Dto
{
    public class SSMServerManagementRequest
    {
        public string? Id { get; set; }
        public string ParentSiteId {  get; set; }
        public string? ChildSiteId { get; set; }
        public string IPAddress {  get; set; }
        public string? Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool IsHttps { get; set; }
    }
    public class DeleteSsmServerRequest
    {
        public string Id { get; set; }
    }
}
