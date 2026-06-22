//-----------------------------------------------------------------------
// <copyright file="HazardCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers implementing SMS hazard management and lifecycle logic.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Events;

using SMS_Infrastructure.Interfaces;

namespace SMS_Application.CommandHandlers
{

    // =============================================
    // HAZARD COMMAND HANDLERS - 
    // =============================================

    /// <summary>
    /// Command handler for creating hazards using CQRS/Mediator pattern
    /// Ensures ALL sub-operations go through AuditPipeline for complete audit trail consistency
    /// </summary>
    public class CreateHazardCommandHandler : BaseCommandBundle, IBaseRequestHandler<CreateHazardCommand, Result<Hazard>>
    {
        private readonly HazardService _hazardService;
        private readonly IBaseMediator _mediator;
        private readonly IBaseEventBus _eventBus;
        private readonly ILogger<CreateHazardCommandHandler> _logger;
        private readonly ILogSupport _logsupport;
        private readonly string _logheader = string.Empty;

        public CreateHazardCommandHandler(
            HazardService hazardService,
            IBaseMediator mediator,  //IBaseMediator injection
            IBaseEventBus eventBus,  //EventBus injection
            ILogSupport logsupport,
            ILogger<CreateHazardCommandHandler> logger)
        {
            _hazardService = hazardService ?? throw new ArgumentNullException(nameof(hazardService));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _logsupport = logsupport;
            _logheader = _logsupport.GenerateLogHeader();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Hazard>> HandleAsync(CreateHazardCommand request, CancellationToken ct = default)
        {

            try
            {
                // =============================================
                // Hazard Create - Using AuditPipeline
                // Location creation is handled by the calling service (UI/API) 
                // =============================================

                Hazard hazard = request.Hazard;
                hazard.ReportCode = request.Hazard.ReportCode;

                var hazardResult = await _hazardService.CreateHazardAsync(hazard, ct);
                if (hazardResult.IsFailure)
                {
                    _logger.LogApplicationError($"{_logheader} Failed to save hazard via application service", ApplicationEventIds.Error, null);
                    return Result<Hazard>.Failure<Hazard>(hazardResult.Error);
                }

                hazard = hazardResult.Value;

                // =============================================
                // Event Bus
                // Only a simple Domain Event at this point
                // =============================================
                try
                {
                    if (!hazard.IsInitialHazard)
                    {
                        await HazardEventPublisher.PublishHazardEventAsync(_eventBus, _logger, EventType.HazardCreated, hazard, HazardPriority.Low);
                    }
                }
                catch (Exception eventEx)
                {
                    // Don't fail the entire command if EventBus fails
                    _logger.LogApplicationWarning(eventEx, "[COMMAND HANDLER] EventBus integration failed for {HazardCode} - continuing with command execution", hazard.Code);
                }

                _logger.LogApplicationInformation(ApplicationEventIds.Information, "CQRS : Hazard saved with complete audit trail - {Code}", hazard.Code);
                return Result<Hazard>.Success(hazardResult.Value);
            }
            catch (Exception ex)
            {
                _logger.LogApplicationError("Exception creating hazard - {Name}", ApplicationEventIds.Error, ex);
                return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.CreateFailed);
            }
        }

       
        #region Private Helper Methods

        /// <summary>
        /// Determines hazard priority based on type and category for automated workflow routing
        /// Business logic to assign priority levels for automated workflow routing
        /// </summary>
        //private SMS_Domain.Enums.HazardPriority DetermineHazardPriority(string? hazardType, string? hazardCategory)
        //{
        //    try
        //    {
        //        // Business rules for priority determination
        //        var type = hazardType?.ToUpper() ?? "";
        //        var category = hazardCategory?.ToUpper() ?? "";

        //        _logger.LogApplicationDebug("[PRIORITY] Determining priority for Type: '{Type}', Category: '{Category}'", type, category);

        //        // Handle DEFAULT classifications - assign High priority for testing
        //        if (type == HazardType.Default.Value || category == HazardCategory.Default.Value)
        //        {
        //            _logger.LogApplicationInformation("[PRIORITY] DEFAULT classification detected - assigning High priority for testing");
        //            return SMS_Domain.Enums.HazardPriority.High;
        //        }

        //        // Critical priority conditions
        //        if (type.Contains("STRUCTURAL") || type.Contains("FIRE") || type.Contains("EXPLOSIVE") || category.Contains("SAFETY_CRITICAL") || category.Contains("REGULATORY"))
        //        {
        //            _logger.LogApplicationInformation("[PRIORITY] Critical priority assigned");
        //            return SMS_Domain.Enums.HazardPriority.Critical;
        //        }

        //        // High priority conditions  
        //        if (type.Contains("EQUIPMENT") || type.Contains("MAINTENANCE") || type.Contains("OPERATIONAL") || category.Contains("OPERATIONAL") || category.Contains("MAINTENANCE"))
        //        {
        //            _logger.LogApplicationInformation("[PRIORITY] High priority assigned");
        //            return SMS_Domain.Enums.HazardPriority.High;
        //        }

        //        // Medium priority conditions
        //        if (type.Contains("ENVIRONMENTAL") || type.Contains("DOCUMENTATION") || category.Contains("ENVIRONMENTAL") || category.Contains("PROCESS"))
        //        {
        //            _logger.LogApplicationInformation("[PRIORITY] Medium priority assigned");
        //            return SMS_Domain.Enums.HazardPriority.Medium;
        //        }

        //        // Default to High for testing purposes
        //        _logger.LogApplicationInformation("[PRIORITY] No specific match - defaulting to High priority for testing");
        //        return SMS_Domain.Enums.HazardPriority.High;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogApplicationWarning(ex, "[PRIORITY] Error determining hazard priority for Type: {Type}, Category: {Category} - defaulting to High",  hazardType, hazardCategory);
        //        return SMS_Domain.Enums.HazardPriority.High;
        //    }
        //}

        #endregion
    }

    public class UpdateHazardCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateHazardCommand, Result<Hazard>>
    {
        private readonly HazardService _hazardService;
        private readonly IBaseMediator _mediator;
        private readonly IBaseEventBus _eventBus;
        private readonly ILogger<UpdateHazardCommandHandler> _logger;
        private readonly ILogSupport _logsupport;
        private readonly string _logheader = string.Empty;


        public UpdateHazardCommandHandler(HazardService hazardService,
            IBaseMediator mediator,  //IBaseMediator injection
            IBaseEventBus eventBus,  //EventBus injection
            ILogSupport logsupport,
            ILogger<UpdateHazardCommandHandler> logger)
        {
            _hazardService = hazardService ?? throw new ArgumentNullException(nameof(hazardService));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _logsupport = logsupport;
            _logheader = _logsupport.GenerateLogHeader();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Hazard>> HandleAsync(UpdateHazardCommand request, CancellationToken ct = default)
        {
            try
            {
                // =============================================
                // Hazard Create - Using AuditPipeline
                // Location creation is handled by the calling service (UI/API) 
                // =============================================

                Hazard hazard = request.Hazard;
                hazard.ReportCode = request.Hazard.ReportCode;

                var hazardResult = await _hazardService.UpdateHazardAsync(hazard, ct);
                if (hazardResult.IsFailure)
                {
                    _logger.LogApplicationError($"{_logheader} Failed to save hazard via application service", ApplicationEventIds.Error, null);
                    return Result<Hazard>.Failure<Hazard>(hazardResult.Error);
                }

                hazard = hazardResult.Value;

                // =============================================
                // Event Bus
                // Only a simple Domain Event at this point
                // =============================================
                try
                {
                    await HazardEventPublisher.PublishHazardEventAsync(_eventBus, _logger, EventType.HazardUpdated, hazard, HazardPriority.Low);

                }
                catch (Exception eventEx)
                {
                    // Don't fail the entire command if EventBus fails
                    _logger.LogApplicationWarning(eventEx, "[COMMAND HANDLER] EventBus integration failed for {HazardCode} - continuing with command execution", hazard.Code);
                }

                _logger.LogApplicationInformation(ApplicationEventIds.Information, "CQRS : Hazard saved with complete audit trail - {Code}", hazard.Code);
                return Result<Hazard>.Success(hazardResult.Value);
            }
            catch (Exception ex)
            {
                _logger.LogApplicationError("Exception creating hazard - {Name}", ApplicationEventIds.Error, ex);
                return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.CreateFailed);
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

                _logger.LogApplicationInformation("Processing ResetHazardScoresCommand for ID: {Id}, Code: {Code}", request.Hazard.Id, request.Hazard.Code);
                request.Hazard.UpdatedDate = DateTime.UtcNow;
                var result = await _hazardService.UpdateHazardAsync(request.Hazard, ct).ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    _logger.LogApplicationInformation("Successfully reset scores for Hazard with ID: {Id}", request.Hazard.Id);
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
                _logger.LogApplicationWarning("ResetHazardScoresCommand operation was cancelled");
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

                _logger.LogApplicationInformation("Processing DeleteHazardCommand for ID: {Id}", request.HazardId);

                var result = await _hazardService.DeleteHazardAsync(request.HazardId, ct).ConfigureAwait(false);

                if (result.IsSuccess)
                {
                    _logger.LogApplicationInformation("Successfully deleted Hazard with ID: {Id}", request.HazardId);
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
                _logger.LogApplicationWarning("DeleteHazardCommand operation was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogApplicationError("Unexpected error occurred while deleting Hazard with ID: {Id}", ApplicationEventIds.Error, ex);
                return Result<bool>.Failure<bool>(DomainErrors.HazardError.DeleteFailed);
            }
        }
    }



    
    public static class HazardEventPublisher
    {
        public static async Task PublishHazardEventAsync(
            IBaseEventBus eventBus,
            ILogger logger,
            EventType eventtype,
            Hazard hazard,
            HazardPriority hazardPriority)
        {
            // Determine hazard priority based on type/category
            hazardPriority = HazardPriority.Low; // Or your logic
            var eventid = new SMSEventID(Guid.NewGuid().ToString());
            BaseDomainEvent? hazardEvent = eventtype switch
            {
                var t when t == EventType.HazardCreated => CreateHazardEvent(new HazardCreatedEvent(eventid)),
                var t when t == EventType.HazardDeleted => CreateHazardEvent(new HazardDeletedEvent(eventid)),
                var t when t == EventType.HazardUpdated => CreateHazardEvent(new HazardUpdatedEvent(eventid)),
                _ => null
            };

            BaseDomainEvent CreateHazardEvent(BaseDomainEvent evt)
            {
                if (evt is HazardCreatedEvent created)
                {
                    created.ReportId = hazard.ReportCode;
                    created.HazardId = hazard.Code;
                    created.HazardCode = hazard.Code;
                    created.HazardName = hazard.Name ?? "Unnamed Hazard";
                    created.HazardType = hazard.HazardType ?? "Unknown";
                    created.HazardCategory = hazard.HazardCategory ?? "Unknown";
                    created.Description = hazard.Description ?? "No description";
                    created.LocationArea = hazard.LocationArea ?? "Unknown Location";
                    created.ReportCode = hazard.ReportCode ?? "Unknown Report";
                    created.CreatedBy = !string.IsNullOrWhiteSpace(hazard.CreatedBy) ? hazard.CreatedBy : "System";
                    created.CreatedDate = hazard.CreatedDate ?? DateTime.UtcNow;
                    created.IsInitialHazard = hazard.IsInitialHazard;
                    created.Priority = hazardPriority;
                    created.Latitude = hazard.HazardLocation?.Latitude;
                    created.Longitude = hazard.HazardLocation?.Longitude;
                }
                if (evt is HazardUpdatedEvent updated)
                {
                    updated.ReportId = hazard.ReportCode;
                    updated.HazardId = hazard.Code;
                    updated.UpdatedBy = "SYSTEM";
                    updated.UpdatedDate = DateTime.UtcNow;
                }
                if (evt is HazardDeletedEvent deleted)
                {
                    deleted.ReportId = hazard.ReportCode;
                    deleted.HazardId = hazard.Code;
                }
                return evt;
            }

            var eventResult = await eventBus.PublishDomainEventAsync(hazardEvent, EventExecutionMode.Manual);

            if (!eventResult.IsSuccess)
            {
                logger.LogApplicationWarning("[COMMAND HANDLER] Failed to publish Event for {HazardCode}: {Error} - continuing with command execution", hazard.Code, eventResult.Error.Message);
            }
        }
    }









}




    

