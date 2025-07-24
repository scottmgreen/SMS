using CBT3_Application.Interfaces;
using CBT3_Application.Services;

using CBT3_Domain.Entities;
using CBT3_Domain.Errors;

using Moq;

namespace CBT3_UnitTests.Application.Services;

public class CourseMachineServiceTests
{

    Mock<IMediator> _MockMediator = new Mock<IMediator>();
    Mock<IMessenger> _MockEventAggregator = new Mock<IMessenger>();
    CourseMachine _coursemachine; 




     public CourseMachineServiceTests()
    {
        // Arrange

        _coursemachine = new CourseMachine(_MockMediator.Object, _MockEventAggregator.Object);

        // Act
        //var result = service.IsPaused;

        // Assert
        //Assert.True(result);
        //Assert.Fail("This test needs an implementation");
    }

    [Fact()]
    public void InitializeMachine_Valid()
    {
        // Arrange

        //var service = new CourseMachine(_MockMediator.Object, _MockEventAggregator.Object);
        TraineeID traineeID = new("1234567");
        CourseID courseID = new("Course1234");
        LessonID lessonID = new("lesson1");
        // Act
        Trainee trainee = new(traineeID);
        Course course = new(courseID);
        Lesson lesson = new(lessonID);
        course.Lessons.Add(lesson);
        var result = _coursemachine.InitializeMachine(trainee, course);



        Assert.True(result.IsSuccess);
       // Assert.True(result.IsSuccess);
        
       

    }
    [Fact()]
    public void InitializeMachineNullTrainee_InValid()
    {
        // Arrange

        //var service = new CourseMachine(_MockMediator.Object, _MockEventAggregator.Object);

        Trainee trainee = null;//  new("1234567");
        CourseID courseID = new("Course1234");
        LessonID lessonID = new("lesson1");
        Course course = new(courseID);
        Lesson lesson = new(lessonID);
        // Act

        var result= _coursemachine.InitializeMachine(trainee, course);

        // Assert
        Assert.Equal(DomainErrors.TraineeError.NullOrEmpty, result.Error);
        Assert.True(result.IsFailure);



    }
    [Fact()]
    public void InitializeMachineNullCourse_InValid()
    {
        // Arrange

        //var service = new CourseMachine(_MockMediator.Object, _MockEventAggregator.Object);
        TraineeID traineeID = new("1234567");
        Trainee trainee = new(traineeID);
        Course course = null;//  new("Course1234");
        //Lesson lesson = new("lesson1");
        
        // Act
        var result = _coursemachine.InitializeMachine(trainee, course);

        // Assert
        Assert.Equal(DomainErrors.CourseError.NullOrEmpty, result.Error);
        Assert.True(result.IsFailure);



    }
    [Fact()]
    public void StartTest_Valid()
    {
        // Arrange

        //var service = new CourseMachine(_MockMediator.Object, _MockEventAggregator.Object);
        TraineeID traineeID = new("1234567");
        CourseID courseID = new("Course1234");
        LessonID lessonID = new("lesson1");
        Trainee trainee = new(traineeID);
        Course course = new(courseID);
        Lesson lesson = new(lessonID);
        course.Lessons.Add(lesson);

        _coursemachine.InitializeMachine(trainee, course);

        // Act
        var result = _coursemachine.Start();

        // Assert
        Assert.True(result.IsSuccess);
    }
    [Fact()]
    public void StartTest_InValid()
    {
        // Arrange

        //var service = new CourseMachine(_MockMediator.Object, _MockEventAggregator.Object);
        TraineeID traineeID = new("1234567");
        CourseID courseID = new("Course1234");
        Trainee trainee = new(traineeID);
        Course course = new(courseID);

        //NO Lessons!

        _coursemachine.InitializeMachine(trainee, course);

        // Act
        var result = _coursemachine.Start();

        // Assert
        Assert.Equal(DomainErrors.MachineError.CurrentStateNullOrEmpty, result.Error);
        Assert.True(result.IsFailure);
    }
    //[Fact()]
    //public void StateChangeTest()
    //{
    //    // Arrange
    //    var mediatorMock = new Mock<IMediator>();
    //    var eventAggregatorMock = new Mock<IEventAggregator>();
    //    var service = new CourseMachine(mediatorMock.Object, eventAggregatorMock.Object);

    //    // Act
    //    //var result = service.IsPaused;

    //    // Assert
    //    Assert.Fail("This test needs an implementation");
    //}

    //[Fact()]
    //public void MachinePauseTest()
    //{
    //    // Arrange
    //    var mediatorMock = new Mock<IMediator>();
    //    var eventAggregatorMock = new Mock<IEventAggregator>();
    //    var service = new CourseMachine(mediatorMock.Object, eventAggregatorMock.Object);

    //    // Act
    //    //var result = service.IsPaused;

    //    // Assert
    //    Assert.Fail("This test needs an implementation");
    //}

    //[Fact()]
    //public void MachineResumeTest()
    //{
    //    // Arrange
    //    var mediatorMock = new Mock<IMediator>();
    //    var eventAggregatorMock = new Mock<IEventAggregator>();
    //    var service = new CourseMachine(mediatorMock.Object, eventAggregatorMock.Object);

    //    // Act
    //    //var result = service.IsPaused;

    //    // Assert
    //    Assert.Fail("This test needs an implementation");
    //}
}