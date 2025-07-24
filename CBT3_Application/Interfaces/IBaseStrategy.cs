namespace CBT3_Application.Interfaces;

public interface IBaseStrategy
{
    void ExecuteStrategy(LessonPage? lessonpage, Lesson lesson);
}
