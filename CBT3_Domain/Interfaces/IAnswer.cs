namespace CBT3_Domain.Interfaces;

public interface IAnswer
{
    string? AnswerImage { get; set; }
    string? AnswerText { get; set; }
    bool? IsCorrect { get; set; }
    bool IsSelected { get; set; }
    string? QuestionID { get; set; }
}