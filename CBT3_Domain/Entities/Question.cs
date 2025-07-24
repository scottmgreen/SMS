namespace CBT3_Domain.Entities;

public class Question : BaseEntity
{
    public Question(QuestionID id) : base(id) { }
    public QuestionPoolID QuestionPoolID { get; set; }
    public string QuestionText { get; set; }
    public string QuestionImage { get; set; }
    public QuestionType QuestionType { get; set; }
    public List<Answer> Answers { get; set; } = new();

}
