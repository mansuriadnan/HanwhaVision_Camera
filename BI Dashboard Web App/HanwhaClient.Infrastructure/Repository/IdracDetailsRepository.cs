using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using System.Text;
using System.Threading.Tasks;
using HanwhaClient.Model.Dto;

namespace HanwhaClient.Infrastructure.Repository
{
    public class IdracDetailsRepository : RepositoryBase<IdracDetails>, IIdracDetailsRepository
    {
        public IdracDetailsRepository(MongoDbConnectionService mongoDbConnectionService) : base(mongoDbConnectionService, AppDBConstants.IdracDetails)
        {

        }

        public async Task<IdracDetails> GetIdracDetails(string serverId)
        {
            var filter = Builders<IdracDetails>.Filter.And(
                    Builders<IdracDetails>.Filter.Eq(x => x.IdracServerId, serverId),
                    Builders<IdracDetails>.Filter.Eq(x => x.IsDeleted, false)
                );

            var data = await dbEntity.Find(filter).FirstOrDefaultAsync();

            return data;
        }

        public async Task<List<IdracStatusReponse>> GetIdracServerHealth(IEnumerable<string> serverId)
        {
            var filter = Builders<IdracDetails>.Filter.And(
                            Builders<IdracDetails>.Filter.In(x => x.IdracServerId, serverId),
                            Builders<IdracDetails>.Filter.Eq(x => x.IsDeleted, false));

            var data = await dbEntity
                .Find(filter)
                .Project(x => new IdracStatusReponse
                {
                    ServerId = x.IdracServerId,
                    Health = x.Health
                })
                .ToListAsync();

            return data;
        }

        public async Task UpsertIdracDetailsAsync(IdracDetails idracDetails)
        {
            var filter = MongoDB.Driver.Builders<IdracDetails>.Filter.Eq(x => x.IdracServerId, idracDetails.IdracServerId);
            
            var existingRecord = await dbEntity.Find(filter).FirstOrDefaultAsync();

            if (existingRecord != null)
            {
                idracDetails.Id = existingRecord.Id;
                idracDetails.CreatedOn = existingRecord.CreatedOn;
                idracDetails.UpdatedOn = DateTime.UtcNow;
                
                await dbEntity.ReplaceOneAsync(filter, idracDetails);
            }
            else
            {
                idracDetails.CreatedOn = DateTime.UtcNow;
                await dbEntity.InsertOneAsync(idracDetails);
            }
        }
    }
}
