using HanwhaClient.Application.Interfaces;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using MongoDB.Driver;

namespace HanwhaClient.Application.Services.Migrations.Migratation
{
    [Migration(2025011614, "Add iDRAC user during the first-time application execution")]
    public class CreateIdracUserScript2025011614 : IMigration
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUsersRepository _usersRepository;
        private readonly IExposeApiUserService _exposeApiUserService;

        public CreateIdracUserScript2025011614(IRoleRepository roleRepository, IUsersRepository usersRepository, IExposeApiUserService exposeApiUserService)
        {
            _roleRepository = roleRepository;
            _usersRepository = usersRepository;
            _exposeApiUserService = exposeApiUserService;
        }
        public async Task RunAsync(IMongoDatabase database)
        {            
            var roleScreenMappingCollection = database.GetCollection<RoleScreenMapping>(AppDBConstants.RoleScreenMapping);

            var superAdminId = await _roleRepository.GetRoleIdByRoleName("Super Admin");

            #region Insert iDREC user

            if (superAdminId != null)
            {
                var userDto = new ExposeApiUserDto
                {
                    Username = "idracadmin",
                    Password = "Admin@12",
                    Role = "idrac",
                    IsActive = true
                };

                var result = await _exposeApiUserService.AddOrUpdateUserAsync(
                    userDto,
                    superAdminId);

            }

            #endregion

        }

    }

}
