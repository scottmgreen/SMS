//-----------------------------------------------------------------------
// <copyright file="RiskAnalysisService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS Risk assessment service managing risk analysis and mitigation strategies.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Domain.Enums;


using Microsoft.Extensions.Logging;

namespace SMS_Application.Services;

public sealed class RiskAnalysisService : IRiskAnalysisService
{
    private readonly RiskAnalysisDataService _dataService;
    private readonly ILogger<RiskAnalysisService> _logger;

    public RiskAnalysisService(RiskAnalysisDataService dataService, ILogger<RiskAnalysisService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region IRiskAnalysisService Implementation

    public async Task<Result<RiskAnalysis>> CreateRiskAnalysisAsync(RiskAnalysis analysis, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Creating risk analysis with code: {Code}", analysis?.Code);
            var result = await _dataService.CreateRiskAnalysisAsync(analysis, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully created risk analysis with ID: {Id}", result.Value?.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to create risk analysis. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error creating risk analysis");
            return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.CreateFailed);
        }
    }

    public async Task<Result<RiskAnalysis>> GetRiskAnalysisByIdAsync(RiskAnalysisID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving risk analysis with ID: {Id}", id);
            return await _dataService.GetRiskAnalysisByCodeAsync(id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving risk analysis with ID: {Id}", id);
            return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.NotFound);
        }
    }

    public async Task<Result<RiskAnalysis>> GetRiskAnalysisByHazardCodeAsync(string hazardCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving risk analyses for hazard: {HazardCode}", hazardCode);
            var hazardId = new HazardID(hazardCode);
            
            // The data service returns a single RiskAnalysis, so we wrap it in a list
            var singleResult = await _dataService.GetRiskAnalysisByHazardCodeAsync(hazardId, ct).ConfigureAwait(false);
            
            if (singleResult.IsFailure)
            {
                return Result<RiskAnalysis>.Failure<RiskAnalysis>(singleResult.Error);
            }

            var resultList = singleResult; 
               

            return resultList;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving risk analyses for hazard: {HazardCode}", hazardCode);
            return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.NotFound);
        }
    }

    public async Task<Result<RiskAnalysis>> UpdateRiskAnalysisAsync(RiskAnalysis analysis, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Updating risk analysis with ID: {Id}", analysis?.Id);
            var result = await _dataService.UpdateRiskAnalysisAsync(analysis, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully updated risk analysis with ID: {Id}", analysis?.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to update risk analysis. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error updating risk analysis with ID: {Id}", analysis?.Id);
            return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteRiskAnalysisAsync(RiskAnalysisID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Deleting risk analysis with ID: {Id}", id);
            var result = await _dataService.DeleteRiskAnalysisAsync(id, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully deleted risk analysis with ID: {Id}", id);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete risk analysis. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error deleting risk analysis with ID: {Id}", id);
            return Result<bool>.Failure<bool>(DomainErrors.RiskAnalysisError.DeleteFailed);
        }
    }

    public async Task<Result<RiskAnalysisResult>> PerformRiskAnalysisAsync(RiskAnalysisParameters parameters, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Performing risk analysis for hazard: {HazardCode}", parameters?.HazardCode);
            
            if (parameters == null)
            {
                return Result<RiskAnalysisResult>.Failure<RiskAnalysisResult>(DomainErrors.RiskAnalysisError.NullOrEmpty);
            }

            // Business logic for comprehensive risk analysis
            var riskScore = parameters.Severity * parameters.Likelihood;
            var riskLevel = riskScore switch
            {
                >= 20 => "Very High",
                >= 15 => "High", 
                >= 10 => "Medium",
                >= 5 => "Low",
                _ => "Very Low"
            };

            var recommendations = GenerateRecommendations(riskLevel, parameters);

            var result = new RiskAnalysisResult
            {
                RiskLevel = riskLevel,
                RiskScore = riskScore,
                Recommendations = recommendations,
                AnalyzedDate = DateTime.UtcNow
            };

            _logger.LogApplicationInformation("Completed risk analysis for hazard {HazardCode} with level {RiskLevel}", 
                parameters.HazardCode, riskLevel);

            return Result<RiskAnalysisResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error performing risk analysis for hazard: {HazardCode}", parameters?.HazardCode);
            return Result<RiskAnalysisResult>.Failure<RiskAnalysisResult>(DomainErrors.RiskAnalysisError.UpdateFailed);
        }
    }

    #endregion

    #region Legacy Methods (keeping for backward compatibility)

    public async Task<Result<RiskAnalysis>> GetRiskAnalysisByHazardIdAsync(HazardID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving risk analysis with hazard ID: {Id}", id);
            return await _dataService.GetRiskAnalysisByHazardCodeAsync(id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving risk analysis with hazard ID: {Id}", id);
            return Result<RiskAnalysis>.Failure<RiskAnalysis>(DomainErrors.RiskAnalysisError.NotFound);
        }
    }

    public async Task<Result<List<RiskAnalysis>>> GetAllRiskAnalysisAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving all risk analysis");
            return await _dataService.GetAllRiskAnalysisAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving all risk analysis");
            return Result<List<RiskAnalysis>>.Failure<List<RiskAnalysis>>(DomainErrors.RiskAnalysisError.NullOrEmpty);
        }
    }

    #endregion

    #region Private Helper Methods

    private string GenerateRecommendations(string riskLevel, RiskAnalysisParameters parameters)
    {
        return riskLevel switch
        {
            "Very High" => "Immediate action required. Stop operations and implement emergency controls.",
            "High" => "Urgent action required. Implement additional controls within 24 hours.",
            "Medium" => "Action required. Implement additional controls within reasonable timeframe.",
            "Low" => "Consider additional controls. Monitor for changes.",
            _ => "Continue current practices. Regular monitoring recommended."
        };
    }

    #endregion
}

