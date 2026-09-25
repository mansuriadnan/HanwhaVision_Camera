using HanwhaClient.Application.Interfaces;
using HanwhaClient.Model.DbEntities;
using Microsoft.Extensions.DependencyInjection;


namespace HanwhaClient.Application.Services
{
    public class ArchiveTimeService : IArchiveTimeService
    {
        public ClientSettings CurrentClientSettings { get; set; } = new ClientSettings();
        private IClientSettingService _clientSettingService;
        private readonly IServiceScopeFactory _scopeFactory;

        public ArchiveTimeService(IServiceScopeFactory scopeFactory)
        {
            this._scopeFactory = scopeFactory;
        }


        public async Task GetClinetSettings()
        {
            using (var scope = this._scopeFactory.CreateScope())
            {
                _clientSettingService = scope.ServiceProvider.GetRequiredService<IClientSettingService>();
                CurrentClientSettings = await _clientSettingService.GetClientSetting();
            }
        }


        public bool CheckCollectionArchiceTime(DateTime filterTime)
        {
            if(CurrentClientSettings != null && CurrentClientSettings.RetentionDBConfiguration != null && CurrentClientSettings.RetentionDBConfiguration.Enable)
            {
                if(CurrentClientSettings.RetentionDBConfiguration.RetentionTil > filterTime)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
