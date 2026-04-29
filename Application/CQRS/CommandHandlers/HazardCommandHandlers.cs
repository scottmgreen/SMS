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
using SMS_Domain.Events;
using SMS_Domain.ValueObjects;
using SMS_Application.Interfaces;
using SMS_Application.Services;

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
public class CreateHazardCommandHandler : BaseCommandBundle, IBaseRequestHandler<CreateHazardCommand, Result<Hazard>>
{
    private readonly HazardService _hazardService;
    private readonly IBaseMediator _mediator;  // 🔧 ADD: IBaseMediator for consistent sub-operations
    private readonly IBaseEventBus _eventBus; // NEW: EventBus for event publishing
    private readonly ILogger<CreateHazardCommandHandler> _logger;
    private readonly ILogSupport _logsupport;
    private readonly string _logheader = string.Empty;

    public CreateHazardCommandHandler(
        HazardService hazardService,
        IBaseMediator mediator,  // 🔧 ADD: IBaseMediator injection
        IBaseEventBus eventBus,  // NEW: EventBus injection
        ILogSupport logsupport,
        ILogger<CreateHazardCommandHandler> logger)
    {
        _hazardService = hazardService ?? throw new ArgumentNullException(nameof(hazardService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));  // 🔧 ADD: Validation
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus)); // NEW: EventBus validation
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

            // 🔧 FIXED: DO NOT auto-create default location here
            // Location creation should be handled by the calling service (UI/API) based on actual data
            // This prevents duplicate location entries and allows proper coordinate handling
            _logger.LogInformation("✅ Hazard created without auto-location - Code: {HazardCode}. Location will be handled by calling service.", hazard.Code);

            // 🚀 NEW: EventBus Integration - Publish HazardCreatedEvent for complete workflow automation
            try
            {
                _logger.LogInformation("🎯 [COMMAND HANDLER] Publishing HazardCreatedEvent for {HazardCode}", hazard.Code);

                // Determine hazard priority based on type/category
                var hazardPriority = DetermineHazardPriority(hazard.HazardType, hazard.HazardCategory);

                _logger.LogInformation("🎯 [COMMAND HANDLER] Determined priority: {Priority} for hazard {HazardCode} (Type: {Type}, Category: {Category})",
                    hazardPriority, hazard.Code, hazard.HazardType, hazard.HazardCategory);

                // Create and publish HazardCreatedEvent for complete workflow automation
                var hazardCreatedEvent = new HazardCreatedEvent(
                    hazardId: hazard.Code,
                    hazardCode: hazard.Code,
                    hazardName: hazard.Name ?? "Unnamed Hazard",
                    hazardType: hazard.HazardType ?? "Unknown",
                    hazardCategory: hazard.HazardCategory ?? "Unknown",
                    description: hazard.Description ?? "No description",
                    locationArea: hazard.LocationArea ?? "Unknown Location",
                    reportCode: hazard.ReportCode ?? "Unknown Report",
                    createdBy: hazard.CreatedBy ?? "System",
                    createdDate: hazard.CreatedDate ?? DateTime.UtcNow,
                    isInitialHazard: hazard.IsInitialHazard,
                    priority: hazardPriority,
                    latitude: hazard.HazardLocation?.Latitude,
                    longitude: hazard.HazardLocation?.Longitude
                );

                // Publish the domain event - this triggers the complete workflow (MANUAL mode for testing)
                var eventResult = await _eventBus.PublishDomainEventAsync(hazardCreatedEvent, EventExecutionMode.Manual);

                if (eventResult.IsSuccess)
                {
                    _logger.LogInformation("✅ [COMMAND HANDLER] HazardCreatedEvent published successfully for {HazardCode} - Complete workflow initiated", hazard.Code);
                }
                else
                {
                    _logger.LogWarning("⚠️ [COMMAND HANDLER] Failed to publish HazardCreatedEvent for {HazardCode}: {Error} - continuing with command execution", 
                        hazard.Code, eventResult.Error.Message);
                }
            }
            catch (Exception eventEx)
            {
                // Don't fail the entire command if EventBus fails
                _logger.LogWarning(eventEx, "⚠️ [COMMAND HANDLER] EventBus integration failed for {HazardCode} - continuing with command execution", hazard.Code);
            }

            _logger.LogApplicationInformation(ApplicationEventIds.Information, "✅ CQRS Consistent: Hazard saved with complete audit trail - {Code}", hazard.Code);
            return Result<Hazard>.Success(hazardResult.Value);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("❌ Exception creating hazard - {Name}", ApplicationEventIds.Error, ex);
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.CreateFailed);
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Determines hazard priority based on type and category for automated workflow routing
    /// Business logic to assign priority levels for automated workflow routing
    /// </summary>
    private SMS_Domain.Enums.HazardPriority DetermineHazardPriority(string? hazardType, string? hazardCategory)
    {
        try
        {
            // Business rules for priority determination
            var type = hazardType?.ToUpper() ?? "";
            var category = hazardCategory?.ToUpper() ?? "";

            _logger.LogDebug("🎯 [PRIORITY] Determining priority for Type: '{Type}', Category: '{Category}'", type, category);

            // Handle DEFAULT classifications - assign High priority for testing
            if (type.Contains("DEFAULT") || category.Contains("DEFAULT"))
            {
                _logger.LogInformation("🎯 [PRIORITY] DEFAULT classification detected - assigning High priority for testing");
                return SMS_Domain.Enums.HazardPriority.High;
            }

            // Critical priority conditions
            if (type.Contains("STRUCTURAL") || type.Contains("FIRE") || type.Contains("EXPLOSIVE") ||
                category.Contains("SAFETY_CRITICAL") || category.Contains("REGULATORY"))
            {
                _logger.LogInformation("🎯 [PRIORITY] Critical priority assigned");
                return SMS_Domain.Enums.HazardPriority.Critical;
            }

            // High priority conditions  
            if (type.Contains("EQUIPMENT") || type.Contains("MAINTENANCE") || type.Contains("OPERATIONAL") ||
                category.Contains("OPERATIONAL") || category.Contains("MAINTENANCE"))
            {
                _logger.LogInformation("🎯 [PRIORITY] High priority assigned");
                return SMS_Domain.Enums.HazardPriority.High;
            }

            // Medium priority conditions
            if (type.Contains("ENVIRONMENTAL") || type.Contains("DOCUMENTATION") ||
                category.Contains("ENVIRONMENTAL") || category.Contains("PROCESS"))
            {
                _logger.LogInformation("🎯 [PRIORITY] Medium priority assigned");
                return SMS_Domain.Enums.HazardPriority.Medium;
            }

            // Default to High for testing purposes
            _logger.LogInformation("🎯 [PRIORITY] No specific match - defaulting to High priority for testing");
            return SMS_Domain.Enums.HazardPriority.High;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "🎯 [PRIORITY] Error determining hazard priority for Type: {Type}, Category: {Category} - defaulting to High",
                hazardType, hazardCategory);
            return SMS_Domain.Enums.HazardPriority.High;
        }
    }

    #endregion
}

public class UpdateHazardCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateHazardCommand, Result<Hazard>>
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

public class ResetHazardScoresCommandHandler : BaseCommandBundle, IBaseRequestHandler<ResetHazardScoresCommand, Result<Hazard>>
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

public class DeleteHazardCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeleteHazardCommand, Result<bool>>
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
