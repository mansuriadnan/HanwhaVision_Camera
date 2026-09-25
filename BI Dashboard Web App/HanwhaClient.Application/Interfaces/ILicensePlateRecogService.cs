using HanwhaClient.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Interfaces
{
    public interface ILicensePlateRecogService
    {
        Task<(string Id, string ErrorMessage)> SaveLicensePlateRecogDetailsAsync(JsonElement request, string userId);
        Task<(PagedResult<AllLprDetailsResponse> Data, Dictionary<string, object> ReferenceData)> GetAllLprDetailsAsync(AllLprRequest request);
        Task<StringBuilder> GetLPRDtailsCSVAsync(AllLprRequest lprRequest,string userId);
    }
}
