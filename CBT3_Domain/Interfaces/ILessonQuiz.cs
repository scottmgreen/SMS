namespace CBT3_Domain.Interfaces;

public interface ILessonQuiz
{
    int AttemptsAllowed { get; set; }
    string LessonID { get; set; }
    List<QuestionPool> QuestionPools { get; set; }
}