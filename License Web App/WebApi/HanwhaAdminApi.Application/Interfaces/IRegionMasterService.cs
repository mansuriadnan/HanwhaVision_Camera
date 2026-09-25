using HanwhaAdminApi.Model.Common.ReferenceData;
using HanwhaAdminApi.Model.DbEntities;
using HanwhaAdminApi.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaAdminApi.Application.Interfaces
{
    public interface IRegionMasterService
    {
        Task<(string Id, string ErrorMessage)> SaveRegionAsync(RequestRegionMasterDto userRequest, string userId);
        Task<(IEnumerable<RegionMasterResponseDto> data, Dictionary<string, object> referenceData)> GetAllRegionAsync();
        Task<(bool IsSuccess, string ErrorMessage)> DeleteRegionAsync(string id, string userId);
        Task<List<OptionModel<string, string>>> GetRegionNameReferenceDataAsync(IEnumerable<string> ids);
    }
}
