using Microsoft.Extensions.Logging;

using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// INTERVIEW QUERY HANDLERS
// =============================================

public class GetInterviewByCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetInterviewByCodeQuery, Result<Interview>>
{
    private readonly InterviewDataService _interviewDataService;
    private readonly ILogger<GetInterviewByCodeQueryHandler> _logger;

    public GetInterviewByCodeQueryHandler(InterviewDataService interviewDataService, ILogger<GetInterviewByCodeQueryHandler> logger)
    {
        _interviewDataService = interviewDataService ?? throw new ArgumentNullException(nameof(interviewDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Interview>> HandleAsync(GetInterviewByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetInterviewByCodeQuery for Code: {Code}", request.InterviewId);
            var result = await _interviewDataService.GetInterviewByCodeAsync(request.InterviewId, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetInterviewByCodeQuery for Code: {Code}", ApplicationEventIds.Error, ex);
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
            _logger.LogApplicationError("Error processing GetAllInterviewsQuery", ApplicationEventIds.Error, ex);
            return Result<List<Interview>>.Failure<List<Interview>>(DomainErrors.InterviewError.NullOrEmpty);
        }
    }
}