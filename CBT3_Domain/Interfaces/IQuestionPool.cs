namespace CBT3_Domain.Interfaces;

public interface IQuestionPool
{
    string JumpBackEnd { get; set; }
    string JumpBackFilename { get; set; }
    string JumpBackStart { get; set; }
    List<Question> Questions { get; set; }
}