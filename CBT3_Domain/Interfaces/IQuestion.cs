namespace CBT3_Domain.Interfaces;

public interface IQuestion
{
    List<Answer> Answers { get; set; }
    string QuestionImage { get; set; }
    string QuestionPoolID { get; set; }
    string QuestionText { get; set; }
    QuestionType QuestionType { get; set; }
}