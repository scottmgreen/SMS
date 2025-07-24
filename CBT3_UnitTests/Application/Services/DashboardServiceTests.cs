using CBT3_Infrastructure.Repositories;
using CBT3_Infrastructure.Services;

using CBT3_Application.Configuration;
using CBT3_Application.Interfaces;
using CBT3_Application.Services;

using CBT3_Domain.Entities;

using CBT3_Infrastructure.Configuration;
using CBT3_Infrastructure.Services;

using CBT3_Shared;
using CBT3_Shared.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

using Moq;
using CBT3_Infrastructure.Interfaces;
using System;
using CBT3_Domain.Interfaces;

namespace CBT3_UnitTests.Application.Services
{
    public class DashboardServiceTests
    {
        IConfigurationRoot _configuration;
        CancellationTokenSource cts = new();
        CancellationToken ct;
        Mock<IServiceScopeFactory> _MockScopefactory = new Mock<IServiceScopeFactory>();
        Mock<ILogSupport> _MockLoggerSupport = new Mock<ILogSupport>();
        Mock<ILogger<DashboardDataService>> _MockServicelogger = new Mock<ILogger<DashboardDataService>>();
        Mock<ILogger<DashboardRepository>> _MockRepologger = new Mock<ILogger<DashboardRepository>>();
        IFeatureManager _featureManager;
        IMediator _mediator;

        public DashboardServiceTests()
        {
            _configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();
        }
        public void Initialize()
        {
            ServiceCollection services = new ServiceCollection();
            
            services.AddSingleton<IConfiguration>(_configuration);
            services.AddInfrastructureServices(_configuration);
            services.AddApplicationServices();
            services.AddSharedServices(_configuration);

            ServiceProvider = services.BuildServiceProvider();
            IFeatureManager _featureManager = ServiceProvider.GetRequiredService<IFeatureManager>();
            IMediator _mediator = ServiceProvider.GetRequiredService<IMediator>();
        }
        public static IServiceProvider ServiceProvider { get; private set; }

        [Fact()]
        public void GetTrainingMachines_Valid()
        {
            // Arrange
            Initialize();
                        
            var repo = new DashboardRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration);
            var service = new DashboardDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);
            
            // Act
             var result = service.GetTrainingStationsAsync();

            // Assert
            Assert.True(result.Result.Value.Count > 0);
            Assert.True(result.Result.IsSuccess);

 
        }
        [Fact()]
        public void GetTrainingMachineByHostName_Valid()
        {
            // Arrange
            Initialize();

            var repo = new DashboardRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration);
            var service = new DashboardDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);

            string hostName = "PDXCBT31";
            // Act
            var result = service.GetTrainingStationByHostNameAsync(hostName);

            // Assert
            Assert.True(result.Result.Value.HostName == hostName);
            Assert.True(result.Result.IsSuccess);


        }
        [Fact()]
        public void UpdateTrainingMachine_Valid()
        {
            // Arrange
            Initialize();

            var repo = new DashboardRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration);
            var service = new DashboardDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);

            string hostName = "PDXCBT31"; //FAKE HOSTNAME

            TrainingStation trainingMachine = new TrainingStation
            {
                HostName = hostName,
                Status = CBT3_Domain.Enums.TrainingStationStatus.MachineRunning,
                CircuitId = Guid.NewGuid().ToString()//FAKE CircuitId
            };

            // Act
            var result = service.UpdateTrainingStationAsync(trainingMachine);

            // Assert
            Assert.True(result.Result.Value.HostName == hostName);
            Assert.True(result.Result.IsSuccess);


        }
    }
}
