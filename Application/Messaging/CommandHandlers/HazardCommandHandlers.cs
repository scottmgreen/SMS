//-----------------------------------------------------------------------
// <copyright file="HazardCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers implementing SMS hazard management and lifecycle logic.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Infrastructure.Interfaces;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// HAZARD COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

/// <summary>
/// Command handler for creating hazards using Application Services
/// Used by HazardReporting page following Clean Architecture principles
/// </summary>
public class CreateHazardCommandHandler : BaseCommandBundle, IRequestHandler<CreateHazardCommand, Result<Hazard>>
{
    private readonly HazardService _hazardService;
    private readonly IScoringPanelService _scoringPanelService;
    private readonly HazardLocationService _hazardLocationService;
    private readonly IRiskAssessmentService _riskAssessmentService;
    private readonly IRiskAnalysisService _riskAnalysisService;
    private readonly ILogger<CreateHazardCommandHandler> _logger;
    private readonly ILogSupport _logsupport;
    private readonly string _logheader = string.Empty;

    public CreateHazardCommandHandler(
        HazardService hazardService,
        IScoringPanelService scoringPanelService,
        HazardLocationService hazardLocationService,
        IRiskAssessmentService riskAssessmentService,
        IRiskAnalysisService riskAnalysisService,
        ILogSupport logsupport,
        ILogger<CreateHazardCommandHandler> logger)
    {
        _hazardService = hazardService ?? throw new ArgumentNullException(nameof(hazardService));
        _scoringPanelService = scoringPanelService ?? throw new ArgumentNullException(nameof(scoringPanelService));
        _hazardLocationService = hazardLocationService ?? throw new ArgumentNullException(nameof(hazardLocationService));
        _riskAssessmentService = riskAssessmentService ?? throw new ArgumentNullException(nameof(riskAssessmentService));
        _riskAnalysisService = riskAnalysisService ?? throw new ArgumentNullException(nameof(riskAnalysisService));
        _logsupport = logsupport;
        _logheader = _logsupport.GenerateLogHeader();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Hazard>> HandleAsync(CreateHazardCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("✅ Clean Architecture: Creating new hazard - {Name} for Report: {ReportCode}", 
                request.Hazard.Name, request.Hazard.ReportCode);

            Hazard hazard = request.Hazard;
            hazard.ReportCode = request.Hazard.ReportCode;

            var hazardResult = await _hazardService.CreateHazardAsync(hazard, ct);
            if (hazardResult.IsFailure)
            {
                _logger.LogApplicationError($"{_logheader} Clean Architecture: Failed to save hazard via application service", ApplicationEventIds.Error, null);
                return Result<Hazard>.Failure<Hazard>(hazardResult.Error);
            }
            else
            {
                hazard = hazardResult.Value;

                //// Create scoring panel using Application Service
                //var scoringPanel = new ScoringPanel(new ScoringPanelID("SP-0000"))
                //{
                //    HazardCode = hazard.Code
                //};

                //var scoringPanelResult = await _scoringPanelService.CreateScoringPanelAsync(scoringPanel, ct);
                //if (scoringPanelResult.IsFailure)
                //{
                //    _logger.LogWarning("Failed to create scoring panel for hazard {HazardCode}", hazard.Code);
                //}

                // Create hazard location using Application Service
                var hazardLocation = new HazardLocation(new HazardLocationID("HL-0000"))
                {
                    HazardCode = hazard.Code,
                    Latitude = 0,
                    Longitude = 0,
                    Description = "Map selected location"
                };

                var createdLocationResult = await _hazardLocationService.CreateHazardLocationAsync(hazardLocation, ct);
                if (createdLocationResult.IsSuccess)
                {
                    hazard.HazardLocation = createdLocationResult.Value;
                }
            }

            _logger.LogApplicationInformation(ApplicationEventIds.Information, "✅ Clean Architecture: Hazard saved via application service - {Code}", hazard.Code);
            return Result<Hazard>.Success(hazardResult.Value);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("✅ Clean Architecture: Exception creating hazard - {Name}", ApplicationEventIds.Error, ex);
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.CreateFailed);
        }
    }
}

public class UpdateHazardCommandHandler : BaseCommandBundle, IRequestHandler<UpdateHazardCommand, Result<Hazard>>
{
    private readonly HazardService _hazardService;
    private readonly ILogger<UpdateHazardCommandHandler> _logger;

    public UpdateHazardCommandHandler(HazardService hazardService, ILogger<UpdateHazardCommandHandler> logger)
    {
        _hazardService = hazardService ?? throw new ArgumentNullException(nameof(hazardService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Hazard>> HandleAsync(UpdateHazardCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.Hazard is null)
            {
                _logger.LogApplicationError("UpdateHazardCommand received with null Hazard", ApplicationEventIds.Error, null);
                return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateHazardCommand for ID: {Id}, Code: {Code}",
                request.Hazard.Id, request.Hazard.Code);

            var result = await _hazardService.UpdateHazardAsync(request.Hazard, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated Hazard with ID: {Id}", request.Hazard.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to update Hazard with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateHazardCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating Hazard with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.UpdateFailed);
        }
    }
}

public class ResetHazardScoresCommandHandler : BaseCommandBundle, IRequestHandler<ResetHazardScoresCommand, Result<Hazard>>
{
    private readonly HazardService _hazardService;
    private readonly ILogger<ResetHazardScoresCommandHandler> _logger;

    public ResetHazardScoresCommandHandler(HazardService hazardService, ILogger<ResetHazardScoresCommandHandler> logger)
    {
        _hazardService = hazardService ?? throw new ArgumentNullException(nameof(hazardService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Hazard>> HandleAsync(ResetHazardScoresCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.Hazard is null)
            {
                _logger.LogApplicationError("ResetHazardScoresCommand received with null Hazard", ApplicationEventIds.Error, null);
                return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.NullOrEmpty);
            }

            _logger.LogInformation("Processing ResetHazardScoresCommand for ID: {Id}, Code: {Code}", request.Hazard.Id, request.Hazard.Code);
            request.Hazard.UpdatedDate = DateTime.UtcNow;
            var result = await _hazardService.UpdateHazardAsync(request.Hazard, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully reset scores for Hazard with ID: {Id}", request.Hazard.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to reset scores for Hazard with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("ResetHazardScoresCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while resetting Hazard scores with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.UpdateFailed);
        }
    }
}

public class DeleteHazardCommandHandler : BaseCommandBundle, IRequestHandler<DeleteHazardCommand, Result<bool>>
{
    private readonly HazardService _hazardService;
    private readonly ILogger<DeleteHazardCommandHandler> _logger;

    public DeleteHazardCommandHandler(HazardService hazardService, ILogger<DeleteHazardCommandHandler> logger)
    {
        _hazardService = hazardService ?? throw new ArgumentNullException(nameof(hazardService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteHazardCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.HazardId is null)
            {
                _logger.LogApplicationError("DeleteHazardCommand received with null HazardId", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.HazardError.NullOrEmpty);
            }

            _logger.LogInformation("Processing DeleteHazardCommand for ID: {Id}", request.HazardId);

            var result = await _hazardService.DeleteHazardAsync(request.HazardId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted Hazard with ID: {Id}", request.HazardId);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete Hazard with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteHazardCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deleting Hazard with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.HazardError.DeleteFailed);
        }
    }
}
