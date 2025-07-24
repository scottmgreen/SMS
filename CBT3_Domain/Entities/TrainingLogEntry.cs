
namespace CBT3_Domain.Entities;

public class TrainingLogEntry:BaseAuditableEntity
{
    public TrainingLogEntry(TrainingLogEntryID id) : base(id, "SYSTEM", DateTime.UtcNow) { }
    public TraineeID TraineeId { get; set; }
    public CourseID CourseId { get; set; }
    public LessonID LessonId { get; set; }
    public LessonQuizID LessonQuizId { get; set; }
    public QuestionPoolID QuestionPoolId { get; set; }
    public QuestionID QuestionId { get; set; }
    public AnswerID AnswerId { get; set; }
    public bool IsCorrect { get; set; }
    public DateTime RecordedAt { get; set; }
}
