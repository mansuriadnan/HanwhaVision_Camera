using HanwhaClient.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Interfaces
{
    public interface ISSMDashboardService
    {
        Task<List<SsmServerHierarchyResponse>> GetSsmServerHierarchyAsync(List<string> parentSiteIds, DateTime date, string userId);
        Task<List<SsmServerAvailabilityResponse>> GetServerAvailabilityAsync(SsmServerAvailabilityRequest request, string userId);
        Task<List<SsmDeviceAvailabilityResponse>> GetSsmDeviceAvailabilityAsync(SsmDeviceAvailabilityRequest request, string userId);
        Task<SSMDeviceDetailsResponse> GetSsmDeviceDetailsAsync(SSMDeviceDetailsRequest request, string userId);
        Task<SsmServerCpuUtilizationResponse> GetSsmServerCpuUtilizationAsync(SsmServerCpuUtilizationRequest request, string userId);
        Task<SsmServerRamUtilizationResponse> GetSsmServerRamUtilizationAsync(SsmServerRamUtilizationRequest request, string userId);
        Task<List<SsmServerHealthReportResponse>> GetSsmServerHealthReportAsync(SsmServerHealthReportRequest request, string userId);
    }
}
