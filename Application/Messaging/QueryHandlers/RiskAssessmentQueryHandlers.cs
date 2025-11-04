using Microsoft.Extensions.Logging;
using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// RISK ASSESSMENT QUERY HANDLERS
// =============================================

public class GetRiskAssessmentByIdQueryHandler : BaseQueryBundle, IRequestHandler<GetRiskAssessmentByIdQuery, Result<RiskAssessment>>
{
    private readonly RiskAssessmentDataService _riskAssessmentDataService;
    private readonly ILogger<GetRiskAssessmentByIdQueryHandler> _logger;

    public GetRiskAssessmentByIdQueryHandler(RiskAssessmentDataService riskAssessmentDataService, ILogger<GetRiskAssessmentByIdQueryHandler> logger)
    {
        _riskAssessmentDataService = riskAssessmentDataService ?? throw new ArgumentNullException(nameof(riskAssessmentDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAssessment>> HandleAsync(GetRiskAssessmentByIdQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetRiskAssessmentByIdQuery for ID: {Id}", request.RiskAssessmentId);
            var result = await _riskAssessmentDataService.GetRiskAssessmentByIdAsync(request.RiskAssessmentId, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetRiskAssessmentByIdQuery for ID: {Id}", request.RiskAssessmentId);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NotFound);
        }
    }
}

public class GetAllRiskAssessmentsQueryHandler : BaseQueryBundle, IRequestHandler<GetAllRiskAssessmentsQuery, Result<List<RiskAssessment>>>
{
    private readonly RiskAssessmentDataService _riskAssessmentDataService;
    private readonly ILogger<GetAllRiskAssessmentsQueryHandler> _logger;

    public GetAllRiskAssessmentsQueryHandler(RiskAssessmentDataService riskAssessmentDataService, ILogger<GetAllRiskAssessmentsQueryHandler> logger)
    {
        _riskAssessmentDataService = riskAssessmentDataService ?? throw new ArgumentNullException(nameof(riskAssessmentDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<RiskAssessment>>> HandleAsync(GetAllRiskAssessmentsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllRiskAssessmentsQuery");
            var result = await _riskAssessmentDataService.GetAllRiskAssessmentsAsync(ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetAllRiskAssessmentsQuery");
            return Result<List<RiskAssessment>>.Failure<List<RiskAssessment>>(DomainErrors.RiskAssessmentError.NullOrEmpty);
        }
    }
}