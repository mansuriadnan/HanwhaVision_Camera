using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Interfaces
{
    public interface IMongoService
    {
        Task<(bool IsSuccess, string OutputPath, string Error)> BackupDatabaseAsync(string targetPath = null);
        Task RestoreDatabase(string encryptedFilePath);

    }
}
