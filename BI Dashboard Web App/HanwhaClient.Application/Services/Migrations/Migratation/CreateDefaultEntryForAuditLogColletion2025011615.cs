using HanwhaClient.Core.Interfaces;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace HanwhaClient.Application.Services.Migrations.Migratation
{
    [Migration(2025011615, "Create and delete entry in audit log related table")]
    public class CreateDefaultEntryForAuditLogColletion2025011615 : IMigration
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUsersRepository _userRepository;
        private readonly IVehicleOwnerRepository _vehicleOwnerRepository;
        private readonly IANPRVehicleRepository _anprVehicleRepository;
        private readonly IDeviceMasterRepository _deviceMasterRepository;
        private readonly IViServerManagementRepository _serverManagementRepository;
        private readonly IIDracManagementRepository _idracRepository;
        private readonly ISSMServerManagementRepository _ssmSiteMappingRepository;

        public CreateDefaultEntryForAuditLogColletion2025011615(IRoleRepository roleRepository, 
            IUsersRepository userRepository,
            IVehicleOwnerRepository vehicleOwnerRepository,
            IANPRVehicleRepository anprVehicleRepository,
            IDeviceMasterRepository deviceMasterRepository,
            IViServerManagementRepository serverManagementRepository,
            IIDracManagementRepository idracRepository,
            ISSMServerManagementRepository ssmSiteMappingRepository)
        {
            _roleRepository = roleRepository;
            _userRepository = userRepository;
            _vehicleOwnerRepository = vehicleOwnerRepository;
            _anprVehicleRepository = anprVehicleRepository;
            _deviceMasterRepository = deviceMasterRepository;
            _serverManagementRepository = serverManagementRepository;
            _idracRepository = idracRepository;
            _ssmSiteMappingRepository = ssmSiteMappingRepository;
        }

        public async Task RunAsync(IMongoDatabase database)
        {                                
            await CreateDeleteDocAsync();
        }
        private async Task CreateDeleteDocAsync()
        {   
            var user = new UserMaster();
            await _userRepository.InsertAsync(user);
            await _userRepository.DeleteAsync(user.Id);

            //RoleMaster
            var role = new RoleMaster();
            await _roleRepository.InsertAsync(role);
            await _roleRepository.DeleteAsync(role.Id);

            // VehicleOwner
            var vehicleOwner = new VehicleOwner();
            await _vehicleOwnerRepository.InsertAsync(vehicleOwner);
            await _vehicleOwnerRepository.DeleteAsync(vehicleOwner.Id);

            // AnprVehicle
            var anprVehicle = new ANPRVehicle();
            await _anprVehicleRepository.InsertAsync(anprVehicle);
            await _anprVehicleRepository.DeleteAsync(anprVehicle.Id);

            // DeviceMaster
            var deviceMaster = new DeviceMaster();
            await _deviceMasterRepository.InsertAsync(deviceMaster);
            await _deviceMasterRepository.DeleteAsync(deviceMaster.Id);

            // MultiServer
            var viMultiServerManagement = new ViMultiServerManagement();
            await _serverManagementRepository.InsertAsync(viMultiServerManagement);
            await _serverManagementRepository.DeleteAsync(viMultiServerManagement.Id);

            // IdracMaster
            var idracMaster = new IDracMaster();
            await _idracRepository.InsertAsync(idracMaster);
            await _idracRepository.DeleteAsync(idracMaster.Id);

            // SsmServer
            var ssmSiteMapping = new SsmSiteMapping();
            await _ssmSiteMappingRepository.InsertAsync(ssmSiteMapping);
            await _ssmSiteMappingRepository.DeleteAsync(ssmSiteMapping.Id);
        }
    }
}
