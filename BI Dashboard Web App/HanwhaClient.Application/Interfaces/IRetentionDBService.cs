using HanwhaClient.Model.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Interfaces
{
    public interface IRetentionDBService
    {
        Task<bool> InsertOrDeleteRetentionData(int retentionPeriod);
    }
}
