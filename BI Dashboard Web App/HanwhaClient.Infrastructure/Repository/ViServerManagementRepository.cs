using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HanwhaClient.Infrastructure.Repository
{
    public class ViServerManagementRepository : RepositoryBase<ViMultiServerManagement>, IViServerManagementRepository
    {
        public ViServerManagementRepository(MongoDbConnectionService mongoDbConnectionService) : base(mongoDbConnectionService, AppDBConstants.ViMultiServerManagement)
        {
        }

        public async Task<bool> CheckMultiServerExists(ViMultiServerManagementDTO serverManagementRequest)
        {
            var filter = Builders<ViMultiServerManagement>.Filter.And(
                Builders<ViMultiServerManagement>.Filter.Eq(x => x.IsDeleted, false),
                Builders<ViMultiServerManagement>.Filter.Or(
                    Builders<ViMultiServerManagement>.Filter.Eq(x => x.HostingAddress, serverManagementRequest.HostingAddress),
                    Builders<ViMultiServerManagement>.Filter.Eq(x => x.ServerName, serverManagementRequest.ServerName)));

            var data = await dbEntity.Find(filter).ToListAsync();

            return data.Count > 0;
                
        }

        public async Task<IEnumerable<ViMultiServerManagement>> GetAllActiveServer()
        {
            var filter = Builders<ViMultiServerManagement>.Filter.And(
                Builders<ViMultiServerManagement>.Filter.Eq(x => x.IsDeleted, false),
                Builders<ViMultiServerManagement>.Filter.Eq(x => x.IsActive, true),
                Builders<ViMultiServerManagement>.Filter.Eq(x => x.IsAvailable, true));

            return await dbEntity.Find(filter).ToListAsync();
        }

        public string GetApplicationConnectionString()
        {

            string host = dbEntity.Database.Client.Settings.Server.Host;
            
            if(host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            {
                host = Dns.GetHostEntry(Dns.GetHostName())
                          .AddressList
                          .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?
                          .ToString() ?? host;
            }

            string connectionString = "mongodb://" + host + ":" + dbEntity.Database.Client.Settings.Server.Port + "/" + dbEntity.Database.DatabaseNamespace.DatabaseName;
            return connectionString;
        }
    }
}
