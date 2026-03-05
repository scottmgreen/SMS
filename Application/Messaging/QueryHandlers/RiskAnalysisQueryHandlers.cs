//-----------------------------------------------------------------------
// <copyright file="RiskAnalysisQueryHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query handlers implementing SMS risk assessment data retrieval logic.
//                  Implements query handlers for processing read operations.
//                  Retrieves and transforms data for presentation layer consumption.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// RISK ANALYSIS QUERY HANDLERS - Clean Architecture Pattern
// =============================================

public class GetRiskAnalysisByCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetRiskAnalysisByCodeQuery, Result<RiskAnalysis>>
{
    private readonly RiskAnalysisService _riskAnalysisService;
    private readonly ILogger<GetRiskAnalysisByCodeQueryHandler> _logger;

    public GetRiskAnalysisByCodeQueryHandler(RiskAnalysisService riskAnalysisService, ILogger<GetRiskAnalysisByCodeQueryHandler> logger)
    {
        _riskAnalysisService = riskAnalysisService ?? throw new ArgumentNullException(nameof(riskAnalysisService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAnalysis>> HandleAsync(GetRiskAnalysisByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("✅ Clean Architecture: Processing GetRiskAnalysisByCodeQuery for Code: {Code}", request.RiskAnalysisCode);
            var result = await _riskAnalysisService.GetRiskAnalysisByIdAsync(new RiskAnalysisID(request.RiskAnalysisCode.Value), ct).ConfigureAwait(false);
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
    private readonly RiskAnalysisService _riskAnalysisService;
    private readonly ILogger<GetRiskAnalysisByIdQueryHandler> _logger;

    public GetRiskAnalysisByIdQueryHandler(RiskAnalysisService riskAnalysisService, ILogger<GetRiskAnalysisByIdQueryHandler> logger)
    {
        _riskAnalysisService = riskAnalysisService ?? throw new ArgumentNullException(nameof(riskAnalysisService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAnalysis>> HandleAsync(GetRiskAnalysisByIdQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("✅ Clean Architecture: Processing GetRiskAnalysisByIdQuery for ID: {Id}", request.RiskAnalysisId);
            var result = await _riskAnalysisService.GetRiskAnalysisByIdAsync(new RiskAnalysisID(request.RiskAnalysisId.Value), ct).ConfigureAwait(false);
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
    private readonly RiskAnalysisService _riskAnalysisService;
    private readonly ILogger<GetRiskAnalysisByHazardCodeQueryHandler> _logger;

    public GetRiskAnalysisByHazardCodeQueryHandler(RiskAnalysisService riskAnalysisService, ILogger<GetRiskAnalysisByHazardCodeQueryHandler> logger)
    {
        _riskAnalysisService = riskAnalysisService ?? throw new ArgumentNullException(nameof(riskAnalysisService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAnalysis>> HandleAsync(GetRiskAnalysisByHazardCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("✅ Clean Architecture: Processing GetRiskAnalysisByHazardCodeQuery for Code: {Code}", request.HazardCode);
            var result = await _riskAnalysisService.GetRiskAnalysisByHazardCodeAsync(request.HazardCode.Value, ct).ConfigureAwait(false);
 
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
    private readonly RiskAnalysisService _riskAnalysisService;
    private readonly ILogger<GetRiskAnalysisByHazardAndAssessmentQueryHandler> _logger;

    public GetRiskAnalysisByHazardAndAssessmentQueryHandler(RiskAnalysisService riskAnalysisService, ILogger<GetRiskAnalysisByHazardAndAssessmentQueryHandler> logger)
    {
        _riskAnalysisService = riskAnalysisService ?? throw new ArgumentNullException(nameof(riskAnalysisService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAnalysis>> HandleAsync(GetRiskAnalysisByHazardAndAssessmentQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("🔍 Processing GetRiskAnalysisByHazardAndAssessmentQuery for Hazard: {HazardCode}, Assessment: {AssessmentCode}",   request.HazardCode, request.RiskAssessmentCode);

            // Use LINQ filtering approach to eliminate database round trip
            var getAllResult = await _riskAnalysisService.GetAllRiskAnalysisAsync(ct).ConfigureAwait(false);
            
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
            _logger.LogApplicationError("Error processing GetRiskAnalysisByHazardAndAssessmentQuery for HazardCode: {HazardCode}", ApplicationEventIds.Error, ex);
            return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.NotFound);
        }
    }
}

public class GetAllRiskAnalysisQueryHandler : BaseQueryBundle, IRequestHandler<GetAllRiskAnalysisQuery, Result<List<RiskAnalysis>>>
{
    private readonly RiskAnalysisService _riskAnalysisService;
    private readonly ILogger<GetAllRiskAnalysisQueryHandler> _logger;

    public GetAllRiskAnalysisQueryHandler(RiskAnalysisService riskAnalysisService, ILogger<GetAllRiskAnalysisQueryHandler> logger)
    {
        _riskAnalysisService = riskAnalysisService ?? throw new ArgumentNullException(nameof(riskAnalysisService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<RiskAnalysis>>> HandleAsync(GetAllRiskAnalysisQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("✅ Clean Architecture: Processing GetAllRiskAnalysisQuery");
            var result = await _riskAnalysisService.GetAllRiskAnalysisAsync(ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetAllRiskAnalysisQuery", ApplicationEventIds.Error, ex);
            return Result<List<RiskAnalysis>>.Failure<List<RiskAnalysis>>(DomainErrors.RiskAnalysisError.NullOrEmpty);
        }
    }
}
