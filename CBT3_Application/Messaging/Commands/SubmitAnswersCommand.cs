namespace CBT3_Application.Messaging;

public class SubmitAnswersCommand : BaseCommandBundle, IRequest<Result<Question>>
{
    public SubmitAnswersCommand(List<Answer> answers)
    {
        Answers = answers;
    }
    public List<Answer> Answers { get; init; }

}