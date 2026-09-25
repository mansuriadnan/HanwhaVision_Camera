using System.Text.Json.Serialization;

namespace HanwhaClient.Model.SSM
{
    public class SSMLoginResponse
    {
        [JsonPropertyName("SessionId")] public string? SessionId { get; set; }
        [JsonPropertyName("UID")] public string? Uid { get; set; }
        [JsonPropertyName("secretKey")] public string? SecretKey { get; set; }
        [JsonPropertyName("userStatus")] public int UserStatus { get; set; }
    }

    public class SsmServerSummary
    {
        [JsonPropertyName("guid")] public string Guid { get; set; } = "";
        [JsonPropertyName("name")] public string Name { get; set; } = "";
        [JsonPropertyName("address")] public string Address { get; set; } = "";
        [JsonPropertyName("status")] public int Status { get; set; }
        public Child Child { get; set; }
    }

    public class Child
    {
        public string Type { get; set; }
        public int Total { get; set; }
        public int Failure { get; set; }
    }

    public class SsmServerDto
    {
        public string Guid { get; set; }
        public int Status { get; set; }
        public SsmServerData Data { get; set; }
    }

    public class SsmServerData
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }
        public int Port { get; set; }
        public string TotalProcessorUsage { get; set; }
        public string TotalMemoryUsage { get; set; }
        public string ServerProcessorUsage { get; set; }
        public string ServerMemoryUsage { get; set; }
        public string GatewayProcessorUsage { get; set; }
        public string GatewayMemoryUsage { get; set; }

        [JsonExtensionData]
        public Dictionary<string, System.Text.Json.JsonElement>? ExtensionData { get; set; }

        public string DiskTotalSize { get; set; }
        public string DiskFreeSize { get; set; }
        public string DiskFreePercentage { get; set; }
        public int TotalCameraCount { get; set; }
        public int FailureCameraCount { get; set; }
    }

    public class SsmCameraDto
    {
        public string MonitorType { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public SsmCameraData Data { get; set; }
    }

    public class SsmCameraData
    {
        public string Name { get; set; }
        public string NameExt { get; set; }
        public string Address { get; set; }
        public string Location { get; set; }
    }
}
