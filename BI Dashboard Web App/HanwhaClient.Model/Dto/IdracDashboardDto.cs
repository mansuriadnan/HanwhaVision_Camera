using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Dto
{
    public class IdracServerListDashboardResponse
    {
        public string? Id { get; set; }
        public string ServerName { get; set; }
        public string ParentSiteId { get; set; }
        public string? ChildSiteId { get; set; }
        public string IPAddress { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class IdracEventLogsListResponse
    {
        public List<IdracEventLogsListRes> EventLogsDetails { get; set; } = [];

        public long TotalCount { get; set; }
    }
    public class IdracEventLogsListRes
    {
        public string? Id { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset? EventTimestamp { get; set; }
        public string? Severity { get; set; }
    }
    public class IdracEventLogsListRequest : PagingSortingModel
    {
        public string IpAddress { get; set; }
    }
    public class IdracLedIndicatorRequest
    {
        public string ServerId { get; set; }
        public string IpAddress { get; set; }
        public bool LedState { get; set; }
    }
    public class IdracPowerActionRequest
    {
        public string ServerId { get; set; }
        public string IpAddress { get; set; }
        public string ResetType { get; set; }
    }
    public class IdracSystemInformationResponse
    {
        public string? PowerState { get; set; }
        public string? Model { get; set; }
        public string? HostName { get; set; }
        public string? OperatingSystem { get; set; }
        public string? OperatingSystemVersion { get; set; }
        public string? ServiceTag { get; set; }
        public string? BiosVersion { get; set; }
        public string? IdracFirmwareVersion { get; set; }
        public string? IPAddress { get; set; }
        public string? IdracMacAddress { get; set; }
        public string? License { get; set; }
    }
    public class IdracSystemInformationRequest
    {
        public string IpAddress { get; set; } = string.Empty;
    }
    public class IdracSystemLogsListRes
    {
        public string? Id { get; set; }
        public string? Description { get; set; }
        public DateTime? SystemTimestamp { get; set; }
        public string? Severity { get; set; }
    }
    public class IdracSystemtLogsListResponse
    {
        public List<IdracSystemLogsListRes> SystemLogsDetails { get; set; } = [];
        public long TotalCount { get; set; }
    }
    public class IdracSystemLogsListRequest : PagingSortingModel
    {
        public string IdracServerId { get; set; }
    }

    public class IdracStatusReponse
    {
        public string ServerId { get; set; }
        public string Health { get; set; }
    }

    public class IdracOverallResponse {
        public IdracDetails IdracDetails { get; set; }
        public bool LightIndigator { get; set; }
        public List<IdracStatusReponse> ServerHealth { get; set; }
    }

    public class AlarmEventDetails
    {
        public string Event { get; set; }
        public int TimeLimit { get; set; }
    }
}
