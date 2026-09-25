using HanwhaClient.Application.Interfaces;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common.ReferenceData;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using HanwhaClient.Model.SSM;
using MongoDB.Bson;
using MongoDB.Driver;


namespace HanwhaClient.Application.Services
{
    public class SSMServerManagementService : ISSMServerManagementService
    {
        public readonly ISSMServerManagementRepository _sSMServerManagementRepository;
        private readonly ISsmClientService _ssmClientService;
        private readonly ISsmServerRepository _ssmServerRepository;
        private readonly ISiteRepository _siteRepository;
        private readonly IUsersRepository _userRepository;

        public SSMServerManagementService(ISSMServerManagementRepository sSMServerManagementRepository,
            ISsmClientService ssmClientService,
            ISsmServerRepository ssmServerRepository,
            ISiteRepository siteRepository,
            IUsersRepository userRepository)
        {
            _sSMServerManagementRepository = sSMServerManagementRepository;
            _ssmClientService = ssmClientService;
            _ssmServerRepository = ssmServerRepository;
            _siteRepository = siteRepository;
            _userRepository = userRepository;
        }
        public async Task<(string Id , string ErrorMessage)> AddUpdateServerDetails(SSMServerManagementRequest request, string userId)
        {
            string httpURL = request.IsHttps ? "https://" : "http://";
            string baseUrl = httpURL + request.IPAddress + ":" + request.Port;
            bool isLogin = await _ssmClientService.LoginAsync(baseUrl, request.UserName, request.Password);
            if (!isLogin)
                return await Task.FromResult(("", $"Unable to login SSM server"));

            string ssmSiteId = "";
            
            var existing = await _sSMServerManagementRepository.GetByIpAndPortAsync(request.IPAddress,request.Port,request.Id);

            if (existing != null)
            {
                await _ssmClientService.LogoutAsync("/v2/session");
                return ("", "Server already exists with same IP and Port");
            }
            var existInSsmServer = await _ssmServerRepository.GetBySsmServerByIpAsync(request.IPAddress);
            if (existInSsmServer?.SsmSiteId != null)
            {
                var ssmSiteMapping = await _sSMServerManagementRepository
                    .GetAsync(existInSsmServer.SsmSiteId);

                bool isDifferentMapping =
                    ssmSiteMapping != null &&
                    ssmSiteMapping.Id.ToString() != request.Id;

                if (isDifferentMapping)
                {
                    await _ssmClientService.LogoutAsync("/v2/session");
                    return ("",
                        $"Server already interconnected with other server '{ssmSiteMapping.IPAddress}'.");
                }
            }
            if (string.IsNullOrEmpty(request.Id))
            {
                //var existing = await _sSMServerManagementRepository.GetByIpAndPortAsync(request.IPAddress, request.Port);
                //if (existing != null)
                //{
                //    return ("", "Server already exists with same IP and Port");
                //}

                var data = new SsmSiteMapping
                {
                    IPAddress = request.IPAddress,
                    Port = request.Port,
                    ParentSiteId = request.ParentSiteId,
                    ChildSiteId = request.ChildSiteId,
                    Username = request.UserName,
                    Password = request.Password,
                    IsHttps = request.IsHttps,
                    CreatedBy = userId,
                    UpdatedBy = userId,
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow,
                };

                ssmSiteId = await _sSMServerManagementRepository.InsertAsync(data);
            }
            else
            {
                var update = Builders<SsmSiteMapping>.Update
                .Set(c => c.IPAddress, request.IPAddress)
                .Set(c => c.Port, request.Port)
                .Set(c => c.ParentSiteId, request.ParentSiteId)
                .Set(c => c.ChildSiteId, request.ChildSiteId)
                .Set(c => c.Username, request.UserName)
                .Set(c => c.Password, request.Password)
                .Set(c => c.IsHttps, request.IsHttps)
                .Set(c => c.UpdatedOn, DateTime.UtcNow)
                .Set(c => c.UpdatedBy, userId);
                var updated = await _sSMServerManagementRepository.UpdateFieldsAsync(request.Id, update);
                ssmSiteId = request.Id;
            }
            
            // added try-catch as sometimes server added successfully in DB but below SSM APIs throws error and return 500 even server added. if below APIs fail this data will fetch by background job.
            try
            {
                var servers = await _ssmClientService.GetAsync<List<SsmServerSummary>>("/v3/metrics/servers");

                foreach (var server in servers)
                {
                    var serverDetails = await _ssmClientService.GetAsync<SsmServerDto>("/v3/metrics/servers/" + server.Guid);
                    if (serverDetails == null)
                        continue;
                    await _ssmServerRepository.AddUpdateSsmServerDetails(serverDetails, ssmSiteId);
                }
            }
            catch
            {
                await _ssmClientService.LogoutAsync("/v2/session");
            }
            await _ssmClientService.LogoutAsync("/v2/session");
            return await Task.FromResult((ssmSiteId, ""));

        }

        public async Task<(IEnumerable<SsmSiteMapping> data, Dictionary<string, object> referenceData)> GetAllSsmServersAsync()
        {
            var servers = await _sSMServerManagementRepository.GetAllAsync();

            Dictionary<string, object> referenceData = new();

            var parentSiteIds = servers.Where(x => x.ParentSiteId != null).Select(x => x.ParentSiteId).Distinct();
            var parentSiteReferenceData = await GetIDracMasterParentSiteReferenceDataAsync(parentSiteIds);
            referenceData.Add("parentSite", parentSiteReferenceData);
            
            var childSiteIds = servers.Where(x => x.ChildSiteId != null).Select(x => x.ChildSiteId).Distinct();
            var childSiteReferenceData = await GetChildSiteReferenceDataAsync(childSiteIds);
            referenceData.Add("childSite", childSiteReferenceData);

            var CreatedByIds = servers.Select(x => x.CreatedBy).Distinct().ToList();
            var createByReferenceData = await GetIDracMasterReferenceDataAsync(CreatedByIds);
            referenceData.Add("createdBy", createByReferenceData);

            var UpdatedByIds = servers.Select(x => x.UpdatedBy).Distinct().ToList();
            var UpdateByReferenceData = await GetIDracMasterReferenceDataAsync(UpdatedByIds);
            referenceData.Add("updatedBy", UpdateByReferenceData);

            return await Task.FromResult((servers, referenceData));
        }

        public async Task<List<OptionModel<string, string>>> GetIDracMasterReferenceDataAsync(IEnumerable<string> ids)
        {
            var options = new List<OptionModel<string, string>>();
            ProjectionDefinition<UserMaster> projection = Builders<UserMaster>.Projection
            .Include("Username")
            .Include("_id");
            var users = await _userRepository.GetManyAsync(ids, projection);
            options = users.Select(x => new OptionModel<string, string>(x.Id, x.Username)).ToList();
            return options;
        }

        public async Task<List<OptionModel<string, string>>> GetIDracMasterParentSiteReferenceDataAsync(IEnumerable<string> ids)
        {
            var options = new List<OptionModel<string, string>>();
            ProjectionDefinition<SiteMaster> projection = Builders<SiteMaster>.Projection
            .Include("siteName")
            .Include("_id");
            var sites = await _siteRepository.GetManyAsync(ids, projection);
            options = sites.Select(x => new OptionModel<string, string>(x.Id, x.SiteName)).ToList();
            return options;
        }

        public async Task<List<OptionModel<string, string>>> GetChildSiteReferenceDataAsync(IEnumerable<string> ids)
        {
            var objectIds = ids.Select(id => (id)).ToList();

            var filter = Builders<SiteMaster>.Filter.ElemMatch(
                x => x.ChildSites,
                c => objectIds.Contains(c.Id)
            );

            var projection = Builders<SiteMaster>.Projection.Include("childSites");

            var sites = await _siteRepository.GetByFilterAsync(filter, projection);

            return sites
                .SelectMany(x => x.ChildSites ?? new List<ChildSite>())
                .Where(c => objectIds.Contains(c.Id))
                .Select(c => new OptionModel<string, string>(
                    c.Id.ToString(),
                    c.SiteName
                ))
                .DistinctBy(x => x.label)
                .ToList();
        }

        public async Task<bool> DeleteSsmServerManagement(DeleteSsmServerRequest request, string userId)
        {
            var result = await _sSMServerManagementRepository.SoftDeleteAsync(request.Id, userId);
            if (result)
            {
                return await _ssmServerRepository.SoftDeleteSsmServerAsync(request.Id, userId);
            }
            return result;
        }
        
    }
}
