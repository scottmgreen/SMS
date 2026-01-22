using Microsoft.Extensions.Logging;

using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// RISK ANALYSIS QUERY HANDLERS
// =============================================

public class GetRiskAnalysisByIdQueryHandler : BaseQueryBundle, IRequestHandler<GetRiskAnalysisByIdQuery, Result<RiskAnalysis>>
{
    private readonly RiskAnalysisDataService _riskAnalysisDataService;
    private readonly ILogger<GetRiskAnalysisByIdQueryHandler> _logger;

    public GetRiskAnalysisByIdQueryHandler(RiskAnalysisDataService riskAnalysisDataService, ILogger<GetRiskAnalysisByIdQueryHandler> logger)
    {
        _riskAnalysisDataService = riskAnalysisDataService ?? throw new ArgumentNullException(nameof(riskAnalysisDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAnalysis>> HandleAsync(GetRiskAnalysisByIdQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetRiskAnalysisByIdQuery for ID: {Id}", request.RiskAnalysisId);
            var result = await _riskAnalysisDataService.GetRiskAnalysisByIdAsync(request.RiskAnalysisId, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetRiskAnalysisByIdQuery for ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.NotFound);
        }
    }
}

public class GetRiskAnalysisByHazardIdQueryHandler : BaseQueryBundle, IRequestHandler<GetRiskAnalysisByHazardIdQuery, Result<RiskAnalysis>>
{
    private readonly RiskAnalysisDataService _riskAnalysisDataService;
    private readonly ILogger<GetRiskAnalysisByHazardIdQueryHandler> _logger;

    public GetRiskAnalysisByHazardIdQueryHandler(RiskAnalysisDataService riskAnalysisDataService, ILogger<GetRiskAnalysisByHazardIdQueryHandler> logger)
    {
        _riskAnalysisDataService = riskAnalysisDataService ?? throw new ArgumentNullException(nameof(riskAnalysisDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAnalysis>> HandleAsync(GetRiskAnalysisByHazardIdQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetRiskAnalysisByIdQuery for ID: {Id}", request.HazardId);
            var result = await _riskAnalysisDataService.GetRiskAnalysisByHazardIdAsync(request.HazardId, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetRiskAnalysisByIdQuery for ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.NotFound);
        }
    }
}

public class GetAllRiskAnalysisQueryHandler : BaseQueryBundle, IRequestHandler<GetAllRiskAnalysisQuery, Result<List<RiskAnalysis>>>
{
    private readonly RiskAnalysisDataService _riskAnalysisDataService;
    private readonly ILogger<GetAllRiskAnalysisQueryHandler> _logger;

    public GetAllRiskAnalysisQueryHandler(RiskAnalysisDataService riskAnalysisDataService, ILogger<GetAllRiskAnalysisQueryHandler> logger)
    {
        _riskAnalysisDataService = riskAnalysisDataService ?? throw new ArgumentNullException(nameof(riskAnalysisDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<RiskAnalysis>>> HandleAsync(GetAllRiskAnalysisQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllRiskAnalysisQuery");
            var result = await _riskAnalysisDataService.GetAllRiskAnalysisAsync(ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetAllRiskAnalysisQuery", ApplicationEventIds.Error, ex);
            return Result<List<RiskAnalysis>>.Failure<List<RiskAnalysis>>(DomainErrors.RiskAnalysisError.NullOrEmpty);
        }
    }
}