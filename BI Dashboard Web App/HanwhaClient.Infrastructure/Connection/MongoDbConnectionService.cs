using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using System;

namespace HanwhaClient.Infrastructure.Connection
{
    public class MongoDbConnectionService
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoDatabase _database;
        private List<IMongoDatabase> _linkedDatabase;
        private readonly ILogger<MongoDbConnectionService>? _logger;
        private readonly IServiceProvider _serviceProvider;


        public IMongoDatabase? Database => _database;
        public List<IMongoDatabase>? LinkedDatabase => _linkedDatabase;

        public MongoDbConnectionService(IConfiguration configuration, ILogger<MongoDbConnectionService>? logger = null, IServiceProvider serviceProvider = null)
        {
            this._configuration = configuration;
            this._logger = logger;
            _serviceProvider = serviceProvider;

            try
            {
                // 1️⃣ Check environment variable first
                var envConnectionString = Environment.GetEnvironmentVariable("VisionInsightMongoConn");

                MongoUrl mongoUrl;

                if (!string.IsNullOrWhiteSpace(envConnectionString))
                {
                    _logger?.LogInformation("Using MongoDB connection string from environment variable.");
                    mongoUrl = MongoUrl.Create(envConnectionString);
                }
                else
                {
                    // Fixed: Use proper IConfiguration methods instead of GetValue
                    var host = _configuration["ConnectionStrings:Host"] ?? "127.0.0.1";
                    var port = _configuration["ConnectionStrings:Port"] ?? "27017";
                    var databaseName = _configuration["ConnectionStrings:DatabaseName"] ?? "visioninsightBIDashboard";
                    var authentication = _configuration["ConnectionStrings:Authentication"] ?? "false";
                    if (authentication == "true")
                    {
                        mongoUrl = MongoCredentials.BuildMongoUrl(host, port, databaseName);
                    }
                    else
                    {
                        mongoUrl = MongoUrl.Create($"mongodb://{host}:{port}/{databaseName}");
                    }

                    _logger?.LogInformation("Connecting to MongoDB at {Host}:{Port} with database {DatabaseName}", host, port, databaseName);
                }

                // Build secure connection string using credentials from C# class

                //return $"mongodb://{username}:{password}@{host}:{port}/{databaseName}";
                var mongoClient = new MongoClient(mongoUrl);
                _database = mongoClient.GetDatabase(mongoUrl.DatabaseName ?? "visioninsightBIDashboard");

                // Test the connection
                _database.RunCommand<MongoDB.Bson.BsonDocument>(new MongoDB.Bson.BsonDocument("ping", 1));
                _logger?.LogInformation("Successfully connected to MongoDB database: {DatabaseName}", mongoUrl.DatabaseName);
                InitializeLinkedServers().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to connect to MongoDB");
                //throw new InvalidOperationException("Failed to establish MongoDB connection", ex);
            }

        }

        public async Task InitializeLinkedServers()
        {
            try
            {
                _linkedDatabase = [];
                var serverManagementsDBinstanse = Database?.GetCollection<ViMultiServerManagement>(AppDBConstants.ViMultiServerManagement);
                FilterDefinition<ViMultiServerManagement> filter = Builders<ViMultiServerManagement>.Filter.And(
                    Builders<ViMultiServerManagement>.Filter.Ne(x => x.IsDeleted, true),
                    Builders<ViMultiServerManagement>.Filter.Eq(x => x.IsActive, true));
                var ssmServerManagementsData = serverManagementsDBinstanse.Find(filter).ToList();

                if (ssmServerManagementsData != null && ssmServerManagementsData.Count() > 0)
                {
                    foreach (var ssmServer in ssmServerManagementsData)
                    {
                        try
                        {

                            MongoUrl mongoUrl = MongoUrl.Create(ssmServer.DatabaseConnectionString);
                            var mongoClient = new MongoClient(mongoUrl);
                            var _database = mongoClient.GetDatabase(mongoUrl.DatabaseName ?? "visioninsightBIDashboard");

                            // Test the connection
                            _database.RunCommand<MongoDB.Bson.BsonDocument>(new MongoDB.Bson.BsonDocument("ping", 1));
                            _logger?.LogInformation("Successfully connected to Linked MongoDB database: {DatabaseName}", mongoUrl);

                            _linkedDatabase.Add(_database);
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, ex.Message);
                            //var exceptionLog2 = new ExceptionLog();
                            //using (var scope = _serviceProvider.CreateScope())
                            //{
                            //    var exceptionLog = scope.ServiceProvider.GetRequiredService<IExceptionLogRepository>();
                            //    exceptionLog2.ExceptionMessage = ex.Message + " Database Server Name : " + ssmServer.ServerName;
                            //    exceptionLog2.StackTrace = ex.StackTrace;
                            //    exceptionLog2.ExceptionType = ex.GetType().Name;
                            //    exceptionLog2.LoggedAt = DateTime.Now;
                            //    exceptionLog2.RequestPath = "Mongo db connection service for multiple server";
                            //    exceptionLog2.ResponseTime = DateTime.Now;
                            //    exceptionLog2.IsSuccess = false;
                            //    await exceptionLog.InsertAsync(exceptionLog2);
                            //}
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                if (_logger != null)
                {
                    _logger?.LogError(ex, "Failed to initialize linked server connections: {Message}", ex.Message);
                    var exceptionLog2 = new ExceptionLog();
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var exceptionLog = scope.ServiceProvider.GetRequiredService<IExceptionLogRepository>();
                        exceptionLog2.ExceptionMessage = ex.Message;
                        exceptionLog2.StackTrace = ex.StackTrace;
                        exceptionLog2.ExceptionType = ex.GetType().Name;
                        exceptionLog2.LoggedAt = DateTime.Now;
                        exceptionLog2.RequestPath = "Mongo db connection service for multiple server";
                        exceptionLog2.ResponseTime = DateTime.Now;
                        exceptionLog2.IsSuccess = false;
                        await exceptionLog.InsertAsync(exceptionLog2);
                    }
                }
            }
        }


        // Method to get connection info (without exposing password)
        public object GetConnectionInfo()
        {
            var host = _configuration["ConnectionStrings:Host"] ?? "127.0.0.1";
            var port = _configuration["ConnectionStrings:Port"] ?? "27017";
            var databaseName = _configuration["ConnectionStrings:DatabaseName"] ?? "visioninsightBIDashboard";

            return new
            {
                Host = host,
                Port = port,
                DatabaseName = databaseName,
                Username = MongoCredentials.GetUsername(),
                AuthenticationEnabled = true
                // Password is never exposed
            };
        }
    }
}
