namespace CBT3_Domain.Entities;

public sealed class LessonQuiz : BaseAuditableEntity
{
    public LessonQuiz(LessonQuizID id) : base(id, "SYSTEM", DateTime.UtcNow) { }
    public LessonID LessonID { get; set; }
    public List<QuestionPool> QuestionPools { get; set; } = new();
    public int AttemptsAllowed { get; set; }
}
