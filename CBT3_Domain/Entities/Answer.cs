using System.Runtime.CompilerServices;

namespace CBT3_Domain.Entities;

public sealed class Answer : BaseEntity
{
    
    public Answer(AnswerID id) : base(id) { }
    public QuestionID? QuestionID { get; set; }
    public string? AnswerText { get; set; }
    public string? AnswerImage { get; set; }
    public bool? IsCorrect { get; set; }
    public bool IsSelected { get; set; }
}
