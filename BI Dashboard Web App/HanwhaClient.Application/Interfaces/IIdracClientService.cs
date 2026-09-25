using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Interfaces
{
    public interface IIdracClientService
    {
        Task<bool> IdracLoginAsync(string ipAddress, string username, string password);
        Task<T?> GetIdracAsync<T>(string url, string ipAddress);
        Task<T?> PatchIdracAsync<T>(string url, object body, string ipAddress);
        Task<T?> PostIdracAsync<T>(string url, object body, string ipAddress);
        Task<bool> DeleteIdracAsync(string url, string ipAddress);
        Task<bool> IdracLogoutAsync(string ipAddress, string? sessionId = null);
    }
}
