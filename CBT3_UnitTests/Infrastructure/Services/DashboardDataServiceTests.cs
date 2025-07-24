using CBT3_Application.Configuration;

using CBT3_Domain.Entities;
using CBT3_Domain.Errors;

using CBT3_Infrastructure.Configuration;
using CBT3_Infrastructure.Interfaces;
using CBT3_Infrastructure.Repositories;
using CBT3_Infrastructure.Services;

using CBT3_Shared.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;

namespace CBT3_UnitTests.Application.Services
{
    public class DashboardDataServiceTests
    {
        IConfigurationRoot _configuration;
        CancellationTokenSource cts = new();
        CancellationToken ct;
        Mock<IServiceScopeFactory> _MockScopefactory = new Mock<IServiceScopeFactory>();
        Mock<ILogSupport> _MockLoggerSupport = new Mock<ILogSupport>();
        Mock<ILogger<DashboardDataService>> _MockServicelogger = new Mock<ILogger<DashboardDataService>>();
        Mock<ILogger<DashboardRepository>> _MockRepologger = new Mock<ILogger<DashboardRepository>>();

        public DashboardDataServiceTests()
        {
            _configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();
        }

        
        public void Initialize()
        {
            ServiceCollection services = new ServiceCollection();
                
            services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(_configuration);
            services.AddTransient<UserDetails>();
            services.AddInfrastructureServices(_configuration);
            services.AddApplicationServices();
            services.AddSharedServices(_configuration);



            ServiceProvider = services.BuildServiceProvider();

        }

        public static IServiceProvider ServiceProvider { get; private set; }


        
        [Fact]
        public void GetTrainingStations_Valid()
        {
            // Arrange
            Initialize();
                       

            var repo = new DashboardRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration);
            var service = new DashboardDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);

            // Act

            var result = service.GetTrainingStationsAsync().Result;

            // Assert
            Assert.True(result.Value.Count > 0);
            Assert.True(result.IsSuccess);


        }
        [Fact]
        public void GetTrainingStations_InValid()
        {
            // Arrange
            Initialize();
           
            var repo = new DashboardRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration);
            var service = new DashboardDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);

            // Act

            var result = service.GetTrainingStationsAsync().Result;

            // Assert
            Assert.False(result.Value.Count > 67); //THERE IS ONLY 33 MACHINES ALLOWED
            Assert.True(result.IsSuccess);


        }
        // Add more tests for other methods and scenarios...

        [Fact]
        public void GetTrainingStation_Valid()
        {
            // Arrange
            Initialize();
          
            var repo = new DashboardRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration);
            var service = new DashboardDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);

            // Act
            string hostName = "PDXCBT31";
            var result = service.GetTrainingStationByHostNameAsync(hostName).Result;

            // Assert
            //Assert.True(result.Value is not null);
            Assert.True(result.IsSuccess);

        }
        [Fact]
        public void GetTrainingStation_InValid()
        {
            // Arrange
            Initialize();
    

            var repo = new DashboardRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration);
            var service = new DashboardDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);

            // Act
            string hostName = "PDXCBT71"; //FAKE HOSTNAME
            var result = service.GetTrainingStationByHostNameAsync(hostName).Result;

            // Assert
            Assert.Equal(DomainErrors.TraininingStationError.NullOrEmpty, result.Error);
            Assert.True(result.IsFailure);


        }


        [Fact]
        public void UpdateTrainingStation_Valid()
        {
            Initialize();


            var repo = new DashboardRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration);
            var service = new DashboardDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);

            string hostName = "PDXCBT31"; //FAKE HOSTNAME
            TrainingStation trainingstation = new();
            trainingstation.HostName = hostName;
            trainingstation.CircuitId = Guid.NewGuid().ToString();//FAKE CircuitId
            trainingstation.Status = CBT3_Domain.Enums.TrainingStationStatus.MachineRunning;
                       

            
            // Act
            var result = service.UpdateTrainingStationAsync(trainingstation).Result;

            // Assert
            Assert.True(result.Value.HostName == hostName);
            Assert.True(result.Value.Status == CBT3_Domain.Enums.TrainingStationStatus.MachineRunning);
            Assert.True(result.IsSuccess);

        }
        [Fact]
        public void UpdateTrainingStation_InValid()
        {
            // Arrange
            Initialize();

            var repo = new DashboardRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration);
            var service = new DashboardDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);

            // Act
            string hostName = "PDXCBT71"; //FAKE HOSTNAME
            

            TrainingStation trainingMachine = new TrainingStation
            {
                HostName = hostName,
                Status = CBT3_Domain.Enums.TrainingStationStatus.MachineRunning,
                
            };

            var result = service.UpdateTrainingStationAsync(trainingMachine).Result;

            // Assert
            Assert.Equal(DomainErrors.TraininingStationError.NullOrEmpty, result.Error);
            Assert.True(result.IsFailure);


        }
    }
}
