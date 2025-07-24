using CBT3_Domain.Enums;
using CBT3_Application.Interfaces;
using CBT3_Application.Services;

using CBT3_Domain.Entities;

using Moq;
using System.Drawing;
using CBT3_Domain.Common;
using CBT3_Infrastructure.Interfaces;
using CBT3_Infrastructure.Repositories;
using CBT3_Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CBT3_Application.Configuration;
using CBT3_Infrastructure.Configuration;
using CBT3_Shared.Configuration;

namespace CBT3_UnitTests.Application.Services
{
    public class LessonQuizServiceTests
    {
        IConfigurationRoot _configuration;
        IMediator _Mediator;
        Mock<IMessenger> _MockEventAggregator = new Mock<IMessenger>();
        Mock<IServiceScopeFactory> _MockScopefactory = new Mock<IServiceScopeFactory>();
        Mock<ILogSupport> _MockLoggerSupport = new Mock<ILogSupport>();
        //loggers//
        Mock<ILogger<TrainingDataService>> _MockTrainingServicelogger = new Mock<ILogger<TrainingDataService>>();
        Mock<ILogger<CourseDataService>> _MockCourseServicelogger = new Mock<ILogger<CourseDataService>>();
        Mock<ILogger<TrainingRepository>> _MockTrainingRepoLogger = new Mock<ILogger<TrainingRepository>>();
        Mock<ILogger<CourseRepository>> _MockCourseRepoLogger = new Mock<ILogger<CourseRepository>>();

        public LessonQuizServiceTests()
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

        [Fact]
        public void LessonQuiz_IsQuizComplete_WithLessonLoaded()
        {
            Initialize();
            // Arrange

            var service = new LessonQuizService(_Mediator, _MockEventAggregator.Object);
            TraineeID id = new("1234567");
            var trainee = new Trainee(id);
            CourseID courseId = new("123");
            LessonQuizID qid = new("qiz");
            var lessonQuiz = new LessonQuiz(qid);
            QuestionID qid1 = new("Q1");
            QuestionID qid2 = new("Q2");
            QuestionID qid3 = new("Q3");
            QuestionID qid4 = new("Q4"); 
            Question q1 = new Question(qid1);
            Question q2 = new Question(qid2);
            Question q3 = new Question(qid3);
            Question q4 = new Question(qid4);

            q1.QuestionType = QuestionType.TrueFalse;
            q2.QuestionType = QuestionType.TrueFalse;
            q3.QuestionType = QuestionType.TrueFalse;
            q4.QuestionType = QuestionType.TrueFalse;

            AnswerID anid1 = new("AN1");
            AnswerID anid2 = new("AN2");
            AnswerID anid3 = new("AN3");
            AnswerID anid4 = new("AN4");


            Answer an1 = new(anid1);
            Answer an2 = new(anid2);
            Answer an3 = new(anid3);
            Answer an4 = new(anid4);
            an1.IsCorrect = true;
            an2.IsCorrect = false;
            an3.IsCorrect = true;
            an4.IsCorrect = false;

            q1.Answers.Add(an1);
            q2.Answers.Add(an2);
            q3.Answers.Add(an3);
            q4.Answers.Add(an4);
            QuestionPoolID qpid1 = new("QP1");
            QuestionPool questionpool1 = new QuestionPool(qpid1);
            questionpool1.Questions.Add(q1);
            questionpool1.Questions.Add(q2);


            QuestionPoolID qpid2 = new("QP2");
            QuestionPool questionpool2 = new QuestionPool(qpid2);
            questionpool2.Questions.Add(q3);
            questionpool2.Questions.Add(q4);

            lessonQuiz.QuestionPools.Add(questionpool1);
            lessonQuiz.QuestionPools.Add(questionpool2);

            // Act
            service.InitializeQuiz(trainee, courseId, lessonQuiz);

            // Assert
            //Assert.Equal(trainee, service._trainee);
            //Assert.Equal(courseId, service._courseId);
            //Assert.Equal(lessonQuiz, service._lessonQuiz);
            var result = service.IsQuizComplete();
            Assert.Equal(result, false);

        }

        
        [Fact]
        public void LessonQuiz_IsQuizComplete_WithoutLessonLoaded()
        {
            Initialize();
            // Arrange
            var service = new LessonQuizService(_Mediator, _MockEventAggregator.Object);

            // Act
            var result = service.IsQuizComplete();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void InitializeQuiz_SetsUpQuizCorrectly()
        {
            Initialize();
            // Arrange
            var trainingRepo = new TrainingRepository(_MockTrainingRepoLogger.Object, _MockLoggerSupport.Object, _configuration);
            var courseRepo = new CourseRepository(_MockCourseRepoLogger.Object, _MockLoggerSupport.Object, _configuration);

            var training_service = new TrainingDataService(_MockTrainingServicelogger.Object, _MockScopefactory.Object, _configuration, trainingRepo);
            var course_service = new CourseDataService(_MockCourseServicelogger.Object, _MockScopefactory.Object, _configuration, courseRepo);

            var trainingService = new TrainingService(_Mediator, training_service, course_service);

            var service = new LessonQuizService(_Mediator, _MockEventAggregator.Object);
            
            var trainee = trainingService.GetTraineeByIdAsync(new TraineeID("e1a1a875-4aea-467d-a1dc-b1aeff0b0257")).Result.Value; // Assuming Trainee has an Id property
            
            var courseId = new CourseID("C1400"); // Assuming Course has an Id property

            var course = trainingService.GetCourseByIdAsync(courseId).Result.Value;
            var lessonWithQuizAndQuestions = course.Lessons
                .FirstOrDefault(x => x.LessonQuiz != null && x.LessonQuiz.QuestionPools.Any());

            // Act
            service.InitializeQuiz(trainee, courseId, lessonWithQuizAndQuestions.LessonQuiz);

            // Assert
            Assert.Equal(0, service.Score);
            Assert.Equal(false, service.IsQuizComplete());
            Assert.Null(service.CurrentQuestion);
            Assert.Null(service.CurrentPoolId);
        }
        [Fact]
        public void SubmitAnswer_CorrectAnswer_IncrementsScoreAndLoadsNextQuestion()
        {
            
            Initialize();
            // Arrange
            var trainingRepo = new TrainingRepository(_MockTrainingRepoLogger.Object, _MockLoggerSupport.Object, _configuration);
            var courseRepo = new CourseRepository(_MockCourseRepoLogger.Object, _MockLoggerSupport.Object, _configuration);

            var training_service = new TrainingDataService(_MockTrainingServicelogger.Object, _MockScopefactory.Object, _configuration, trainingRepo);
            var course_service = new CourseDataService(_MockCourseServicelogger.Object, _MockScopefactory.Object, _configuration, courseRepo);

            var trainingService = new TrainingService(_Mediator, training_service, course_service);

            var service = new LessonQuizService(_Mediator, _MockEventAggregator.Object);

            var trainee = trainingService.GetTraineeByIdAsync(new TraineeID("e1a1a875-4aea-467d-a1dc-b1aeff0b0257")).Result.Value; // Assuming Trainee has an Id property

            var courseId = new CourseID("C1400"); // Assuming Course has an Id property

            var course = trainingService.GetCourseByIdAsync(courseId).Result.Value;
            var lessonWithQuizAndQuestions = course.Lessons
                .FirstOrDefault(x => x.LessonQuiz != null && x.LessonQuiz.QuestionPools.Any());
            LessonQuiz quiz = lessonWithQuizAndQuestions.LessonQuiz;

            // Act
            service.InitializeQuiz(trainee, courseId, quiz);

            var question = service.GetNextQuestion(true);

            var answer = question.Answers.FirstOrDefault();

            

            // Act
            var result = service.SubmitAnswer(answer);

            // Assert
            Assert.Equal(1, service.Score);
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);  // Verify next question is selected correctly
        }

        //[Fact]
        //public void SubmitAnswer_IncorrectAnswer_DoesNotIncrementScore()
        //{
        //    // Arrange
        //    var answer = new Answer { Id = 1, IsCorrect = false };
        //    var trainee = new Trainee { Id = 1 };
        //    var courseId = new CourseID(1);
        //    var lessonQuiz = new LessonQuiz
        //    {
        //        Id = new LessonQuizID(1),
        //        QuestionPools = new List<QuestionPool>
        //{
        //    new QuestionPool
        //    {
        //        Id = new QuestionPoolID(1),
        //        Questions = new List<Question>
        //        {
        //            new Question { Id = new QuestionID(1) }
        //        }
        //    }
        //}
        //    };
        //    _quizService.InitializeQuiz(trainee, courseId, lessonQuiz);

        //    // Act
        //    var result = _quizService.SubmitAnswer(answer);

        //    // Assert
        //    Assert.Equal(0, _quizService.Score); // Score should not change
        //    Assert.NotNull(result);  // A question should be returned
        //    Assert.Equal(1, result.Id.Value);  // Verify next question is selected correctly
        //}

        //[Fact]
        //public void IsQuizComplete_QuizCompleted_ReturnsTrue()
        //{
        //    // Arrange
        //    var lessonQuiz = new LessonQuiz
        //    {
        //        Id = new LessonQuizID(1),
        //        QuestionPools = new List<QuestionPool>
        //{
        //    new QuestionPool
        //    {
        //        Id = new QuestionPoolID(1),
        //        Questions = new List<Question>
        //        {
        //            new Question { Id = new QuestionID(1) }
        //        }
        //    }
        //}
        //    };
        //    _quizService.InitializeQuiz(new Trainee { Id = 1 }, new CourseID(1), lessonQuiz);

        //    // Act
        //    _quizService.GetNextQuestion(true); // Answer the first question
        //    var result = _quizService.IsQuizComplete();

        //    // Assert
        //    Assert.True(result); // The quiz should be complete
        //}

    }
}
