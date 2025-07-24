
namespace CBT3_Application.Messaging.CommandHandlers;
public class SubmitAnswersCommandHandler : BaseCommandBundle, IRequestHandler<SubmitAnswersCommand, Result<Question>>
{
    private readonly LessonQuizService _quizService;
    public SubmitAnswersCommandHandler(LessonQuizService dataService)
    {
        _quizService = dataService;
    }

    public async Task<Result<Question>> HandleAsync(SubmitAnswersCommand request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Result<Question> result = _quizService.SubmitAnswers(request.Answers);
        return result;
    }


}

