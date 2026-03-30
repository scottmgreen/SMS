//-----------------------------------------------------------------------
// <copyright file="RiskAssessmentService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS Risk assessment service managing risk analysis and mitigation strategies.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS_Application.Services;

/// <summary>
/// Enhanced RiskAssessmentService with Steps 1-5 support
/// Provides application-level orchestration for risk assessment operations
/// </summary>
public sealed class RiskAssessmentService : IRiskAssessmentService
{
    private readonly RiskAssessmentDataService _dataService;
    private readonly IMediator _mediator;
    private readonly ILogger<RiskAssessmentService> _logger;

    public RiskAssessmentService(
        RiskAssessmentDataService dataService,
        IMediator mediator,
        ILogger<RiskAssessmentService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region IRiskAssessmentService Implementation

    public async Task<Result<RiskAssessment>> CreateRiskAssessmentAsync(RiskAssessment assessment, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating risk assessment with code: {Code}", assessment?.Code);
            var result = await _dataService.CreateRiskAssessmentAsync(assessment, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created risk assessment with ID: {Id}", result.Value?.Id);
            }
            else
            {
                _logger.LogError("Failed to create risk assessment. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating risk assessment");
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.CreateFailed);
        }
    }

    public async Task<Result<RiskAssessment>> GetRiskAssessmentByIdAsync(RiskAssessmentID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving risk assessment with ID: {Id}", id);
            return await _dataService.GetRiskAssessmentByCodeAsync(id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving risk assessment with ID: {Id}", id);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NotFound);
        }
    }

    public async Task<Result<List<RiskAssessment>>> GetRiskAssessmentsByHazardCodeAsync(string hazardCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving risk assessments for hazard: {HazardCode}", hazardCode);
            var hazardId = new HazardID(hazardCode);
            return await _dataService.GetRiskAssessmentsByHazardCodeAsync(hazardId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving risk assessments for hazard: {HazardCode}", hazardCode);
            return Result<List<RiskAssessment>>.Failure<List<RiskAssessment>>(DomainErrors.RiskAssessmentError.NotFound);
        }
    }

    public async Task<Result<RiskAssessment>> UpdateRiskAssessmentAsync(RiskAssessment assessment, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating risk assessment with ID: {Id}", assessment?.Id);
            var result = await _dataService.UpdateRiskAssessmentAsync(assessment, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated risk assessment with ID: {Id}", assessment?.Id);
            }
            else
            {
                _logger.LogError("Failed to update risk assessment. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating risk assessment with ID: {Id}", assessment?.Id);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteRiskAssessmentAsync(RiskAssessmentID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deleting risk assessment with ID: {Id}", id);
            var result = await _dataService.DeleteRiskAssessmentAsync(id, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted risk assessment with ID: {Id}", id);
            }
            else
            {
                _logger.LogError("Failed to delete risk assessment. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting risk assessment with ID: {Id}", id);
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.DeleteFailed);
        }
    }

    public async Task<Result<string>> CalculateRiskLevelAsync(RiskAssessment assessment, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Calculating risk level for assessment: {Id}", assessment?.Id);
            
            // Business logic for risk level calculation
            if (assessment == null)
            {
                return Result<string>.Failure<string>(DomainErrors.RiskAssessmentError.NullOrEmpty);
            }

            // Simple risk matrix calculation (can be enhanced with more complex business rules)
            var severity = assessment.FinalSeverityScore ?? 0;
            var likelihood = assessment.FinalLikelihoodScore ?? 0;
            var riskScore = severity * likelihood;
            
            var riskLevel = riskScore switch
            {
                >= 20 => "Very High",
                >= 15 => "High", 
                >= 10 => "Medium",
                >= 5 => "Low",
                _ => "Very Low"
            };

            _logger.LogInformation("Calculated risk level {RiskLevel} for assessment {Id}", riskLevel, assessment.Id);
            return Result<string>.Success(riskLevel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calculating risk level for assessment: {Id}", assessment?.Id);
            return Result<string>.Failure<string>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    #endregion

    #region Legacy Methods (keeping for backward compatibility)

    public async Task<Result<RiskAssessment>> GetRiskAssessmentByCodeAsync(RiskAssessmentID id, CancellationToken ct = default)
    {
        return await GetRiskAssessmentByIdAsync(id, ct);
    }

    public async Task<Result<List<RiskAssessment>>> GetRiskAssessmentsByHazardCodeAsync(HazardID code, CancellationToken ct = default)
    {
        return await GetRiskAssessmentsByHazardCodeAsync(code.Value, ct);
    }

    public async Task<Result<List<RiskAssessment>>> GetAllRiskAssessmentsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all risk assessments");
            return await _dataService.GetAllRiskAssessmentsAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all risk assessments");
            return Result<List<RiskAssessment>>.Failure<List<RiskAssessment>>(DomainErrors.RiskAssessmentError.NullOrEmpty);
        }
    }

    #endregion
}
