using DocumentFormat.OpenXml.Drawing.Diagrams;
using HanwhaClient.Application.Interfaces;
using HanwhaClient.Infrastructure.Connection;
using HanwhaClient.Model.Common;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Services
{
    public class MongoService : IMongoService
    {
        private readonly MongoSettings _config;
        private readonly IConfiguration _configuration;
        public MongoService(MongoSettings config, IConfiguration configuration)
        {
            _config = config;
            _configuration = configuration;
        }
        public async Task<(bool IsSuccess, string OutputPath, string Error)> BackupDatabaseAsync(string rootBackupPath = null)
        {
            string backupFolder = null;
            try
            {
                var sw1 = Stopwatch.StartNew();
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                // mongodump process

                var envConnectionString = Environment.GetEnvironmentVariable("VisionInsightMongoConn");
                string host, port, connectionString, databaseName = "";

                if (!string.IsNullOrWhiteSpace(envConnectionString))
                {
                    Uri uri = new Uri(envConnectionString);
                    connectionString = $"{uri.Scheme}://{uri.Host}:{uri.Port}";
                    databaseName = uri.AbsolutePath.Trim('/') == "" ? "visioninsightBIDashboard" : uri.AbsolutePath.Trim('/');
                }
                else
                {
                    // Fixed: Use proper IConfiguration methods instead of GetValue
                    host = _configuration["ConnectionStrings:Host"] ?? "127.0.0.1";
                    port = _configuration["ConnectionStrings:Port"] ?? "27017";

                    connectionString = ($"mongodb://{host}:{port}");

                    databaseName = _configuration["ConnectionStrings:DatabaseName"] ?? "visioninsightBIDashboard";
                }

                if (rootBackupPath != null)
                {

                    backupFolder = Path.Combine(rootBackupPath, DateTime.Now.Date.ToString("dd-MM-yyyy"), DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + databaseName);
                }
                else
                {
                    backupFolder = Path.Combine("BackupFile", DateTime.Now.Date.ToString("dd-MM-yyyy"), DateTime.Now.ToString("yyyyMMdd_HHmmss") + "_" + databaseName);
                }

                if (!Directory.Exists(backupFolder))
                {
                    Directory.CreateDirectory(backupFolder);
                }

                var isDatabaseIfExist = await CheckDatabaseIfExist(databaseName, connectionString);
                if (!isDatabaseIfExist)
                {
                    return (false, null, "Database doesn't exist");
                }

                var mongodump = new ProcessStartInfo
                {
                    FileName = _config.MongoDumpPath,
                    // Arguments = $"--uri=\"{_config.ConnectionString}\" --db={_config.DatabaseName} --out=\"{backupFolder}\" --gzip --numParallelCollections={Environment.ProcessorCount * 2} --quiet --readPreference=secondaryPreferred",
                    Arguments = $"--uri=\"{connectionString}\" --db={databaseName} --out=\"{backupFolder}\"  --gzip --numParallelCollections={Math.Min(Environment.ProcessorCount * 4, 16)} --quiet --readPreference=secondaryPreferred --excludeCollection=system.indexes --forceTableScan",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(mongodump);
                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();
                await Task.WhenAll(outputTask, errorTask);

                sw1.Stop();
                Console.WriteLine($"Backup: {sw1.Elapsed.TotalSeconds:F1}s");
                var error = await errorTask;
                await process.WaitForExitAsync();

                if (process.ExitCode == 0)
                {
                    var sw2 = Stopwatch.StartNew();
                    await CompressBackupAsync(backupFolder);
                    sw2.Stop();
                    Console.WriteLine($"Compression: {sw2.Elapsed.TotalSeconds:F1}s");

                    var sw3 = Stopwatch.StartNew();
                    await EncryptBackupAsync(backupFolder);
                    sw3.Stop();
                    Console.WriteLine($"Encryption: {sw3.Elapsed.TotalSeconds:F1}s");

                    string encryptedFilePath = backupFolder + ".zip.aes";
                    stopwatch.Stop();
                    Console.WriteLine($"Main Mathod Ended: {stopwatch.Elapsed.TotalSeconds:F1}s");
                    return (true, encryptedFilePath, null);
                }
                else
                {
                    return (false, null, error);
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //return (false, null, ex.Message);
            }
            finally
            {
                // Cleanup backup folder in case of any failure
                if (!string.IsNullOrEmpty(backupFolder) && Directory.Exists(backupFolder))
                {

                    await Task.Run(() => Directory.Delete(backupFolder, true));
                }

                if (!string.IsNullOrEmpty(backupFolder + ".zip") && File.Exists(backupFolder + ".zip"))
                {

                    await Task.Run(() => File.Delete(backupFolder + ".zip"));
                }
                CleanupOldBackupFolders(rootBackupPath);
            }
        }


        private static async Task CompressBackupAsync(string folderPath)
        {
            string zipFilePath = folderPath + ".zip";
            await Task.Run(() => ZipFile.CreateFromDirectory(folderPath, zipFilePath, CompressionLevel.Fastest, false));
        }

        private async Task EncryptBackupAsync(string folderPath)
        {
            string zipFilePath = folderPath + ".zip";
            string encryptedFilePath = zipFilePath + ".aes";

            using var inputStream = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4 * 1024 * 1024); // 4MB buffer
            using var fileStream = new FileStream(encryptedFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4 * 1024 * 1024);
            using var aes = Aes.Create();

            byte[] key = Encoding.UTF8.GetBytes(_config.EncryptionKey);
            byte[] iv = new byte[16];
            aes.Key = key;
            aes.IV = iv;

            using var cryptoStream = new CryptoStream(fileStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            await inputStream.CopyToAsync(cryptoStream, bufferSize: 4 * 1024 * 1024);

            // Cleanup intermediate files
            //await CleanupTempFilesAsync(folderPath, zipFilePath);
        }


        private static async Task CleanupTempFilesAsync(string backupFolder, string zipFilePath)
        {
            try
            {
                // Delete zip file if it exists
                if (!string.IsNullOrEmpty(zipFilePath) && File.Exists(zipFilePath))
                {
                    await Task.Run(() => File.Delete(zipFilePath));
                    Console.WriteLine($"Temporary zip file '{zipFilePath}' deleted successfully.");
                }

                //// Delete backup folder if it exists
                //if (!string.IsNullOrEmpty(backupFolder) && Directory.Exists(backupFolder))
                //{
                //    await Task.Run(() => Directory.Delete(backupFolder, true));
                //    Console.WriteLine($"Backup folder '{backupFolder}' deleted successfully.");
                //}
            }
            catch (Exception ex)
            {
                throw ex;
                //Console.WriteLine($"Warning: Failed to cleanup temporary files: {ex.Message}");
                // Don't throw - cleanup failures shouldn't fail the backup operation
            }
        }

        private void CleanupOldBackupFolders(string rootBackupPath, int days = 7)
        {
            if (string.IsNullOrWhiteSpace(rootBackupPath))
                return;

            if (!Directory.Exists(rootBackupPath))
                return;

            var folders = Directory.GetDirectories(rootBackupPath);

            foreach (var folder in folders)
            {
                try
                {
                    string folderName = Path.GetFileName(folder);

                    // Folder name format: dd-MM-yyyy
                    if (!DateTime.TryParseExact(folderName, "dd-MM-yyyy",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out DateTime folderDate))
                    {
                        // Skip folders not matching format
                        continue;
                    }

                    // If folder date is older than X days → delete
                    if (folderDate < DateTime.Now.AddDays(-days))
                    {
                        Directory.Delete(folder, true); // delete folder + files
                    }
                }
                catch
                {
                    // log if needed, but avoid job crash
                }
            }
        }


        private async Task<(bool Success, string Error)> MonitorBackupProcess(Process process)
        {
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await Task.WhenAll(outputTask, errorTask);
            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                return (true, null);
            }
            else
            {
                var error = await errorTask;
                return (false, error);
            }
        }

        private async Task<string> WaitAndPostProcess(string backupFolder, Task<(bool Success, string Error)> backupTask)
        {
            // Wait for backup to complete
            await backupTask;

            // Start compression and encryption in parallel if possible
            await CompressBackupAsync(backupFolder);
            await EncryptBackupAsync(backupFolder);

            return backupFolder + ".zip.aes";
        }

        // Instead of ZipFile.CreateFromDirectory
        private static async Task CompressBackupAsync1(string folderPath)
        {
            string zipFilePath = folderPath + ".zip";
            await Task.Run(() => ZipFile.CreateFromDirectory(folderPath, zipFilePath, CompressionLevel.Fastest, false));
        }

        // Make encryption async
        private async Task EncryptBackupAsync1(string folderPath)
        {
            string zipFilePath = folderPath + ".zip";
            string encryptedFilePath = zipFilePath + ".aes";

            using var inputStream = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 1024 * 1024); // 1MB buffer
            using var fileStream = new FileStream(encryptedFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 1024 * 1024);
            using var aes = Aes.Create();

            byte[] key = Encoding.UTF8.GetBytes(_config.EncryptionKey);
            byte[] iv = new byte[16];
            aes.Key = key;
            aes.IV = iv;

            using var cryptoStream = new CryptoStream(fileStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            await inputStream.CopyToAsync(cryptoStream, bufferSize: 1024 * 1024);
        }

        private async Task<bool> CheckDatabaseIfExist(string dbName, string connectionString)
        {
            var client = new MongoClient(connectionString); // Modify with your MongoDB connection string
            var database = client.GetDatabase(dbName);

            var dbList = await client.ListDatabaseNamesAsync();
            var dbNames = dbList.ToList();

            if (dbNames.Contains(dbName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task RestoreDatabase(string encryptedFilePath)
        {
            var stopwatch = Stopwatch.StartNew();
            Console.WriteLine($"Compression: {stopwatch.Elapsed.TotalSeconds:F1}s");
            string _backupDirectory = null;
            string compressedFilePath = null;

            try
            {

                // Step 1: Decrypt the file (async)
                var sw1 = Stopwatch.StartNew();
                compressedFilePath = await DecryptBackupAsync(encryptedFilePath);
                sw1.Stop();
                Console.WriteLine($"Compression : {sw1.Elapsed.TotalSeconds:F1}s");

                // Step 2: Create temp restore directory
                var sw2 = Stopwatch.StartNew();
                _backupDirectory = Path.Combine(Path.GetTempPath(), "MongoRestore_" + Guid.NewGuid());
                Directory.CreateDirectory(_backupDirectory);
                sw2.Stop();
                Console.WriteLine($"Create temp restore directory : {sw2.Elapsed.TotalSeconds:F1}s");


                // Step 3: Unzip the backup (async)
                var sw3 = Stopwatch.StartNew();
                await UnzipBackupAsync(compressedFilePath, _backupDirectory);
                Directory.CreateDirectory(_backupDirectory);
                sw3.Stop();
                Console.WriteLine($"Unzip the backup : {sw3.Elapsed.TotalSeconds:F1}s");

                var envConnectionString = Environment.GetEnvironmentVariable("VisionInsightMongoConn");
                string host, port, connectionString, databaseName = "";

                if (!string.IsNullOrWhiteSpace(envConnectionString))
                {
                    Uri uri = new Uri(envConnectionString);
                    connectionString = $"{uri.Scheme}://{uri.Host}:{uri.Port}";
                    databaseName = uri.AbsolutePath.Trim('/') == "" ? "visioninsightBIDashboard" : uri.AbsolutePath.Trim('/');
                }
                else
                {
                    host = _configuration["ConnectionStrings:Host"] ?? "127.0.0.1";
                    port = _configuration["ConnectionStrings:Port"] ?? "27017";
                    connectionString = ($"mongodb://{host}:{port}");
                    databaseName = _configuration["ConnectionStrings:DatabaseName"] ?? "visioninsightBIDashboard";
                }


                // Step 4: Restore (already async)
                var sw4 = Stopwatch.StartNew();
                await RestoreDatabaseAsync(connectionString, databaseName, _backupDirectory, compressedFilePath);
                Directory.CreateDirectory(_backupDirectory);

                sw4.Stop();
                Console.WriteLine($"Restore : {sw4.Elapsed.TotalSeconds:F1}s");


                stopwatch.Stop();
                Console.WriteLine($"Restore took {stopwatch.Elapsed.TotalMinutes:F2} minutes.");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                // Cleanup in finally block
                await CleanupRestoreFilesAsync(_backupDirectory, compressedFilePath);
            }
        }


        private async Task<string> DecryptBackupAsync(string encryptedFilePath)
        {
            string decryptedFilePath = encryptedFilePath.Replace(".aes", "");
            byte[] key = Encoding.UTF8.GetBytes(_config.EncryptionKey);
            byte[] iv = new byte[16];

            using var aesAlg = Aes.Create();
            aesAlg.Key = key;
            aesAlg.IV = iv;

            using var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            using var encryptedStream = new FileStream(encryptedFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4 * 1024 * 1024);
            using var cryptoStream = new CryptoStream(encryptedStream, decryptor, CryptoStreamMode.Read);
            using var decryptedStream = new FileStream(decryptedFilePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4 * 1024 * 1024);

            await cryptoStream.CopyToAsync(decryptedStream, bufferSize: 4 * 1024 * 1024);

            Console.WriteLine($"Decryption completed: {decryptedFilePath}");
            return decryptedFilePath;
        }


        private static async Task UnzipBackupAsync(string zipFilePath, string extractPath)
        {
            await Task.Run(() => ZipFile.ExtractToDirectory(zipFilePath, extractPath));
            Console.WriteLine($"Backup unzipped successfully to {extractPath}");
        }

        private async Task RestoreDatabaseAsync(string connectionString, string _dbName, string _backupDirectory, string compressedFilePath)
        {
            var mongorestore = new ProcessStartInfo
            {
                FileName = _config.MongoRestorePath,
                Arguments = $"--uri=\"{connectionString}\" --db={_dbName} --dir=\"{_backupDirectory}\\{_dbName}\" --drop --gzip --numParallelCollections={Environment.ProcessorCount * 2} --numInsertionWorkersPerCollection=8 --quiet --stopOnError",
                //    Arguments = $"--uri=\"{_config.ConnectionString}\" " +
                //$"--db={_dbName} " +
                //$"--dir=\"{_backupDirectory}\\{_dbName}\" " +
                //"--drop --gzip " +
                //$"--numParallelCollections={Environment.ProcessorCount * 2} " +
                //"--numInsertionWorkersPerCollection=8 " +
                //"--writeConcern={{w:0,j:false}} " + // keep JSON brackets escaped
                //"--quiet --stopOnError",
                //Arguments = $"--uri=\"{_config.ConnectionString}\" --db={_dbName} --dir=\"{_backupDirectory}\\{_dbName}\" --drop --gzip --numParallelCollections={Environment.ProcessorCount * 2} --numInsertionWorkersPerCollection=8 --writeConcern={{w:0,j:false}} --quiet --stopOnError --maintainInsertionOrder=false",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = mongorestore };
            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await Task.WhenAll(outputTask, errorTask);
            await process.WaitForExitAsync();

            var error = await errorTask;

            if (process.ExitCode == 0)
            {
                Console.WriteLine("Restore completed successfully.");
            }
            else
            {
                Console.WriteLine($"Restore failed: {error}");
                throw new Exception($"Restore failed: {error}");
            }
        }

        private static async Task CleanupRestoreFilesAsync(string backupDirectory, string compressedFilePath)
        {
            var cleanupTasks = new List<Task>();

            if (Directory.Exists("BackupFile"))
            {
                Directory.Delete("BackupFile", true);
            }

            // Delete backup directory
            if (!string.IsNullOrEmpty(backupDirectory) && Directory.Exists(backupDirectory))
            {
                cleanupTasks.Add(Task.Run(() =>
                {
                    try
                    {
                        Directory.Delete(backupDirectory, true);
                        Console.WriteLine("Backup directory deleted successfully.");
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }));
            }

            // Delete compressed file
            if (Directory.Exists("uploads"))
            {
                cleanupTasks.Add(Task.Run(() =>
                {
                    try
                    {
                        Directory.Delete("uploads", true);
                        Console.WriteLine("Compressed file deleted successfully.");
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }));
            }

            if (cleanupTasks.Any())
            {
                await Task.WhenAll(cleanupTasks);
            }
        }

    }
}
