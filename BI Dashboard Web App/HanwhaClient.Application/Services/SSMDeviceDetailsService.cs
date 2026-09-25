using HanwhaClient.Application.Interfaces;
using HanwhaClient.Infrastructure.Interfaces;

namespace HanwhaClient.Application.Services
{
    public class SSMDeviceDetailsService : ISSMDeviceDetailsService
    {
        private readonly ISSMDeviceDetailsRepository _sSMDeviceDetailsRepository;

        public SSMDeviceDetailsService(ISSMDeviceDetailsRepository sSMDeviceDetailsRepository)
        {
            _sSMDeviceDetailsRepository = sSMDeviceDetailsRepository;
        }
    }
}
