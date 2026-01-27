using Microsoft.Extensions.Logging;

using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// RISK ANALYSIS QUERY HANDLERS
// =============================================

public class GetRiskAnalysisByCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetRiskAnalysisByCodeQuery, Result<RiskAnalysis>>
{
    private readonly RiskAnalysisDataService _riskAnalysisDataService;
    private readonly ILogger<GetRiskAnalysisByCodeQueryHandler> _logger;

    public GetRiskAnalysisByCodeQueryHandler(RiskAnalysisDataService riskAnalysisDataService, ILogger<GetRiskAnalysisByCodeQueryHandler> logger)
    {
        _riskAnalysisDataService = riskAnalysisDataService ?? throw new ArgumentNullException(nameof(riskAnalysisDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAnalysis>> HandleAsync(GetRiskAnalysisByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetRiskAnalysisByCodeQuery for Code: {Code}", request.RiskAnalysisCode);
            var result = await _riskAnalysisDataService.GetRiskAnalysisByCodeAsync(request.RiskAnalysisCode, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetRiskAnalysisByCodeQuery for Code: {Code}", ApplicationEventIds.Error, ex);
            return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.NotFound);
        }
    }
}

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
            var result = await _riskAnalysisDataService.GetRiskAnalysisByCodeAsync(request.RiskAnalysisId, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetRiskAnalysisByIdQuery for ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.NotFound);
        }
    }
}

public class GetRiskAnalysisByHazardCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetRiskAnalysisByHazardCodeQuery, Result<RiskAnalysis>>
{
    private readonly RiskAnalysisDataService _riskAnalysisDataService;
    private readonly ILogger<GetRiskAnalysisByHazardCodeQueryHandler> _logger;

    public GetRiskAnalysisByHazardCodeQueryHandler(RiskAnalysisDataService riskAnalysisDataService, ILogger<GetRiskAnalysisByHazardCodeQueryHandler> logger)
    {
        _riskAnalysisDataService = riskAnalysisDataService ?? throw new ArgumentNullException(nameof(riskAnalysisDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAnalysis>> HandleAsync(GetRiskAnalysisByHazardCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetRiskAnalysisByHazardCodeQuery for Code: {Code}", request.HazardCode);
            var result = await _riskAnalysisDataService.GetRiskAnalysisByHazardCodeAsync(request.HazardCode, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetRiskAnalysisByHazardCodeQuery for Code: {Code}", ApplicationEventIds.Error, ex);
            return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.NotFound);
        }
    }
}

public class GetRiskAnalysisByHazardAndAssessmentQueryHandler : BaseQueryBundle, IRequestHandler<GetRiskAnalysisByHazardAndAssessmentQuery, Result<RiskAnalysis>>
{
    private readonly RiskAnalysisDataService _riskAnalysisDataService;
    private readonly ILogger<GetRiskAnalysisByHazardAndAssessmentQueryHandler> _logger;

    public GetRiskAnalysisByHazardAndAssessmentQueryHandler(RiskAnalysisDataService riskAnalysisDataService, ILogger<GetRiskAnalysisByHazardAndAssessmentQueryHandler> logger)
    {
        _riskAnalysisDataService = riskAnalysisDataService ?? throw new ArgumentNullException(nameof(riskAnalysisDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAnalysis>> HandleAsync(GetRiskAnalysisByHazardAndAssessmentQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("🔍 Processing GetRiskAnalysisByHazardAndAssessmentQuery for Hazard: {HazardCode}, Assessment: {AssessmentCode}",   request.HazardCode, request.RiskAssessmentCode);

            // Use LINQ filtering approach to eliminate database round trip
            var getAllResult = await _riskAnalysisDataService.GetAllRiskAnalysisAsync(ct).ConfigureAwait(false);
            
            if (!getAllResult.IsSuccess || getAllResult.Value == null)
            {
                _logger.LogWarning("❌ Failed to get all RiskAnalysis records");
                return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.NotFound);
            }

            _logger.LogInformation("📊 Searching through {Count} RiskAnalysis records", getAllResult.Value.Count);

            // Filter in-memory using LINQ
            var matchingAnalysis = getAllResult.Value
                .Where(ra => 
                    string.Equals(ra.HazardCode?.Trim(), request.HazardCode?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(ra.RiskAssessmentCode?.Trim(), request.RiskAssessmentCode?.Trim(), StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (matchingAnalysis != null)
            {
                _logger.LogInformation("✅ Found RiskAnalysis {Code} for Hazard {HazardCode} and Assessment {AssessmentCode}", 
                    matchingAnalysis.Code, request.HazardCode, request.RiskAssessmentCode);
                
                return Result<RiskAnalysis>.Success(matchingAnalysis);
            }
            else
            {
                _logger.LogWarning("⚠️ No RiskAnalysis found for HazardCode: {HazardCode} and AssessmentCode: {AssessmentCode}", 
                    request.HazardCode, request.RiskAssessmentCode);

                // Log some debugging info
                var hazardMatches = getAllResult.Value.Where(ra => 
                    string.Equals(ra.HazardCode?.Trim(), request.HazardCode?.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
                _logger.LogInformation("🔍 Found {Count} RiskAnalysis records for Hazard {HazardCode}", hazardMatches.Count, request.HazardCode);
                
                foreach (var match in hazardMatches.Take(5)) // Log first 5 matches
                {
                    _logger.LogInformation("    └─ RiskAnalysis {Code}: Hazard={Hazard}, Assessment={Assessment}", 
                        match.Code, match.HazardCode, match.RiskAssessmentCode);
                }
                
                return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("❌ Error processing GetRiskAnalysisByHazardAndAssessmentQuery for Hazard: {HazardCode}, Assessment: {AssessmentCode}", 
                ApplicationEventIds.Error, ex);
            return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.GeneralError.UnProcessableRequest);
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