using HanwhaClient.Model.DbEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Model.Dto
{
    public class SSMDashboardDto
    {
    }
    public class SsmHierarchyRequest
    {
        public List<string> ParentSiteIds { get; set; }
        public DateTime Date { get; set; }        
    }
    public class SsmServerHierarchyResponse
    {
        public string ParentSiteId { get; set; }
        public string ParentSiteName { get; set; }
        public List<SsmDashboardServerDto> ParentServers { get; set; }
        public List<SubSiteServerDto> SubSites { get; set; }
    }

    public class SubSiteServerDto
    {
        public string ChildSiteId { get; set; }
        public string ChildSiteName { get; set; }
        public List<SsmDashboardServerDto> Servers { get; set; }
    }

    public class SsmDashboardServerDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public string Status { get; set; }
        public DateTime? UpdatedOn { get; set; }
        // Metrics
        public decimal TotalProcessorUsage { get; set; }
        public decimal TotalMemoryUsage { get; set; }
        public decimal CpuSystemUsage { get; set; }
        public decimal MemorySystemUsage { get; set; }
        public decimal CpuMediaUsage { get; set; }
        public decimal MemoryMediaUsage { get; set; }

        public List<DiskDetail> Disks { get; set; } = new List<DiskDetail>();
        //public long CDiskFreeSize { get; set; }
        //public long CDiskTotalSize { get; set; }
        //public long DDiskFreeSize { get; set; }
        //public long DDiskTotalSize { get; set; }
        //public long EDiskFreeSize { get; set; }
        //public long EDiskTotalSize { get; set; }
        public long DiskTotalSize { get; set; }
        public long DiskFreeSize { get; set; }
        public decimal DiskFreePercentage { get; set; }
        public int TotalCameraCount { get; set; }
        public int FailureCameraCount { get; set; }
    }

    public class SsmServerAvailabilityRequest
    {
        public string ServerId { get; set; }
        public DateTime SearchDate { get; set; }
    }

    public class SsmServerAvailabilityResponse
    {
        public string Id { get; set; }
        public DateTime OfflineTime { get; set; }
        public DateTime? OnlineTime { get; set; }
    }

    public class SsmDeviceAvailabilityRequest
    {
        public string DeviceId { get; set; }
        public DateTime SearchDate { get; set; }
    }

    public class SsmDeviceAvailabilityResponse
    {
        public string Id { get; set; }
        public DateTime OfflineTime { get; set; }
        public DateTime? OnlineTime { get; set; }
    }

    public class SsmServerCpuUtilizationRequest
    {
        public string ServerId { get; set; }
        public DateTime Date { get; set; }
    }

    public class SsmServerCpuUtilizationData
    {
        public DateTime CreatedOn { get; set; }
        public decimal TotalUsage { get; set; }
    }

    public class SsmServerCpuUtilizationResponse
    {
        public List<SsmServerCpuUtilizationData> ChartData { get; set; }
        public decimal CpuSystemUsage { get; set; }
        public decimal CpuMediaUsage { get; set; }
    }

    public class SsmServerRamUtilizationRequest
    {
        public string ServerId { get; set; }
        public DateTime Date { get; set; }
    }

    public class SsmServerRamUtilizationData
    {
        public DateTime CreatedOn { get; set; }
        public decimal TotalUsage { get; set; }
    }

    public class SsmServerRamUtilizationResponse
    {
        public List<SsmServerRamUtilizationData> ChartData { get; set; }
        public decimal MemorySystemUsage { get; set; }
        public decimal MemoryMediaUsage { get; set; }
    }
}
