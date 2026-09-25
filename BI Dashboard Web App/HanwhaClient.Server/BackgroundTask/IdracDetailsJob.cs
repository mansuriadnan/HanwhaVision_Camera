using HanwhaClient.Application.Interfaces;
using HanwhaClient.Core.SignalR;
using HanwhaClient.Helper;
using HanwhaClient.Infrastructure.Interfaces;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.Dto;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X509;
using Quartz;

namespace HanwhaClient.Server.BackgroundTask
{
    public class IdracDetailsJob : IJob
    {
        private readonly IIDracManagementRepository _iIDracManagementRepository;
        private readonly IIdracClientService _idracClientService;
        private readonly IIdracDetailsRepository _idracDetailsRepository;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHubContext<NotificationHub> _hubContext;

        public IdracDetailsJob(IIDracManagementRepository iDracManagementRepository,
            IIdracClientService idracClientService,
            ILogger<GlobalExceptionHandlerMiddleware> logger,
            IServiceProvider serviceProvider,
            IIdracDetailsRepository idracDetailsRepository,
            IHubContext<NotificationHub> hubContext) 
        {
            _iIDracManagementRepository = iDracManagementRepository;
            _idracClientService = idracClientService;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _hubContext = hubContext;
            _idracDetailsRepository = idracDetailsRepository;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var idracList = await _iIDracManagementRepository.GetAllAsync();
            Console.WriteLine($"idrac job executed at: {DateTime.Now}");

            var tasks = idracList.Select(async idrac =>
            {
                try
                {
                    var login = await _idracClientService.IdracLoginAsync(idrac.IPAddress, idrac.UserName, idrac.Password);
                    if (login)
                    {
                        IdracDetails idracDetails = new IdracDetails();
                        
                        var idracMainModel = await _idracClientService.GetIdracAsync<IdracMainComponent>(IdracApiConstant.IdracMainComponent, idrac.IPAddress);
                        if (idracMainModel != null)
                        {
                            idracDetails.Health = idracMainModel.Status.Health;
                            idracDetails.Memory = await GetMemoryDetailsAsync(idrac.IPAddress, idracMainModel);
                            idracDetails.Processor =  await GetCpuDetailsAsync(idrac.IPAddress, idracMainModel);
                            var coolingDetails = await GetCoolingDetailsAsync(idrac.IPAddress, idracMainModel);
                            idracDetails.Cooling = coolingDetails.Cooling;
                            idracDetails.Temperature = coolingDetails.temperature;

                            idracDetails.PowerSupply = await GetPowerSupplyDetailsAsync(idrac.IPAddress);
                            idracDetails.EmbeddedNetworkCard = await GetEmbeddedNetworkCardCardDetailsAsync(idrac.IPAddress);
                            idracDetails.IntegratedNetworkCard = await GetIntegratedNetworkCardCardDetailsAsync(idrac.IPAddress);
                            idracDetails.Storage = await GetStorageDetailsAsync(idrac.IPAddress);

                            idracDetails.IdracServerId = idrac.Id;
                            await _idracDetailsRepository.UpsertIdracDetailsAsync(idracDetails);

                            if (idrac.CpuLoad != null && idrac.CpuLoad > 0)
                            {
                                var cpuUsage = await _idracClientService.GetIdracAsync<CPUUsageResponse>(IdracApiConstant.IdracSystemBoardCPUUsage, idrac.IPAddress);

                                if (cpuUsage != null) {
                                    if (cpuUsage.Reading > idrac.CpuLoad)
                                    {
                                        var jsonMessage = JsonConvert.SerializeObject(new
                                        {
                                            SourceIP = idrac.IPAddress,
                                            Message = $"{idrac.ServerName} CPU utilization exceeded the configured threshold ({idrac.CpuLoad}). Current utilization: {cpuUsage.Reading}",
                                            Severity = "High"
                                        });

                                        await _hubContext.Clients.Group("iDRAC_Group").SendAsync("idracEventNotification", jsonMessage);
                                    }
                                }
                            }

                            if (idrac.MemoryUsage != null && idrac.MemoryUsage > 0)
                            {
                                var memUsage = await _idracClientService.GetIdracAsync<MemoryChartResponse>(IdracApiConstant.IdracSystemBoardMEMUsage, idrac.IPAddress);

                                if (memUsage != null)
                                {
                                    if (memUsage.Reading > idrac.MemoryUsage)
                                    {
                                        var jsonMessage = JsonConvert.SerializeObject(new
                                        {
                                            SourceIP = idrac.IPAddress,
                                            Message = $"{idrac.ServerName} Memory utilization exceeded the configured threshold ({idrac.CpuLoad}). Current utilization: {memUsage.Reading}",
                                            Severity = "High"
                                        });

                                        await _hubContext.Clients.Group("iDRAC_Group").SendAsync("idracEventNotification", jsonMessage);
                                    }
                                }
                            }

                            if (idrac.Temperature != null && idrac.Temperature > 0)
                            {
                                if (idracDetails.Temperature != null)
                                {
                                    var exceedTemprature = idracDetails.Temperature.Where(x => x.Temperature > idrac.Temperature);

                                    if (exceedTemprature.Count() > 0)
                                    {
                                        var jsonMessage = JsonConvert.SerializeObject(exceedTemprature.Select(x => new
                                        {
                                            SourceIP = idrac.IPAddress,
                                            Message = $"{x.Name} temprature exceeded the configured threshold ({idrac.Temperature}). Current temprature: {x.Temperature}",
                                            Severity = "High"
                                        }));

                                        await _hubContext.Clients.Group("iDRAC_Group").SendAsync("idracEventNotification", jsonMessage);
                                    }
                                }
                            }

                        }
                    }
                    else 
                    {
                        _logger.LogError(null, "Login failed at Idrac server " + idrac.IPAddress);
                        var exceptionLog2 = new ExceptionLog();
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var exceptionLog = scope.ServiceProvider.GetRequiredService<IExceptionLogService>();
                            exceptionLog2.ExceptionMessage = "Login failed at Idrac server " + idrac.IPAddress;
                            exceptionLog2.StackTrace = null;
                            exceptionLog2.ExceptionType = null;
                            exceptionLog2.LoggedAt = DateTime.Now;
                            exceptionLog2.RequestPath = "Idrac details job";
                            exceptionLog2.ResponseTime = DateTime.Now;
                            exceptionLog2.IsSuccess = false;
                            await exceptionLog.SaveExceptionLogAsync(exceptionLog2);
                        }
                        Console.WriteLine($"idrac job goes into else login failed: {DateTime.Now}");
                    }

                    await _idracClientService.IdracLogoutAsync(idrac.IPAddress);


                }
                catch (Exception ex)
                {
                    await _idracClientService.IdracLogoutAsync(idrac.IPAddress);
                    _logger.LogError(ex, ex.Message);
                    var exceptionLog2 = new ExceptionLog();
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var exceptionLog = scope.ServiceProvider.GetRequiredService<IExceptionLogService>();
                        exceptionLog2.ExceptionMessage = ex.Message;
                        exceptionLog2.StackTrace = ex.StackTrace;
                        exceptionLog2.ExceptionType = ex.GetType().Name;
                        exceptionLog2.LoggedAt = DateTime.Now;
                        exceptionLog2.RequestPath = "";
                        exceptionLog2.ResponseTime = DateTime.Now;
                        exceptionLog2.IsSuccess = false;
                        await exceptionLog.SaveExceptionLogAsync(exceptionLog2);
                    }
                    // Log exception for this specific server processing so the job can continue with the rest
                    Console.WriteLine($"Error processing IDRAC {idrac.IPAddress}: {ex.Message}");
                    return;
                }
            });

            await Task.WhenAll(tasks);
            Console.WriteLine($"idrac job executed ends at: {DateTime.Now}");
            return;
        }

        private async Task<IdracMemory> GetMemoryDetailsAsync(string ipAddress, IdracMainComponent idracMainModel)
        {
            IdracMemory idracMemory = new IdracMemory();
            idracMemory.TotalMemory = idracMainModel.MemorySummary.TotalSystemMemoryGiB;
            idracMemory.TotalSlots = idracMainModel.Oem.Dell.DellSystem.MaxDIMMSlots;
            idracMemory.UsedSlots = idracMainModel.Oem.Dell.DellSystem.PopulatedDIMMSlots;
            idracMemory.Health = idracMainModel.MemorySummary.Status.Health;
            idracMemory.MemoryList = new List<IdracMemoryDetails>();

            var memoryMain = await _idracClientService.GetIdracAsync<MemoryCollectionResponse>(idracMainModel.Memory.OdataId, ipAddress);

            if (memoryMain != null && memoryMain.Members != null)
            {
                var memoryTasks = memoryMain.Members.Select(async memory =>
                {
                    var indMemoryRes = await _idracClientService.GetIdracAsync<MemoryDetailResponse>(memory.OdataId, ipAddress);

                    IdracMemoryDetails indMemory = new IdracMemoryDetails();
                    if (indMemoryRes != null)
                    {
                        indMemory.RamName = indMemoryRes.Name;
                        indMemory.Speed = indMemoryRes.OperatingSpeedMhz;
                        indMemory.Size = indMemoryRes.CapacityMiB;
                        indMemory.Manufacturer = indMemoryRes.Manufacturer;
                        indMemory.Type = indMemoryRes.MemoryDeviceType;
                        indMemory.Health = indMemoryRes.Status?.Health;
                    }
                    return indMemory;
                });

                var memoryResults = await Task.WhenAll(memoryTasks);
                idracMemory.MemoryList.AddRange(memoryResults);
            }

            return idracMemory;
        }

        private async Task<IdracProcessor> GetCpuDetailsAsync(string ipAddress, IdracMainComponent idracMainModel)
        {
            IdracProcessor idracProcessor = new IdracProcessor();
            idracProcessor.TotalCpu = idracMainModel.Oem.Dell.DellSystem.MaxCPUSockets;
            idracProcessor.UsedCpu = idracMainModel.ProcessorSummary.Count;
            idracProcessor.Health = idracMainModel.ProcessorSummary.Status.Health;
            idracProcessor.ProcessorList = new List<IdracProcessorDetails>();

            var processorsMain = await _idracClientService.GetIdracAsync<MemoryCollectionResponse>(idracMainModel.Processors.OdataId, ipAddress);

            if (processorsMain != null && processorsMain.Members != null)
            {
                var processorsTasks = processorsMain.Members.Select(async processor =>
                {
                    var indprocessorRes = await _idracClientService.GetIdracAsync<ProcessorDetailResponse>(processor.OdataId, ipAddress);

                    IdracProcessorDetails indprocessor = new IdracProcessorDetails();
                    if (indprocessorRes != null)
                    {
                        indprocessor.CpuName = indprocessorRes.Name;
                        indprocessor.Model = indprocessorRes.Model;
                        indprocessor.Cores = indprocessorRes.TotalCores;
                        indprocessor.Threads = indprocessorRes.TotalThreads;
                        indprocessor.MaxSpeed = indprocessorRes.MaxSpeedMHz;
                        indprocessor.CurrentSpeed = indprocessorRes.OperatingSpeedMHz;
                        indprocessor.Health = indprocessorRes.Status.Health;

                    }
                    return indprocessor;
                });

                var processorsResults = await Task.WhenAll(processorsTasks);
                idracProcessor.ProcessorList.AddRange(processorsResults);
            }

            return idracProcessor;
        }

        private async Task<(List<CoolingDetail> Cooling, List<CoolingTemperatureDetail> temperature)> GetCoolingDetailsAsync(string ipAddress, IdracMainComponent idracMainModel)
        {
            var CoolingMain = await _idracClientService.GetIdracAsync<ThermalRoot>(IdracApiConstant.IdracCoolingFan, ipAddress);
            List<CoolingDetail> colling = new List<CoolingDetail>();  
            List<CoolingTemperatureDetail> temperature = new List<CoolingTemperatureDetail>();

            if (CoolingMain != null && CoolingMain.Fans != null)
            {
                var coolingTasks = CoolingMain.Fans.Select(async cooling =>
                {
                    CoolingDetail indCoolingDetail = new CoolingDetail();
                    if (cooling != null)
                    {
                        indCoolingDetail.FanName = cooling.FanName;
                        indCoolingDetail.FanSpeed = cooling.Reading;
                        indCoolingDetail.Health = cooling.Status.Health;
                    }
                    return indCoolingDetail;
                });

                var coolingResults = await Task.WhenAll(coolingTasks);
                colling.AddRange(coolingResults);
            }

            if (CoolingMain != null && CoolingMain.Temperatures != null)
            {
                var temperaturesTasks = CoolingMain.Temperatures.Select(async Temperature =>
                {
                    CoolingTemperatureDetail indTemperatureDetail = new CoolingTemperatureDetail();
                    if (Temperature != null)
                    {
                        indTemperatureDetail.Name = Temperature.Name;
                        indTemperatureDetail.PhysicalContext = Temperature.PhysicalContext;
                        indTemperatureDetail.Temperature = Temperature.ReadingCelsius;
                        indTemperatureDetail.health = Temperature.Status.Health;
                    }
                    return indTemperatureDetail;
                });

                var temperatureResults = await Task.WhenAll(temperaturesTasks);
                temperature.AddRange(temperatureResults);
            }
            return (colling, temperature);
        }

        private async Task<PowerSupply> GetPowerSupplyDetailsAsync(string ipAddress)
        {
            PowerSupply powerSupply = new PowerSupply();
            powerSupply.PowerSupplyList = new List<PowerSupplyDetail>();
            var powerSupplyMain = await _idracClientService.GetIdracAsync<PowerSubsystemRoot>(IdracApiConstant.IdracPowerSystem, ipAddress);

            if (powerSupplyMain != null) {
                powerSupply.TotalCapacity = powerSupplyMain.CapacityWatts;
                powerSupply.Health = powerSupplyMain.Status.Health;
                powerSupply.Redundancy = powerSupplyMain.Status.State;

                var powerSupplySupplies = await _idracClientService.GetIdracAsync<MemoryCollectionResponse>(powerSupplyMain.PowerSupplies.OdataId, ipAddress);
                if (powerSupplySupplies != null && powerSupplySupplies.Members.Count > 0) {

                    var powerSupplySuppliesTask = powerSupplySupplies.Members.Select(async powerSupplyUrl =>
                    {
                        PowerSupplyDetail powerSupplyDetailInd = new PowerSupplyDetail();
                        var powerSupplyRes = await _idracClientService.GetIdracAsync<PowerSupplyRoot>(powerSupplyUrl.OdataId, ipAddress);
                        if (powerSupplyRes != null)
                        {
                            powerSupplyDetailInd.Capacity = powerSupplyRes.PowerCapacityWatts;
                            powerSupplyDetailInd.Health = powerSupplyRes.Status.Health;
                            powerSupplyDetailInd.Psu = powerSupplyRes.Name;

                            var powerSupplyMetricsRes = await _idracClientService.GetIdracAsync<PowerSupplyMetricsRoot>(powerSupplyRes.Metrics.OdataId, ipAddress);
                            if (powerSupplyMetricsRes != null)
                            {
                                powerSupplyDetailInd.CurrentVoltage = powerSupplyMetricsRes.InputCurrentAmps.Reading;
                                powerSupplyDetailInd.Output = powerSupplyMetricsRes.OutputPowerWatts.Reading;
                                powerSupplyDetailInd.InputVoltage = powerSupplyMetricsRes.InputVoltage.Reading;
                            }
                        }

                        return powerSupplyDetailInd;
                    });

                    var powerSupplySuppliesResults = await Task.WhenAll(powerSupplySuppliesTask);
                    powerSupply.PowerSupplyList.AddRange(powerSupplySuppliesResults);
                }
            }
            return powerSupply;
        }

        private async Task<NetworkCard> GetEmbeddedNetworkCardCardDetailsAsync(string ipAddress)
        {
            NetworkCard networkCard = new NetworkCard();
            networkCard.NetworkCardList = new List<NetworkCardDetails>();
            var networkAdapter = await _idracClientService.GetIdracAsync<MemoryCollectionResponse>(IdracApiConstant.IdracNetworkAdapter, ipAddress);

            if(networkAdapter != null)
            {
                var embeddedUrl = networkAdapter.Members.Where(x => x.OdataId.Contains("Embedded")).FirstOrDefault();
                var networkAdapterMain = await _idracClientService.GetIdracAsync<NetworkAdapterMain>(embeddedUrl.OdataId, ipAddress);

                if(networkAdapterMain != null)
                {
                    networkCard.Manufacturer = networkAdapterMain.Manufacturer;
                    networkCard.Health = networkAdapterMain.Status.Health;
                    networkCard.Model = networkAdapterMain.Model;

                    if (networkAdapterMain.NetworkDeviceFunctions.OdataId != null && networkAdapterMain.NetworkPorts.OdataId != null)
                    {
                        var networkFunctionLinks = await _idracClientService.GetIdracAsync<MemoryCollectionResponse>(networkAdapterMain.NetworkDeviceFunctions.OdataId, ipAddress);
                        var networkportLinks = await _idracClientService.GetIdracAsync<MemoryCollectionResponse>(networkAdapterMain.NetworkPorts.OdataId, ipAddress);
                        if(networkFunctionLinks != null && networkportLinks!= null)
                        {
                            for (int i = 0; i < networkFunctionLinks.Members.Count; i++)
                            {
                                NetworkCardDetails networkCardDetails = new NetworkCardDetails();
                                var networkFunctionRes = await _idracClientService.GetIdracAsync<NetworkFunctionResponse>(networkFunctionLinks.Members[i].OdataId, ipAddress);

                                if (networkFunctionRes != null)
                                {
                                    networkCardDetails.ProductName = networkFunctionRes.Oem.Dell.DellNIC.ProductName;
                                    networkCardDetails.VendorName = networkFunctionRes.Oem.Dell.DellNIC.VendorName;
                                    networkCardDetails.Protocol = networkFunctionRes.Oem.Dell.DellNIC.Protocol;
                                    networkCardDetails.health = networkFunctionRes.Status.Health;
                                    
                                    var networkPortRes = await _idracClientService.GetIdracAsync<NetworkPortResponse>(networkportLinks.Members[i].OdataId, ipAddress);
                                    if (networkPortRes != null)
                                    {
                                        networkCardDetails.CurrentLinkSpeed = networkPortRes.CurrentLinkSpeedMbps;
                                        networkCardDetails.PhysicalPortNumber = networkPortRes.PhysicalPortNumber;
                                        networkCardDetails.LinkStatus = networkPortRes.LinkStatus;
                                        networkCardDetails.ActiveLinkTechnology = networkPortRes.ActiveLinkTechnology;
                                        networkCardDetails.AssociatedNetworkAddress = networkPortRes.AssociatedNetworkAddresses.FirstOrDefault();
                                    }
                                }
                                networkCard.NetworkCardList.Add(networkCardDetails);
                            }
                        }
                    }
                }

            }

            return networkCard;
        }

        private async Task<NetworkCard> GetIntegratedNetworkCardCardDetailsAsync(string ipAddress)
        {
            NetworkCard networkCard = new NetworkCard();
            networkCard.NetworkCardList = new List<NetworkCardDetails>();
            var networkAdapter = await _idracClientService.GetIdracAsync<MemoryCollectionResponse>(IdracApiConstant.IdracNetworkAdapter, ipAddress);

            if (networkAdapter != null)
            {
                var integratedUrl = networkAdapter.Members.Where(x => x.OdataId.Contains("Integrated")).FirstOrDefault();
                var networkAdapterMain = await _idracClientService.GetIdracAsync<NetworkAdapterMain>(integratedUrl.OdataId, ipAddress);

                if (networkAdapterMain != null)
                {
                    networkCard.Manufacturer = networkAdapterMain.Manufacturer;
                    networkCard.Health = networkAdapterMain.Status.Health;
                    networkCard.Model = networkAdapterMain.Model;

                    if (networkAdapterMain.NetworkDeviceFunctions.OdataId != null && networkAdapterMain.NetworkPorts.OdataId != null)
                    {
                        var networkFunctionLinks = await _idracClientService.GetIdracAsync<MemoryCollectionResponse>(networkAdapterMain.NetworkDeviceFunctions.OdataId, ipAddress);
                        var networkportLinks = await _idracClientService.GetIdracAsync<MemoryCollectionResponse>(networkAdapterMain.NetworkPorts.OdataId, ipAddress);
                        if (networkFunctionLinks != null && networkportLinks != null)
                        {
                            for (int i = 0; i < networkFunctionLinks.Members.Count; i++)
                            {
                                NetworkCardDetails networkCardDetails = new NetworkCardDetails();
                                var networkFunctionRes = await _idracClientService.GetIdracAsync<NetworkFunctionResponse>(networkFunctionLinks.Members[i].OdataId, ipAddress);

                                if (networkFunctionRes != null)
                                {
                                    networkCardDetails.ProductName = networkFunctionRes.Oem.Dell.DellNIC.ProductName;
                                    networkCardDetails.VendorName = networkFunctionRes.Oem.Dell.DellNIC.VendorName;
                                    networkCardDetails.Protocol = networkFunctionRes.Oem.Dell.DellNIC.Protocol;
                                    networkCardDetails.health = networkFunctionRes.Status.Health;

                                    var networkPortRes = await _idracClientService.GetIdracAsync<NetworkPortResponse>(networkportLinks.Members[i].OdataId, ipAddress);
                                    if (networkPortRes != null)
                                    {
                                        networkCardDetails.CurrentLinkSpeed = networkPortRes.CurrentLinkSpeedMbps;
                                        networkCardDetails.PhysicalPortNumber = networkPortRes.PhysicalPortNumber;
                                        networkCardDetails.LinkStatus = networkPortRes.LinkStatus;
                                        networkCardDetails.ActiveLinkTechnology = networkPortRes.ActiveLinkTechnology;
                                        networkCardDetails.AssociatedNetworkAddress = networkPortRes.AssociatedNetworkAddresses.FirstOrDefault();
                                    }
                                }
                                networkCard.NetworkCardList.Add(networkCardDetails);
                            }
                        }
                    }
                }

            }

            return networkCard;
        }

        private async Task<IdracStorage> GetStorageDetailsAsync(string ipAddress)
        {
            IdracStorage idracStorage = new IdracStorage();

            var storageMain = await _idracClientService.GetIdracAsync<StorageCollectionResponse>(
                IdracApiConstant.IdracStorage,
                ipAddress);

            if (storageMain != null && storageMain.Members != null)
            {
                var storageTasks = storageMain.Members.Select(async storageMember =>
                {
                    var storageRes = await _idracClientService.GetIdracAsync<StorageDetailResponse>(
                        storageMember.OdataId,
                        ipAddress);

                    List<IdracDriveDetails> drivesList = new List<IdracDriveDetails>();

                    if (storageRes != null && storageRes.Drives != null)
                    {
                        var drivesTasks = storageRes.Drives.Select(async driveMember =>
                        {
                            var driveRes = await _idracClientService.GetIdracAsync<DriveDetailResponse>(
                                driveMember.OdataId,
                                ipAddress);

                            if (driveRes == null)
                                return null;

                            IdracDriveDetails drive = new IdracDriveDetails
                            {
                                DiskName = driveRes.Name,
                                Size = driveRes.CapacityBytes,
                                Type = driveRes.MediaType,
                                Protocol = driveRes.Protocol,
                                Rpm = 500,
                                Health = driveRes.Status?.Health
                            };

                            return drive;
                        });

                        var drivesResults = await Task.WhenAll(drivesTasks);

                        drivesList.AddRange(drivesResults.Where(x => x != null));
                    }
                    return drivesList;
                });

                var storageResults = await Task.WhenAll(storageTasks);

                idracStorage.Drives = storageResults.SelectMany(x => x).ToList();
            }
            return idracStorage;
        }

    }
}
  