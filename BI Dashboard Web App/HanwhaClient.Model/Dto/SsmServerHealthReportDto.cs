using System;
using System.Collections.Generic;

namespace HanwhaClient.Model.Dto
{
    public class SsmServerHealthReportRequest
    {
        public DateTime StartDateUtc { get; set; }
        public DateTime EndDateUtc { get; set; }
        public List<string> SsmSiteIds { get; set; } = new List<string>();
    }

    public class SsmServerHealthReportResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Port { get; set; }
        public string Status { get; set; }
        public string IpAddress { get; set; }
        public string SiteId { get; set; }
        public string SiteName { get; set; }
        public double UptimePercent { get; set; }
        public double DownTimeMin { get; set; }
        public decimal CpuAvgPercent { get; set; }
        public decimal RamAvgPercent { get; set; }
        public int TotalCameras { get; set; }
        public int OfflineCameras { get; set; }
        public decimal CpuMaxUtilizationPercent { get; set; }
        public decimal TotalRamAvailable { get; set; }
        public decimal RamMaxUtilizationPercent { get; set; }
        public long TotalDiskSpace { get; set; }
        public long FreeDiskSpace { get; set; }
        public decimal DiskUtilizationPercent { get; set; }

        public List<OfflineOnlineData> ServerOfflineOnlineData { get; set; } = new List<OfflineOnlineData>();
        public List<CpuSpikeData> CpuSpikeData { get; set; } = new List<CpuSpikeData>();
        public List<RamSpikeData> RamSpikeData { get; set; } = new List<RamSpikeData>();
        public List<DiskSpikeData> DiskSpikeData { get; set; } = new List<DiskSpikeData>();
        public List<DeviceOfflineData> DeviceOffline { get; set; } = new List<DeviceOfflineData>();
    }

    public class DeviceOfflineData
    {
        public string Id { get; set; }
        public string Ip { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }
        public string Online { get; set; }
        public int OfflineIncidence { get; set; }
        public string OfflineDuration { get; set; }
        public string Rec { get; set; }
        public int NoRecIncidence { get; set; }
        public string NoRecDuration { get; set; }
        public string Status { get; set; }

        public List<DeviceOfflineEvent> OfflineEvents { get; set; } = new List<DeviceOfflineEvent>();
        public List<DeviceStopRecordingEvent> StopRecording { get; set; } = new List<DeviceStopRecordingEvent>();
    }

    public class DeviceOfflineEvent
    {
        public string Id { get; set; }
        public DateTime OfflineTime { get; set; }
        public DateTime? OnlineTime { get; set; }
        public int Duration { get; set; }
    }

    public class DeviceStopRecordingEvent
    {
        public string Id { get; set; }
        public DateTime StopRecordingTime { get; set; }
        public DateTime? StartRecordingTime { get; set; }
        public int Duration { get; set; }
    }

    public class OfflineOnlineData
    {
        public string Id { get; set; }
        public DateTime? OnlineTime { get; set; }
        public DateTime? OfflineTime { get; set; }
    }

    public class CpuSpikeData
    {
        public string Id { get; set; }
        public DateTime? CpuSpikeStartDatetime { get; set; }
        public DateTime? CpuNormalDatetime { get; set; }
    }

    public class RamSpikeData
    {
        public string Id { get; set; }
        public DateTime? RamSpikeStartDatetime { get; set; }
        public DateTime? RamNormalDatetime { get; set; }
    }

    public class DiskSpikeData
    {
        public string Id { get; set; }
        public string Drive { get; set; }
        public string IpAddress { get; set; }
        public DateTime? DiskSpikeStartDatetime { get; set; }
        public DateTime? DiskNormalDatetime { get; set; }
    }
}
