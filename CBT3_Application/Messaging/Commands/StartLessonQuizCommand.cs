using CBT3_Domain.Entities;

using static CBT3_Domain.Errors.DomainErrors;

namespace CBT3_Application.Messaging.Commands;

public class StartLessonQuizCommand : BaseCommandBundle, IRequest<Trainee>, IRequest<Result<bool>>
{
    public StartLessonQuizCommand()
    {
        //TraineeId = traineeId;
        //CourseId = courseId;
        //LessonQuizId = lessonQuizId;
    }
    public TraineeID TraineeId { get; init; }
    public LessonQuizID LessonQuizId { get; init; }
    public CourseID CourseId { get; init; }
    

}
