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

using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace CBT3_UnitTests.Application.Services
{
    public class CourseDataServiceTests
    {
        IConfigurationRoot _configuration;
        CancellationTokenSource cts = new();
        CancellationToken ct;
        Mock<IServiceScopeFactory> _MockScopefactory = new Mock<IServiceScopeFactory>();
        Mock<ILogSupport> _MockLoggerSupport = new Mock<ILogSupport>();
        Mock<ILogger<CourseDataService>> _MockServicelogger = new Mock<ILogger<CourseDataService>>();
        Mock<ILogger<CourseRepository>> _MockRepologger = new Mock<ILogger<CourseRepository>>();

        public CourseDataServiceTests()
        {
            // Correctly assign the class-level _configuration
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
        }


        
            public void Initialize()
            {
                ServiceCollection services = new ServiceCollection();
                

                services.AddSingleton<IConfiguration>(_configuration);
                services.AddTransient<UserDetails>();
                services.AddInfrastructureServices(_configuration);
                services.AddApplicationServices();
                services.AddSharedServices(_configuration);

                ServiceProvider = services.BuildServiceProvider();

            }
            public static IServiceProvider ServiceProvider { get; private set; }
        
        
        [Fact]
        public void GetCourses_Valid()
        {
            // Arrange
            Initialize();

            var repo = new CourseRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration);
            var service = new CourseDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);

            // Act
            TraineeID trainingID = new TraineeID("e1a1a875-4aea-467d-a1dc-b1aeff0b0257");
            var result = service.GetCoursesAsync(trainingID).Result;

            // Assert
            Assert.True(result.Value.Count > 0);
            Assert.True(result.IsSuccess);


        }
        [Fact]
        public void GetCourses_InValid()
        {
            // Arrange
            ServiceProviderContainer.Initialize();

            var repo = new CourseRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration);
            var service = new CourseDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);

            // Act
            TraineeID trainingID = new TraineeID(Guid.NewGuid().ToString());
            var result = service.GetCoursesAsync(trainingID).Result;

            // Assert
            Assert.True(result.Value.Count == 0);
            Assert.True(result.IsSuccess);


        }
        // Add more tests for other methods and scenarios...

        [Fact]
        public void GetCourse_Valid()
        {
            // Arrange
            Initialize();

            var repo = new CourseRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration);
            var service = new CourseDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);

            // Act
            CourseID courseId = new("BLUE_C2120");
            var result = service.GetCourseByIdAsync(courseId).Result;

            // Assert
            //Assert.True(result.Value is not null);
            Assert.True(result.IsSuccess);

        }
        [Fact]
        public void GetCourse_InValid()
        {
            // Arrange
            Initialize();

            var repo = new CourseRepository(_MockRepologger.Object, _MockLoggerSupport.Object, _configuration);
            var service = new CourseDataService(_MockServicelogger.Object, _MockScopefactory.Object, _configuration, repo);

            // Act
            CourseID courseId = null; ;
            var result = service.GetCourseByIdAsync(courseId).Result;

            // Assert
            Assert.Equal(DomainErrors.CourseError.NullOrEmpty, result.Error);
            Assert.True(result.IsFailure);


        }
    }
}
