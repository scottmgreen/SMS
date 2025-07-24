using CBT3_Application.Configuration;
using CBT3_Application.Interfaces;
using CBT3_Application.Services;

using CBT3_Domain.Common;
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
using Microsoft.FeatureManagement;

using Moq;

namespace CBT3_UnitTests.Application.Services
{
    public class SystemServiceTests
    {
        IConfigurationRoot _configuration;
        CancellationTokenSource cts = new();
        CancellationToken ct;
        Mock<IServiceScopeFactory> _MockScopefactory = new Mock<IServiceScopeFactory>();
        Mock<ILogSupport> _MockLoggerSupport = new Mock<ILogSupport>();
        Mock<ILogger<SystemDataService>> _MockServicelogger = new Mock<ILogger<SystemDataService>>();
        Mock<ILogger<SystemRepository>> _MockRepologger = new Mock<ILogger<SystemRepository>>();
        FileService _fileService ;
        IFeatureManager _featureManager;
        IMediator _mediator;


        public SystemServiceTests()
        {
            _configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

            
            

        }
        public void Initialize()
        {
            ServiceCollection services = new ServiceCollection();
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddInfrastructureServices(configuration);
            services.AddApplicationServices();
            services.AddSharedServices(configuration);

            ServiceProvider = services.BuildServiceProvider();

            _featureManager = ServiceProvider.GetRequiredService<IFeatureManager>();
            _mediator = ServiceProvider.GetRequiredService<IMediator>();
            _fileService = ServiceProvider.GetRequiredService<FileService>();

        }

        public static IServiceProvider ServiceProvider { get; private set; }

        [Fact]
        public async Task AddAuditLogEntryAsync_Success_ReturnsSuccess()
        {
            // Arrange
            Initialize();

            var repo = new SystemRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration, _featureManager);
            var service = new SystemDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);


            AuditLogEntryID logentryId = new(Guid.NewGuid().ToString());
            AuditLogEntry logentry = new(logentryId);
            logentry.UserID = "SYSTEM";
            logentry.Workstation = "localhost";
            logentry.Module = "UNIT TESTING";
            logentry.MessageType = "Pipeline";
            logentry.Description = "UNIT TESTING";
            logentry.EventDateTime = DateTime.Now.ToString("MM_dd_yyyy_hh:mm:ss");
            logentry.Function = "AddAuditLogEntryAsync_Success_ReturnsSuccess";
            logentry.Severity = "CBT3_ApplicationEventIds.Information";
            
            // Act
            var result = await service.AddAuditLogEntryAsync(logentry);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value);
        }

        [Fact]
        public async Task AddAuditLogEntryAsync_Failure_ReturnsFailure()
        {
            // Arrange
            Initialize();

            var repo = new SystemRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration, _featureManager);
            var service = new SystemDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);
            AuditLogEntryID auditlogid = new AuditLogEntryID(Guid.NewGuid().ToString());
            var auditLogEntry = new AuditLogEntry(auditlogid);

            //_mockDataService
            //    .Setup(ds => ds.AddAuditLogEntryAsync(It.IsAny<AuditLogEntry>(), It.IsAny<CancellationToken>()))
            //    .ReturnsAsync(Result<bool>.Failure(DomainErrors.SystemError.AuditLogEntryError));

            // Act
            var result = await service.AddAuditLogEntryAsync(auditLogEntry);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(DomainErrors.SystemError.AuditLogEntryError, result.Error);
        }

        [Fact]
        public async Task IsEnabledAsync_FeatureEnabled_ReturnsTrue()
        {
            Initialize();

            //var repo = new SystemRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration);
            //var service = new SystemDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);
            string featureName = "LoggingEnabled";
            

            // Act
            var result = await _featureManager.IsEnabledAsync(featureName);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsEnabledAsync_FeatureDisabled_ReturnsFalse()
        {
            Initialize();
            // Arrange
            string featureName = "TestFeature";

            // Act
            var result = await _featureManager.IsEnabledAsync(featureName);

            // Assert
            Assert.False(result);
        }

     
        [Fact]
        public async Task IsFileExists_FileExists_ReturnsTrue()
        {
            // Arrange
            Initialize();
            // Arrange
            var repo = new SystemRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration, _featureManager);
            var dataservice = new SystemDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);
            var service = new SystemService(_featureManager, dataservice, _fileService);
            string testFilePath = "test.txt";
            string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string fullPath = Path.Combine(wwwRootPath, testFilePath);

            // Ensure the file exists for the test
            Directory.CreateDirectory(wwwRootPath);
            File.WriteAllText(fullPath, "Test Content");

            // Act
            var result = await service.IsFileExists(testFilePath);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value);

            // Cleanup
            File.Delete(fullPath);
        }
        [Fact]
        public async Task IsFileExists_FileDoesNotExist_ReturnsFalse()
        {
            // Arrange
            Initialize();
            // Arrange
            var repo = new SystemRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration, _featureManager);
            var dataservice = new SystemDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);
            var service = new SystemService(_featureManager, dataservice, _fileService);
            string testFilePath = "nonexistent.txt";

            // Act
            var result = await service.IsFileExists(testFilePath);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.Value);
        }

        [Fact]
        public async Task IsFileExists_Exception_ReturnsFailure()
        {
            // Arrange
            Initialize();
            // Arrange
            var repo = new SystemRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration, _featureManager);
            var dataservice = new SystemDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);
            var service = new SystemService(_featureManager, dataservice, _fileService);
            string invalidFileName = "nonexistent<>.txt";
            string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            Directory.CreateDirectory(wwwRootPath);
            string fullPath = Path.Combine(wwwRootPath, invalidFileName);

            CancellationToken ct = new();

            //var invalidPath = new string(Path.GetInvalidPathChars());

            // Act
            var result = await _fileService.IsFileExistsAsync(fullPath,ct);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(DomainErrors.SystemError.FileExistsError, result.Error);

            // Cleanup
            Directory.Delete(wwwRootPath);
        }

        [Fact]
        public async Task GetAdminPasscode_ReturnsCorrectValue()
        {
            Initialize();
            // Arrange
            var repo = new SystemRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration, _featureManager);
            var dataservice = new SystemDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);
            var service = new SystemService(_featureManager, dataservice, _fileService);
            var expectedResult = Result<int>.Success(1379);
            

            // Act
            var result = await service.GetAdminPasscodeAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedResult.Value, result.Value);
        }
        [Fact]
        public async Task GetAdminPasscode_ReturnsIncorrectValue()
        {
            Initialize();
            // Arrange
            var repo = new SystemRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration, _featureManager);
            var dataservice = new SystemDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);
            var service = new SystemService(_featureManager, dataservice, _fileService);
            var expectedResult = Result<int>.Success(1111);


            // Act
            var result = await service.GetAdminPasscodeAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotEqual(expectedResult.Value, result.Value);
        }
    }
}
