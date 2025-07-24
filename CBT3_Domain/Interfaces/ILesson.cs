namespace CBT3_Domain.Interfaces;

public interface ILesson
{
    string CourseID { get; set; }
    string LessonName { get; set; }
    int LessonOrder { get; set; }
    List<LessonPage> LessonPages { get; set; }
    LessonQuiz LessonQuiz { get; set; }
}