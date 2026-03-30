//-----------------------------------------------------------------------
// <copyright file="HazardCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers implementing SMS hazard management and lifecycle logic.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;
using SMS_Infrastructure.Interfaces;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// HAZARD COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

/// <summary>
/// Command handler for creating hazards using CQRS/Mediator pattern
/// Ensures ALL sub-operations go through AuditPipeline for complete audit trail consistency
/// </summary>
public class CreateHazardCommandHandler : BaseCommandBundle, IRequestHandler<CreateHazardCommand, Result<Hazard>>
{
    private readonly HazardService _hazardService;
    private readonly IMediator _mediator;  // 🔧 ADD: IMediator for consistent sub-operations
    private readonly ILogger<CreateHazardCommandHandler> _logger;
    private readonly ILogSupport _logsupport;
    private readonly string _logheader = string.Empty;

    public CreateHazardCommandHandler(
        HazardService hazardService,
        IMediator mediator,  // 🔧 ADD: IMediator injection
        ILogSupport logsupport,
        ILogger<CreateHazardCommandHandler> logger)
    {
        _hazardService = hazardService ?? throw new ArgumentNullException(nameof(hazardService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));  // 🔧 ADD: Validation
        _logsupport = logsupport;
        _logheader = _logsupport.GenerateLogHeader();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Hazard>> HandleAsync(CreateHazardCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("✅ CQRS Consistent: Creating new hazard - {Name} for Report: {ReportCode}", 
                request.Hazard.Name, request.Hazard.ReportCode);

            Hazard hazard = request.Hazard;
            hazard.ReportCode = request.Hazard.ReportCode;

            // ✅ Main hazard creation - goes through AuditPipeline via this handler
            var hazardResult = await _hazardService.CreateHazardAsync(hazard, ct);
            if (hazardResult.IsFailure)
            {
                _logger.LogApplicationError($"{_logheader} Failed to save hazard via application service", ApplicationEventIds.Error, null);
                return Result<Hazard>.Failure<Hazard>(hazardResult.Error);
            }

            hazard = hazardResult.Value;

            // ✅ FIXED: Create hazard location using IMediator -> ensures AuditPipeline consistency
            var hazardLocation = new HazardLocation(new HazardLocationID("HL-0000"))
            {
                HazardCode = hazard.Code,
                Latitude = 0,
                Longitude = 0,
                Description = "Map selected location"
            };

            var createLocationCommand = new CreateHazardLocationCommand(hazardLocation);
            var createdLocationResult = await _mediator.SendAsync(createLocationCommand, ct);  // 🔧 FIXED: Use IMediator instead of direct service call
            
            if (createdLocationResult.IsSuccess)
            {
                hazard.HazardLocation = createdLocationResult.Value;
                _logger.LogInformation("✅ HazardLocation created via IMediator with audit trail for Hazard: {HazardCode}", hazard.Code);
            }
            else
            {
                _logger.LogWarning("⚠️ Failed to create HazardLocation via IMediator for Hazard: {HazardCode}", hazard.Code);
            }

            _logger.LogApplicationInformation(ApplicationEventIds.Information, "✅ CQRS Consistent: Hazard and sub-entities saved with complete audit trail - {Code}", hazard.Code);
            return Result<Hazard>.Success(hazardResult.Value);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("❌ Exception creating hazard - {Name}", ApplicationEventIds.Error, ex);
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
