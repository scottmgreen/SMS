namespace CBT3_Domain.Entities;

public sealed class Lesson : BaseAuditableEntity
{
    public Lesson(LessonID id) : base(id, "SYSTEM", DateTime.UtcNow) { }
    public CourseID CourseID { get; set; }
    public string LessonName { get; set; }
    public int LessonOrder { get; set; }
    public List<LessonPage> LessonPages { get; set; }
    public LessonQuiz LessonQuiz { get; set; }
}
