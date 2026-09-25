using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Interfaces
{
    public interface ISsmClientService
    {
        Task<bool> LoginAsync(string baseAddress, string id, string password);
        Task<T?> GetAsync<T>(string path);
        Task<bool> LogoutAsync(string path);
    }
}
