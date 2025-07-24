
namespace CBT3_Application.Messaging.CommandHandlers;


    public class SubmitAnswerCommandHandler : BaseCommandBundle,IRequestHandler<SubmitAnswerCommand, Result<Question>> 
    {
    private readonly LessonQuizService _quizService;
        public SubmitAnswerCommandHandler(LessonQuizService dataService)
        {
            _quizService = dataService;
        }
       
        public async Task<Result<Question>> HandleAsync(SubmitAnswerCommand request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Result<Question> result =  _quizService.SubmitAnswer(request.Answer);
            return result;
        }

    
    }

