using Microsoft.Extensions.Logging;
using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// INTERVIEW QUERY HANDLERS
// =============================================

public class GetInterviewByIdQueryHandler : BaseQueryBundle, IRequestHandler<GetInterviewByIdQuery, Result<Interview>>
{
    private readonly InterviewDataService _interviewDataService;
    private readonly ILogger<GetInterviewByIdQueryHandler> _logger;

    public GetInterviewByIdQueryHandler(InterviewDataService interviewDataService, ILogger<GetInterviewByIdQueryHandler> logger)
    {
        _interviewDataService = interviewDataService ?? throw new ArgumentNullException(nameof(interviewDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Interview>> HandleAsync(GetInterviewByIdQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetInterviewByIdQuery for ID: {Id}", request.InterviewId);
            var result = await _interviewDataService.GetInterviewByIdAsync(request.InterviewId, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetInterviewByIdQuery for ID: {Id}", request.InterviewId);
            return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.NotFound);
        }
    }
}

public class GetAllInterviewsQueryHandler : BaseQueryBundle, IRequestHandler<GetAllInterviewsQuery, Result<List<Interview>>>
{
    private readonly InterviewDataService _interviewDataService;
    private readonly ILogger<GetAllInterviewsQueryHandler> _logger;

    public GetAllInterviewsQueryHandler(InterviewDataService interviewDataService, ILogger<GetAllInterviewsQueryHandler> logger)
    {
        _interviewDataService = interviewDataService ?? throw new ArgumentNullException(nameof(interviewDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<Interview>>> HandleAsync(GetAllInterviewsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllInterviewsQuery");
            var result = await _interviewDataService.GetAllInterviewsAsync(ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetAllInterviewsQuery");
            return Result<List<Interview>>.Failure<List<Interview>>(DomainErrors.InterviewError.NullOrEmpty);
        }
    }
}