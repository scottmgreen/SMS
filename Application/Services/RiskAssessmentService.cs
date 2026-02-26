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

namespace SMS_Application.Services;

/// <summary>
/// Enhanced RiskAssessmentService with Steps 1-5 support
/// Provides application-level orchestration for risk assessment operations
/// </summary>
public sealed class RiskAssessmentService
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

    #region Basic CRUD Operations

    //public async Task<Result<RiskAssessment>> CreateRiskAssessmentAsync(RiskAssessment riskAssessment, CancellationToken ct = default)
    //{
    //    try
    //    {
    //        _logger.LogInformation("Creating risk assessment with code: {Code}", riskAssessment?.Code);

    //        var command = new CreateRiskAssessmentCommand(riskAssessment);
    //        var result = await _mediator.SendAsync(command, ct).ConfigureAwait(false);

    //        if (result.IsSuccess)
    //        {
    //            _logger.LogInformation("Successfully created risk assessment with ID: {Id}", result.Value?.Id);
    //        }
    //        else
    //        {
    //            _logger.LogError("Failed to create risk assessment. Error: {Error}", result.Error?.Message);
    //        }

    //        return result;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Unexpected error creating risk assessment");
    //        return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.CreateFailed);
    //    }
    //}

    public async Task<Result<RiskAssessment>> GetRiskAssessmentByCodeAsync(RiskAssessmentID id, CancellationToken ct = default)
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
    public async Task<Result<List<RiskAssessment>>> GetRiskAssessmentsByHazardCodeAsync(HazardID code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving risk assessment with ID: {Id}", code);
            return await _dataService.GetRiskAssessmentsByHazardCodeAsync(code, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving risk assessment with Hazard ID: {Id}", code);
            return Result<List<RiskAssessment>>.Failure<List<RiskAssessment>>(DomainErrors.RiskAssessmentError.NotFound);
        }
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
