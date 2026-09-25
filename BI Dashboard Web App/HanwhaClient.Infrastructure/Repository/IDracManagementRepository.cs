using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Infrastructure.Repository
{
    public class IDracManagementRepository : RepositoryBase<IDracMaster>, IIDracManagementRepository
    {
        public IDracManagementRepository(MongoDbConnectionService mongoDbConnectionService) : base(mongoDbConnectionService, AppDBConstants.IDracmaster)
        {
        }

        public async Task<List<IdracServerListDashboardResponse>> GetServersByParentSiteIdAsync(string parentSiteId)
        {
            var data = await dbEntity
                .Find(x => x.ParentSiteId != null && x.ParentSiteId == parentSiteId && x.IsDeleted != true)
                .ToListAsync();

            return data.Select(x => new IdracServerListDashboardResponse
            {
                Id = x.Id,
                ServerName = x.ServerName,
                ParentSiteId = x.ParentSiteId,
                ChildSiteId = x.ChildSiteId,
                IPAddress = x.IPAddress,
                Username = x.UserName,
                Password = x.Password,
                Port = x.Port
            }).ToList();
        }
        public async Task<bool> AddAlarmAsync(string serverId, AlarmDetails alarm)
        {
            var update = Builders<IDracMaster>.Update
                .Push(x => x.Alarms, alarm);

            var result = await dbEntity.UpdateOneAsync(
                x => x.Id == serverId,
                update);

            return result.ModifiedCount > 0;
        }
        public async Task<bool> UpdateAlarmSubscriptionIdAsync(string serverId, string alarmId, string subscriptionId)
        {
            var update = Builders<IDracMaster>.Update
            .Set(x => x.EventSubscriptionId, subscriptionId);

            var result = await dbEntity.UpdateOneAsync(
                x => x.Id == serverId,
                update);

            return result.ModifiedCount > 0;
        }
        public async Task<List<AlarmEventDetails>> GetEventsByIpAddressAsync(string ipAddress)
        {
            var result = await dbEntity
                .Find(x =>
                    x.IPAddress != null &&
                    x.IPAddress == ipAddress &&
                    x.IsDeleted == false)
                .Project(x => x.Alarms!
                    .Select(a => new AlarmEventDetails
                    {
                        Event = a.Event,
                        TimeLimit = a.TimeLimit
                    })
                    .ToList())
                .FirstOrDefaultAsync();

            return result ?? new List<AlarmEventDetails>();
        }    

        public async Task RemoveAlarmAsync(string serverId, string alarmId)
        {
            //var update =
            //    Builders<IDracMaster>.Update.PullFilter(
            //        x => x.Alarms,
            //        a => a.AlarmId == alarmId);

            //await dbEntity.UpdateOneAsync(
            //    x => x.Id == serverId,
            //    update);

            var filter =
            Builders<IDracMaster>.Filter.Eq(
            x => x.Id,
            serverId);

            var update =
                Builders<IDracMaster>.Update.PullFilter(
                    "alarms",
                    Builders<AlarmDetails>.Filter.Eq(
                        x => x.AlarmId,
                        alarmId));

            var result =
                await dbEntity.UpdateOneAsync(
                    filter,
                    update);

        }

        public async Task UpdateIdracLedIndicatorAsync(string serverId, bool ledState)
        {
            var update =
                Builders<IDracMaster>.Update
                    .Set(x => x.LedState, ledState);

            await dbEntity.UpdateOneAsync(
                x => x.Id == serverId,
                update);
        }

        public async Task<(string? UserName, string? Password)>GetCredentialsByIpAddressAsync(string ipAddress)
        {
            var data = await dbEntity
                .Find(x => x.IPAddress == ipAddress && x.IsDeleted == false)
                .Project(x => new
                {
                    x.UserName,
                    x.Password
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return (null, null);
            }

            return (data.UserName, data.Password);
        }
        public async Task UpdatePowerStateAsync(string serverId, string powerState)
        {
            var update =
                Builders<IDracMaster>.Update
                    .Set(x => x.PowerState, powerState);

            await dbEntity.UpdateOneAsync(
                x => x.Id == serverId,
                update);
        }
        public async Task ClearEventSubscriptionIdAsync(string serverId)
        {
            var update =
                Builders<IDracMaster>.Update
                    .Set(x => x.EventSubscriptionId, null);

            await dbEntity.UpdateOneAsync(
                x => x.Id == serverId,
                update);
        }
        public async Task<List<IDracMaster>> GetServersByIpAsync(string ipAddress)
        {
            var data = await dbEntity
                .Find(x => x.IPAddress == ipAddress && x.IsDeleted != true)
                .ToListAsync();
            return data;
        }
        public async Task<bool> IsIpExistsAsync(string ipAddress, string? excludeId = null)
        {
            var filter = Builders<IDracMaster>.Filter.And(
                Builders<IDracMaster>.Filter.Eq(x => x.IPAddress, ipAddress),
                Builders<IDracMaster>.Filter.Eq(x => x.IsDeleted, false)
             );


            if (!string.IsNullOrEmpty(excludeId))
            {
                filter &= Builders<IDracMaster>.Filter.Ne(x => x.Id, excludeId);
            }

            return await dbEntity.Find(filter).AnyAsync();
        }
    }
}
