using HanwhaClient.Core.Interfaces;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using Microsoft.Web.Administration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Services.Migrations.Migratation
{
    [Migration(2025011610, "Add master data for Country.")]
    public class AddCountryMasterData2025011609 : IMigration
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUsersRepository _usersRepository;

        public AddCountryMasterData2025011609(IRoleRepository roleRepository,
            IUsersRepository usersRepository)
        {
            _roleRepository = roleRepository;
            _usersRepository = usersRepository;
        }

        public async Task RunAsync(IMongoDatabase database)
        {
            var countryCollection = database.GetCollection<Country>(AppDBConstants.Country);
            var userCollection = database.GetCollection<UserMaster>(AppDBConstants.UserMaster);
            var systemAdminFilter = Builders<UserMaster>.Filter.Eq(u => u.Username, "sysadmin");
            var existingSystemAdmin = await userCollection.Find(systemAdminFilter).FirstOrDefaultAsync();


            // Sample data (replace with your actual data)
            var countries = new List<CountryInfoModel>
            {
                new CountryInfoModel { CountryName = "United Arab Emirates", CountryCode = "AE", HasState = false },
                new CountryInfoModel { CountryName = "KSA", CountryCode = "SA", HasState = false },
                new CountryInfoModel { CountryName = "EGYPT", CountryCode = "EG", HasState = false },
                new CountryInfoModel { CountryName = "Israel", CountryCode = "IL", HasState = false },
                new CountryInfoModel { CountryName = "Iraq", CountryCode = "IQ", HasState = false },
                new CountryInfoModel { CountryName = "Oman", CountryCode = "OM", HasState = false },
                new CountryInfoModel { CountryName = "Kuwait", CountryCode = "KW", HasState = false },
                new CountryInfoModel { CountryName = "Jordan", CountryCode = "JO", HasState = false },
                new CountryInfoModel { CountryName = "Morocco", CountryCode = "MA", HasState = false },
                new CountryInfoModel { CountryName = "South Africa", CountryCode = "ZA", HasState = true },
                new CountryInfoModel { CountryName = "Kenya", CountryCode = "KE", HasState = false },
                new CountryInfoModel { CountryName = "Tunisia", CountryCode = "TN", HasState = false },
                new CountryInfoModel { CountryName = "Algeria", CountryCode = "DZ", HasState = false },
                new CountryInfoModel { CountryName = "France", CountryCode = "FR", HasState = false },
                new CountryInfoModel { CountryName = "Germany", CountryCode = "DE", HasState = true },
                new CountryInfoModel { CountryName = "United Kingdom", CountryCode = "GB", HasState = false },
                new CountryInfoModel { CountryName = "Netherlands", CountryCode = "NL", HasState = false },
                new CountryInfoModel { CountryName = "Italy", CountryCode = "IT", HasState = false },
                new CountryInfoModel { CountryName = "Czech Republic", CountryCode = "CZ", HasState = false },
                new CountryInfoModel { CountryName = "Austria", CountryCode = "AT", HasState = true },
                new CountryInfoModel { CountryName = "Belgium", CountryCode = "BE", HasState = false },
                new CountryInfoModel { CountryName = "Spain", CountryCode = "ES", HasState = false },
                new CountryInfoModel { CountryName = "Portugal", CountryCode = "PT", HasState = false },
                new CountryInfoModel { CountryName = "Denmark", CountryCode = "DK", HasState = false },
                new CountryInfoModel { CountryName = "Poland", CountryCode = "PL", HasState = false },
                new CountryInfoModel { CountryName = "Sweden", CountryCode = "SE", HasState = false },
                new CountryInfoModel { CountryName = "Norway", CountryCode = "NO", HasState = false },
                new CountryInfoModel { CountryName = "Ireland", CountryCode = "IE", HasState = false },
                new CountryInfoModel { CountryName = "Iceland", CountryCode = "IS", HasState = false },
                new CountryInfoModel { CountryName = "Hungary", CountryCode = "HU", HasState = false },
                new CountryInfoModel { CountryName = "Romania", CountryCode = "RO", HasState = false },
                new CountryInfoModel { CountryName = "Finland", CountryCode = "FI", HasState = false },
                new CountryInfoModel { CountryName = "India", CountryCode = "IN", HasState = true },
                new CountryInfoModel { CountryName = "Singapore", CountryCode = "SG", HasState = false },
                new CountryInfoModel { CountryName = "South Korea", CountryCode = "KR", HasState = false },
                new CountryInfoModel { CountryName = "Japan", CountryCode = "JP", HasState = false },
                new CountryInfoModel { CountryName = "Australia", CountryCode = "AU", HasState = true },
                new CountryInfoModel { CountryName = "New Zealand", CountryCode = "NZ", HasState = false },
                new CountryInfoModel { CountryName = "United States of America", CountryCode = "US", HasState = true },
                new CountryInfoModel { CountryName = "Mexico", CountryCode = "MX", HasState = true }
            };

            var countyList = countries.Select(x => new Country
            {
                Name = x.CountryName,
                Code = x.CountryCode,
                HasStates = x.HasState,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = existingSystemAdmin.Id,
                UpdatedOn = DateTime.UtcNow,
                UpdatedBy = existingSystemAdmin.Id,
            });

            await countryCollection.InsertManyAsync(countyList);
        }
    }
}
