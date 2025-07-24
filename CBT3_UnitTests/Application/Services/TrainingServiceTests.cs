using CBT3_Infrastructure.Repositories;
using CBT3_Infrastructure.Services;

using CBT3_Application.Configuration;
using CBT3_Application.Interfaces;
using CBT3_Application.Services;

using CBT3_Infrastructure.Configuration;

using CBT3_Shared;
using CBT3_Shared.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Moq;
using CBT3_Infrastructure.Interfaces;
using CBT3_Domain.Common;
using CBT3_Domain.Entities;
using CBT3_Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using CBT3_Domain.Interfaces;

namespace CBT3_UnitTests.Application.Services
{
    public class TrainingServiceTests
    {
        IConfigurationRoot _configuration;
        IMediator _Mediator ;
        Mock<IMessenger> _MockEventAggregator = new Mock<IMessenger>();
        Mock<IServiceScopeFactory> _MockScopefactory = new Mock<IServiceScopeFactory>();
        Mock<ILogSupport> _MockLoggerSupport = new Mock<ILogSupport>();
        //loggers//
        Mock<ILogger<TrainingDataService>> _MockTrainingServicelogger = new Mock<ILogger<TrainingDataService>>();
        Mock<ILogger<CourseDataService>> _MockCourseServicelogger = new Mock<ILogger<CourseDataService>>();
        Mock<ILogger<TrainingRepository>> _MockTrainingRepoLogger = new Mock<ILogger<TrainingRepository>>();
        Mock<ILogger<CourseRepository>> _MockCourseRepoLogger = new Mock<ILogger<CourseRepository>>();
        CancellationToken ct;


        public TrainingServiceTests()
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

            _Mediator = ServiceProvider.GetRequiredService<IMediator>();
        }
        public static IServiceProvider ServiceProvider { get; private set; }

        [Fact()]
        public void GetCourseCodes_ReturnsCorrectCourseCodes()
        {
            // Arrange
            Initialize();
                        
            var trainingRepo = new TrainingRepository(_MockTrainingRepoLogger.Object, _MockLoggerSupport.Object, _configuration);
            var courseRepo = new CourseRepository(_MockCourseRepoLogger.Object, _MockLoggerSupport.Object, _configuration);
            
            var training_service = new TrainingDataService(_MockTrainingServicelogger.Object, _MockScopefactory.Object, _configuration, trainingRepo);
            var course_service = new CourseDataService(_MockCourseServicelogger.Object, _MockScopefactory.Object, _configuration, courseRepo);
            
            var trainingService = new TrainingService(_Mediator, training_service, course_service);

            TraineeID traineeid = new TraineeID("e1a1a875-4aea-467d-a1dc-b1aeff0b0257");
            Trainee trainee = trainingService.GetTraineeByIdAsync(traineeid).Result.Value;
            bool showSidaOnly = true;
            // Act
            var result = trainingService.GetCourseCodesAsync(trainee, showSidaOnly);

            // Assert
            Assert.Equal(4, result.Result.Value.Count); // Expected count of SIDA courses

           
            
        }


        [Fact]
        public async Task CheckForPreviousTrainingOnDateAsync_ReturnsExpectedResult()
        {
            // Arrange
            Initialize();

            var trainingRepo = new TrainingRepository(_MockTrainingRepoLogger.Object, _MockLoggerSupport.Object, _configuration);
            var courseRepo = new CourseRepository(_MockCourseRepoLogger.Object, _MockLoggerSupport.Object, _configuration);
            var training_service = new TrainingDataService(_MockTrainingServicelogger.Object, _MockScopefactory.Object, _configuration, trainingRepo);
            var course_service = new CourseDataService(_MockCourseServicelogger.Object, _MockScopefactory.Object, _configuration, courseRepo);

            var trainingService = new TrainingService(_Mediator, training_service, course_service);
            TraineeID traineeId = new TraineeID("e1a1a875-4aea-467d-a1dc-b1aeff0b0257"); //must be legit
            Trainee trainee = trainingService.GetTraineeByIdAsync(traineeId).Result.Value;
            var courseId = new CourseID("C1400"); //must be legit
            var dateToCheck = DateTime.Now;
            var cancellationToken = CancellationToken.None;

            
            //// Act
            var result = await trainingService.CheckForPreviousTrainingOnDateAsync(trainee, courseId, dateToCheck, cancellationToken);

            //// Assert
            Assert.False(result.Value); //no training found 

            
        }
        
        [Fact]
        public async Task AddTrainingLogEntryAsync_ReturnsSuccess()
        {
            // Arrange
            Initialize();

            var trainingRepo = new TrainingRepository(_MockTrainingRepoLogger.Object, _MockLoggerSupport.Object, _configuration);
            var courseRepo = new CourseRepository(_MockCourseRepoLogger.Object, _MockLoggerSupport.Object, _configuration);
            var training_service = new TrainingDataService(_MockTrainingServicelogger.Object, _MockScopefactory.Object, _configuration, trainingRepo);
            var course_service = new CourseDataService(_MockCourseServicelogger.Object, _MockScopefactory.Object, _configuration, courseRepo);

            var trainingService = new TrainingService(_Mediator, training_service, course_service);

            TraineeID traineeID = new("TRAINEE1");
            TrainingLogEntryID tleID = new(Guid.NewGuid().ToString());
            TrainingLogEntry traininglogEntry = new(tleID);
            traininglogEntry.TraineeId = traineeID;
            traininglogEntry.CourseId = new CourseID("COURSE1");
            traininglogEntry.LessonId = new LessonID("LESSON1");
            traininglogEntry.LessonQuizId = new ("LESSONQUIZ1");
            traininglogEntry.QuestionPoolId = new ("QUESTIONPOOLID1");
            traininglogEntry.QuestionId = new ("QUESTION1");
            traininglogEntry.AnswerId = new("ANSWR1");
            traininglogEntry.RecordedAt = DateTime.Now;
            traininglogEntry.IsCorrect = true;

        
        
        // Act
            var result = await trainingService.AddTrainingLogEntryAsync(traininglogEntry, ct);

        // Assert
           Assert.True(result.Value);
        
        }

        [Fact]
        public async Task GetCoursesAsync_ReturnsCourseList()
        {
            // Arrange
            var traineeId = new TraineeID("e1a1a875-4aea-467d-a1dc-b1aeff0b0257"); //must be legit


            var trainingRepo = new TrainingRepository(_MockTrainingRepoLogger.Object, _MockLoggerSupport.Object, _configuration);
            var courseRepo = new CourseRepository(_MockCourseRepoLogger.Object, _MockLoggerSupport.Object, _configuration);
            var training_service = new TrainingDataService(_MockTrainingServicelogger.Object, _MockScopefactory.Object, _configuration, trainingRepo);
            var course_service = new CourseDataService(_MockCourseServicelogger.Object, _MockScopefactory.Object, _configuration, courseRepo);

            var trainingService = new TrainingService(_Mediator, training_service, course_service);

            // Act
            var result = await trainingService.GetCoursesAsync(traineeId, ct);

            // Assert
            Assert.NotNull(result.Value);
            Assert.True(result.Value.Count() > 0);
            
        }

        [Fact]
        public async Task GetCourseCodesAsync_ReturnsCorrectCodes_WhenNoPreviousTrainingFound()
        {
            // Arrange
            Initialize();

            var trainingRepo = new TrainingRepository(_MockTrainingRepoLogger.Object, _MockLoggerSupport.Object, _configuration);
            var courseRepo = new CourseRepository(_MockCourseRepoLogger.Object, _MockLoggerSupport.Object, _configuration);
            var training_service = new TrainingDataService(_MockTrainingServicelogger.Object, _MockScopefactory.Object, _configuration, trainingRepo);
            var course_service = new CourseDataService(_MockCourseServicelogger.Object, _MockScopefactory.Object, _configuration, courseRepo);

            var trainingService = new TrainingService(_Mediator, training_service, course_service);
            TraineeID traineeId = new TraineeID("e1a1a875-4aea-467d-a1dc-b1aeff0b0257"); //must be legit
            Trainee trainee = trainingService.GetTraineeByIdAsync(traineeId).Result.Value;

            string[] expectedCourseCodes = { "BLUE_C2120", "GRAY_C2110", "PURPLE_C2120", "RED_C2130" };
            bool sidaonly = true;
            // Act
            var result = await trainingService.GetCourseCodesAsync(trainee, sidaonly, ct);

            // Assert
            Assert.NotNull(result.Value);
            Assert.Equal(4, result.Value.Count); // Ensure course count is correct 
            Assert.All(expectedCourseCodes, code => Assert.Contains(code, result.Value)); // Ensure all expected codes are present
            Assert.Equal(expectedCourseCodes.OrderBy(c => c), result.Value.OrderBy(c => c)); // Ensure correct order (optional)

        }

        [Fact]
        public async Task GetCourseCodesAsync_ReturnsCorrectCodes_WhenPreviousTrainingFound()
        {
            // Arrange
            Initialize();

            var trainingRepo = new TrainingRepository(_MockTrainingRepoLogger.Object, _MockLoggerSupport.Object, _configuration);
            var courseRepo = new CourseRepository(_MockCourseRepoLogger.Object, _MockLoggerSupport.Object, _configuration);
            var training_service = new TrainingDataService(_MockTrainingServicelogger.Object, _MockScopefactory.Object, _configuration, trainingRepo);
            var course_service = new CourseDataService(_MockCourseServicelogger.Object, _MockScopefactory.Object, _configuration, courseRepo);

            var trainingService = new TrainingService(_Mediator, training_service, course_service);
            //e1a1a875-4aea-467d-a1dc-b1aeff0b0257
            //a00834f5-bfb8-4d78-80d5-f3f2d76e5b8d
            TraineeID traineeId = new TraineeID("e1a1a875-4aea-467d-a1dc-b1aeff0b0257"); //must be legit 
            Trainee trainee = trainingService.GetTraineeByIdAsync(traineeId).Result.Value;
            var sidaOnly = false;
                       

            // Act
            var result = await trainingService.GetCourseCodesAsync(trainee, sidaOnly, ct);

            // Assert

            Assert.NotNull(result.Value);
            Assert.True(result.Value.Count == 11); // there should only be 10
        }

    }
}
