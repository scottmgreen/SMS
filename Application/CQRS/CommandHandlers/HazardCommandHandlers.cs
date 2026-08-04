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
using SMS_Domain.Enums;
using SMS_Application.Queries;

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

                HazardStatus? previousStatus = null;
                var previousRiskLevelValue = string.Empty;
                if (!string.IsNullOrWhiteSpace(hazard.Code))
                {
                    var existingHazardResult = await _hazardService.GetHazardByCodeAsync(new HazardID(hazard.Code), ct);
                    if (existingHazardResult.IsSuccess && existingHazardResult.Value is not null)
                    {
                        previousStatus = existingHazardResult.Value.Status;
                        previousRiskLevelValue = existingHazardResult.Value.HazardRiskLevel?.Value ?? string.Empty;
                    }
                }

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

                    await TransitionEventPublisher.PublishIfChangedAsync(
                        _eventBus,
                        previousStatus,
                        hazard.Status,
                        () => new HazardStatusChangedEvent(
                            id: new SMSEventID("EV-0000"),
                            hazardId: hazard.Id.Value,
                            hazardCode: hazard.Code,
                            previousStatus: previousStatus!,
                            newStatus: hazard.Status,
                            statusChangeReason: "Hazard status updated",
                            changedBy: hazard.UpdatedBy ?? hazard.CreatedBy ?? string.Empty,
                            statusChangeDate: hazard.UpdatedDate ?? DateTime.UtcNow),
                        ct).ConfigureAwait(false);

                    if (HasHazardRiskLevelChanged(previousRiskLevelValue, hazard.HazardRiskLevel?.Value))
                    {
                        await TriggerMitigationApprovalNotificationsForRescoreAsync(hazard, ct).ConfigureAwait(false);
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

        private async Task TriggerMitigationApprovalNotificationsForRescoreAsync(Hazard hazard, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(hazard.Code))
            {
                return;
            }

            var riskAssessmentsResult = await _mediator.SendAsync(
                new GetRiskAssessmentsByHazardCodeQuery(new HazardID(hazard.Code)),
                ct).ConfigureAwait(false);

            if (riskAssessmentsResult.IsFailure || riskAssessmentsResult.Value is null)
            {
                _logger.LogApplicationWarning("Risk-level rescore notification skipped for hazard {HazardCode}: risk assessment lookup failed.", hazard.Code);
                return;
            }

            var hasSubmittedAssessment = riskAssessmentsResult.Value.Any(assessment =>
                assessment.Status == RiskAssessmentStatus.AssessmentComplete);

            if (!hasSubmittedAssessment)
            {
                return;
            }

            var mitigationResult = await _mediator
                .SendAsync(new GetMitigationsByHazardCodeQuery(hazard.Code), ct)
                .ConfigureAwait(false);

            if (mitigationResult.IsFailure || mitigationResult.Value is null)
            {
                _logger.LogApplicationWarning("Risk-level rescore notification skipped for hazard {HazardCode}: mitigation lookup failed.", hazard.Code);
                return;
            }

            var pendingMitigations = mitigationResult.Value
                .Where(m => m.Status == MitigationStatus.PendingApproval)
                .ToList();

            if (pendingMitigations.Count == 0)
            {
                return;
            }

            var riskLevel = hazard.HazardRiskLevel ?? RiskLevel.Low;
            var requiredApprovers = await ResolveApproverCodesAsync(riskLevel, ct).ConfigureAwait(false);
            if (requiredApprovers.Count == 0)
            {
                _logger.LogApplicationWarning(
                    "Risk-level rescore notification skipped for hazard {HazardCode}: no approvers found for required authority {AuthorityLevel}.",
                    hazard.Code,
                    riskLevel.RequiredAuthorityLevel);
                return;
            }

            var publishCount = 0;
            var requestedBy = !string.IsNullOrWhiteSpace(hazard.UpdatedBy) ? hazard.UpdatedBy : hazard.CreatedBy ?? string.Empty;
            var requestDate = DateTime.UtcNow;

            foreach (var mitigation in pendingMitigations)
            {
                var approvalEvent = new MitigationApprovalRequestedEvent(
                    id: new SMSEventID("EV-0000"),
                    mitigationId: mitigation.Id.Value,
                    mitigationCode: mitigation.Code,
                    hazardId: mitigation.HazardCode,
                    hazardCode: mitigation.HazardCode,
                    mitigationDescription: mitigation.Description ?? mitigation.Name ?? "Mitigation requires approval",
                    priority: MapMitigationPriority(mitigation),
                    requestedBy: requestedBy,
                    requestDate: requestDate,
                    proposedImplementationDate: mitigation.TargetDate ?? requestDate.AddDays(14),
                    requiredApprovers: requiredApprovers,
                    approvalJustification: "Hazard risk level rescored after assessment submission; mitigation approval recipients recalculated.",
                    estimatedCost: mitigation.EstimatedCost,
                    resourceRequirements: mitigation.ResourceRequirements ?? string.Empty,
                    estimatedImplementationTime: mitigation.EstimatedHours.HasValue ? TimeSpan.FromHours(mitigation.EstimatedHours.Value) : null,
                    requiresExecutiveApproval: (mitigation.EstimatedCost ?? 0m) >= 25000m,
                    escalationPath: "SafetyManager->SafetyDirector")
                {
                    ReportId = hazard.ReportCode ?? string.Empty
                };

                var publishResult = await _eventBus.PublishDomainEventAsync(approvalEvent, ct).ConfigureAwait(false);
                if (publishResult.IsSuccess)
                {
                    publishCount++;
                }
                else
                {
                    _logger.LogApplicationWarning(
                        "Failed to publish mitigation approval request after hazard rescore for mitigation {MitigationCode}: {Error}",
                        mitigation.Code,
                        publishResult.Error?.Message ?? "Unknown publish error");
                }
            }

            _logger.LogApplicationInformation(
                "Hazard {HazardCode} risk-level change triggered {PublishCount} mitigation approval request event(s) using current authority thresholds.",
                hazard.Code,
                publishCount);
        }

        private async Task<List<string>> ResolveApproverCodesAsync(RiskLevel riskLevel, CancellationToken ct)
        {
            var groupsResult = await _mediator.SendAsync(new GetAllSMSOrganizationalGroupsQuery(), ct).ConfigureAwait(false);
            if (groupsResult.IsSuccess && groupsResult.Value is not null)
            {
                var eligibleGroups = groupsResult.Value
                    .Where(g => g.IsActive)
                    .Select(g => new
                    {
                        GroupCode = g.Code?.Trim() ?? string.Empty,
                        Authority = g.EffectiveAuthorityLevel
                    })
                    .Where(g => !string.IsNullOrWhiteSpace(g.GroupCode))
                    .Where(g => g.Authority >= riskLevel.RequiredAuthorityLevel)
                    .ToList();

                if (eligibleGroups.Count > 0)
                {
                    var minimumAuthority = eligibleGroups.Min(g => g.Authority);
                    return eligibleGroups
                        .Where(g => g.Authority == minimumAuthority)
                        .Select(g => g.GroupCode)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }
            }

            var usersResult = await _mediator.SendAsync(new GetAllSMSOrganizationalUsersQuery(), ct).ConfigureAwait(false);
            if (usersResult.IsFailure || usersResult.Value is null)
            {
                return new List<string>();
            }

            var approvers = new List<string>();
            var approverAuthority = new List<(string Code, int Authority)>();
            foreach (var user in usersResult.Value.Where(u => u.IsActive && !string.IsNullOrWhiteSpace(u.Code)))
            {
                var effectiveAuthorityLevel = await GetEffectiveAuthorityLevelAsync(user, ct).ConfigureAwait(false);
                if (effectiveAuthorityLevel >= riskLevel.RequiredAuthorityLevel)
                {
                    approverAuthority.Add((user.Code.Trim(), effectiveAuthorityLevel));
                }
            }

            if (approverAuthority.Count == 0)
            {
                return new List<string>();
            }

            var minimumQualifiedAuthority = approverAuthority.Min(a => a.Authority);

            approvers.AddRange(approverAuthority
                .Where(a => a.Authority == minimumQualifiedAuthority)
                .Select(a => a.Code));

            return approvers
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private async Task<int> GetEffectiveAuthorityLevelAsync(SMSOrganizationalUser user, CancellationToken ct)
        {
            var authorityCandidates = new List<int>();

            if (user.AuthorityLevel.HasValue)
            {
                authorityCandidates.Add(user.AuthorityLevel.Value);
            }

            if (user.OrganizationLevel is not null)
            {
                authorityCandidates.Add(user.OrganizationLevel.AuthorityLevel);
            }

            var groupsResult = await _mediator.SendAsync(new GetSMSOrganizationalGroupsByUserCodeQuery(user.Code), ct).ConfigureAwait(false);
            if (groupsResult.IsSuccess && groupsResult.Value is not null)
            {
                authorityCandidates.AddRange(groupsResult.Value
                    .Where(g => g.IsActive)
                    .Select(g => g.EffectiveAuthorityLevel));
            }

            return authorityCandidates.DefaultIfEmpty(0).Max();
        }

        private static bool HasHazardRiskLevelChanged(string? previousRiskLevelValue, string? currentRiskLevelValue)
        {
            var previous = previousRiskLevelValue?.Trim() ?? string.Empty;
            var current = currentRiskLevelValue?.Trim() ?? string.Empty;
            return !string.Equals(previous, current, StringComparison.OrdinalIgnoreCase);
        }

        private static SMS_Domain.Events.MitigationPriority MapMitigationPriority(Mitigation mitigation)
        {
            var progress = mitigation.Progress;
            var cost = mitigation.EstimatedCost ?? 0m;

            if (cost >= 100000m)
            {
                return SMS_Domain.Events.MitigationPriority.Emergency;
            }

            if (cost >= 50000m)
            {
                return SMS_Domain.Events.MitigationPriority.Critical;
            }

            if (cost >= 25000m || progress > 50)
            {
                return SMS_Domain.Events.MitigationPriority.High;
            }

            if (cost >= 10000m)
            {
                return SMS_Domain.Events.MitigationPriority.Medium;
            }

            return SMS_Domain.Events.MitigationPriority.Low;
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
            var eventid = new SMSEventID("EV-0000");
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
                    created.CreatedBy = !string.IsNullOrWhiteSpace(hazard.CreatedBy) ? hazard.CreatedBy : string.Empty;
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
                    updated.UpdatedBy = !string.IsNullOrWhiteSpace(hazard.UpdatedBy) ? hazard.UpdatedBy : hazard.CreatedBy ?? string.Empty;
                    updated.UpdatedDate = DateTime.UtcNow;
                }
                if (evt is HazardDeletedEvent deleted)
                {
                    deleted.ReportId = hazard.ReportCode;
                    deleted.HazardId = hazard.Code;
                }
                return evt;
            }

            var eventResult = await eventBus.PublishDomainEventAsync(hazardEvent, EventExecutionMode.Immediate); // changed from Manual to Imediate 06_24_2026

            if (!eventResult.IsSuccess)
            {
                logger.LogApplicationWarning("[COMMAND HANDLER] Failed to publish Event for {HazardCode}: {Error} - continuing with command execution", hazard.Code, eventResult.Error.Message);
            }
        }
    }









}




    

