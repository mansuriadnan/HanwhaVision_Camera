using HanwhaClient.Infrastructure.Repository;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using Microsoft.Extensions.Options;

namespace HanwhaClient.Server.BackgroundTask
{
    public class AuditManagerJob : IHostedService
    {
        private readonly IAuditStreamRepository<UserMaster> _userAudit;
        private readonly IAuditStreamRepository<RoleMaster> _roleMaster;
        private readonly IAuditStreamRepository<VehicleOwner> _vehicleOwner;
        private readonly IAuditStreamRepository<ANPRVehicle> _anprVehicle;
        private readonly IAuditStreamRepository<DeviceMaster> _deviceMaster;
        private readonly IAuditStreamRepository<ViMultiServerManagement> _serverManagement;
        private readonly IAuditStreamRepository<IDracMaster> _idracMaster;
        private readonly IAuditStreamRepository<SsmSiteMapping> _ssmSiteMapping;
        private readonly Logs _logs;

        public AuditManagerJob(IAuditStreamRepository<UserMaster> userAudit,
            IAuditStreamRepository<RoleMaster> roleMaster,
            IAuditStreamRepository<VehicleOwner> vehicleOwner,
            IAuditStreamRepository<ANPRVehicle> anprVehicle,
            IAuditStreamRepository<DeviceMaster> deviceMaster,
            IAuditStreamRepository<ViMultiServerManagement> serverManagement,
            IAuditStreamRepository<IDracMaster> idracMaster,
            IAuditStreamRepository<SsmSiteMapping> ssmSiteMapping,
            IOptions<Logs> logs)
        {
            _userAudit = userAudit;
            _roleMaster = roleMaster;
            _vehicleOwner = vehicleOwner;
            _anprVehicle = anprVehicle;
            _deviceMaster = deviceMaster;
            _serverManagement = serverManagement;
            _idracMaster = idracMaster;
            _ssmSiteMapping = ssmSiteMapping;
            _logs = logs.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_logs.EnableAuditLog)
            {
                _ = _userAudit.StartChangeStreamAsync(AppDBConstants.UserMaster);
                _ = _roleMaster.StartChangeStreamAsync(AppDBConstants.RoleMaster);
                _ = _vehicleOwner.StartChangeStreamAsync(AppDBConstants.VehicleOwner);
                _ = _anprVehicle.StartChangeStreamAsync(AppDBConstants.ANPRVehicle);
                _ = _deviceMaster.StartChangeStreamAsync(AppDBConstants.DeviceMaster);
                _ = _serverManagement.StartChangeStreamAsync(AppDBConstants.ViMultiServerManagement);
                _ = _idracMaster.StartChangeStreamAsync(AppDBConstants.IDracmaster);
                _ = _ssmSiteMapping.StartChangeStreamAsync(AppDBConstants.SsmSiteMapping);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        { 
        }
    }
}
