//-----------------------------------------------------------------------
// <copyright file="RiskAssessmentCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers implementing SMS risk assessment and analysis logic.
//                  Implements command handlers for processing write operations.
//                  Handles business logic execution and domain entity coordination.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Domain.Events;
using SMS_Application.Queries;

using Microsoft.Extensions.Logging;

namespace SMS_Application.CommandHandlers;

// =============================================
// RISK ASSESSMENT COMMAND HANDLERS - Clean Architecture Pattern
// =============================================

public class CreateRiskAssessmentCommandHandler : BaseCommandBundle, IBaseRequestHandler<CreateRiskAssessmentCommand, Result<RiskAssessment>>
{
    private readonly IRiskAssessmentService _riskAssessmentService;
    private readonly ILogger<CreateRiskAssessmentCommandHandler> _logger;

    public CreateRiskAssessmentCommandHandler(IRiskAssessmentService riskAssessmentService, ILogger<CreateRiskAssessmentCommandHandler> logger)
    {
        _riskAssessmentService = riskAssessmentService ?? throw new ArgumentNullException(nameof(riskAssessmentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAssessment>> HandleAsync(CreateRiskAssessmentCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.RiskAssessment is null)
            {
                _logger.LogApplicationError("CreateRiskAssessmentCommand received with null RiskAssessment", ApplicationEventIds.Error, null);
                return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing CreateRiskAssessmentCommand for Code: {Code}", request.RiskAssessment.Code);

            var result = await _riskAssessmentService.CreateRiskAssessmentAsync(request.RiskAssessment, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully created RiskAssessment with ID: {Id}, Code: {Code}",
                    result.Value?.Id, result.Value?.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to create RiskAssessment with Code: {Code}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("CreateRiskAssessmentCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating RiskAssessment", ApplicationEventIds.Error, ex);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.CreateFailed);
        }
    }
}

public class UpdateRiskAssessmentCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateRiskAssessmentCommand, Result<RiskAssessment>>
{
    private readonly IRiskAssessmentService _riskAssessmentService;
    private readonly IBaseMediator _mediator;
    private readonly IBaseEventBus _eventBus;
    private readonly ILogger<UpdateRiskAssessmentCommandHandler> _logger;

    public UpdateRiskAssessmentCommandHandler(
        IRiskAssessmentService riskAssessmentService,
        IBaseMediator mediator,
        IBaseEventBus eventBus,
        ILogger<UpdateRiskAssessmentCommandHandler> logger)
    {
        _riskAssessmentService = riskAssessmentService ?? throw new ArgumentNullException(nameof(riskAssessmentService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAssessment>> HandleAsync(UpdateRiskAssessmentCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.RiskAssessment is null)
            {
                _logger.LogApplicationError("UpdateRiskAssessmentCommand received with null RiskAssessment", ApplicationEventIds.Error, null);
                return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NullOrEmpty);
            }

            RiskAssessmentStatus? previousStatus = null;
            RiskAssessmentStage? previousStage = null;
            var existingResult = await _riskAssessmentService.GetRiskAssessmentByIdAsync(new RiskAssessmentID(request.RiskAssessment.Id.Value), ct).ConfigureAwait(false);
            if (existingResult.IsSuccess && existingResult.Value is not null)
            {
                previousStatus = existingResult.Value.Status;
                previousStage = existingResult.Value.Stage;
            }

            _logger.LogApplicationInformation(" Processing UpdateRiskAssessmentCommand for ID: {Id}, Code: {Code}",
                request.RiskAssessment.Id, request.RiskAssessment.Code);

            var result = await _riskAssessmentService.UpdateRiskAssessmentAsync(request.RiskAssessment, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully updated RiskAssessment with ID: {Id}", request.RiskAssessment.Id);

                var assessment = result.Value;
                if (assessment is not null)
                {
                    var updatedEvent = new RiskAssessmentUpdatedEvent(
                        id: new SMSEventID("EV-0000"),
                        riskAssessmentId: assessment.Id.Value,
                        updatedBy: assessment.UpdatedBy ?? assessment.CreatedBy ?? string.Empty,
                        updatedDate: assessment.UpdatedDate ?? DateTime.UtcNow)
                    {
                        ReportId = assessment.ReportCode ?? string.Empty
                    };

                    var updatePublishResult = await _eventBus.PublishDomainEventAsync(updatedEvent, EventExecutionMode.Immediate, ct).ConfigureAwait(false);
                    if (updatePublishResult.IsFailure)
                    {
                        _logger.LogApplicationWarning("Failed to publish RiskAssessmentUpdated event for {AssessmentCode}: {Error}",
                            assessment.Code,
                            updatePublishResult.Error?.Message ?? "Unknown publish error");
                    }

                    if (previousStatus is not null && !previousStatus.Equals(assessment.Status))
                    {
                        await TransitionEventPublisher.PublishIfChangedAsync(
                            _eventBus,
                            previousStatus,
                            assessment.Status,
                            () => new RiskAssessmentStatusChangedEvent(
                                id: new SMSEventID("EV-0000"),
                                riskAssessmentId: assessment.Id.Value,
                                riskAssessmentCode: assessment.Code,
                                previousStatus: previousStatus,
                                newStatus: assessment.Status,
                                changedBy: assessment.UpdatedBy ?? assessment.CreatedBy ?? string.Empty,
                                changedDate: assessment.UpdatedDate ?? DateTime.UtcNow),
                            ct).ConfigureAwait(false);
                    }

                    if (previousStage is not null && !previousStage.Equals(assessment.Stage))
                    {
                        await TransitionEventPublisher.PublishIfChangedAsync(
                            _eventBus,
                            previousStage,
                            assessment.Stage,
                            () => new RiskAssessmentStageChangedEvent(
                                id: new SMSEventID("EV-0000"),
                                riskAssessmentId: assessment.Id.Value,
                                riskAssessmentCode: assessment.Code,
                                previousStage: previousStage,
                                newStage: assessment.Stage,
                                changedBy: assessment.UpdatedBy ?? assessment.CreatedBy ?? string.Empty,
                                changedDate: assessment.UpdatedDate ?? DateTime.UtcNow),
                            ct).ConfigureAwait(false);
                    }
                }

                if (assessment is not null &&
                    previousStatus is not null &&
                    previousStatus != RiskAssessmentStatus.AssessmentComplete &&
                    assessment.Status == RiskAssessmentStatus.AssessmentComplete)
                {
                    var completedDate = assessment.CompletedDate ?? assessment.UpdatedDate ?? DateTime.UtcNow;
                    var assessmentStartDate = assessment.CreatedDate ?? completedDate.AddDays(-7);
                    var targetCompletionDate = assessmentStartDate.AddDays(14);

                    var completionEvent = new RiskAssessmentCompletedEvent(
                        new SMSEventID("EV-0000"),
                        assessment.Id.Value,
                        assessment.Code ?? assessment.Id.Value,
                        assessmentStartDate,
                        targetCompletionDate,
                        completedDate,
                        assessment.ReportCode ?? string.Empty)
                    {
                        HazardId = assessment.HazardCode ?? assessment.PrimaryHazardId ?? string.Empty,
                        ReportId = assessment.ReportCode ?? string.Empty,
                        RiskLevel = ResolveRiskLevel(assessment.FinalRiskLevel),
                        RiskScore = assessment.FinalSeverityScore.HasValue && assessment.FinalLikelihoodScore.HasValue
                            ? assessment.FinalSeverityScore.Value * assessment.FinalLikelihoodScore.Value
                            : 0,
                        AssessmentType = assessment.AssessmentType?.Value ?? "Technical"
                    };

                    var completionPublishResult = await _eventBus.PublishDomainEventAsync(completionEvent, ct).ConfigureAwait(false);
                    if (completionPublishResult.IsFailure)
                    {
                        _logger.LogApplicationWarning("Failed to publish RiskAssessmentCompleted event for {AssessmentCode}: {Error}",
                            assessment.Code,
                            completionPublishResult.Error?.Message ?? "Unknown publish error");
                    }

                    if (completionEvent.RiskLevel is not null &&
                        (completionEvent.RiskLevel == RiskLevel.Critical || completionEvent.RiskLevel == RiskLevel.High))
                    {
                        var highRiskEvent = new HighRiskIdentifiedEvent(new SMSEventID("EV-0000"))
                        {
                            AssessmentId = assessment.Code ?? assessment.Id.Value,
                            ReportId = assessment.ReportCode ?? string.Empty,
                            RiskLevel = completionEvent.RiskLevel,
                            RiskScore = completionEvent.RiskScore,
                            IdentifiedDate = completedDate,
                            RiskDescription = $"Technical assessment identified {completionEvent.RiskLevel.Value} risk level"
                        };

                        var highRiskPublishResult = await _eventBus.PublishDomainEventAsync(highRiskEvent, ct).ConfigureAwait(false);
                        if (highRiskPublishResult.IsFailure)
                        {
                            _logger.LogApplicationWarning("Failed to publish HighRiskIdentified event for {AssessmentCode}: {Error}",
                                assessment.Code,
                                highRiskPublishResult.Error?.Message ?? "Unknown publish error");
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(assessment.HazardCode))
                    {
                        var mitigationResult = await _mediator
                            .SendAsync(new GetMitigationsByHazardCodeQuery(assessment.HazardCode), ct)
                            .ConfigureAwait(false);

                        if (mitigationResult.IsSuccess && mitigationResult.Value is not null)
                        {
                            var pendingMitigations = mitigationResult.Value
                                .Where(m => m.Status == MitigationStatus.PendingApproval)
                                .ToList();

                            if (pendingMitigations.Count > 0)
                            {
                                var approverEmails = await ResolveApproverEmailsAsync(assessment.HazardCode, ct).ConfigureAwait(false);

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
                                        requestedBy: assessment.UpdatedBy ?? assessment.CreatedBy ?? string.Empty,
                                        requestDate: completedDate,
                                        proposedImplementationDate: mitigation.TargetDate ?? completedDate.AddDays(14),
                                        requiredApprovers: approverEmails,
                                        approvalJustification: "Risk assessment submitted; mitigation approval required.",
                                        estimatedCost: mitigation.EstimatedCost,
                                        resourceRequirements: mitigation.ResourceRequirements ?? string.Empty,
                                        estimatedImplementationTime: mitigation.EstimatedHours.HasValue ? TimeSpan.FromHours(mitigation.EstimatedHours.Value) : null,
                                        requiresExecutiveApproval: (mitigation.EstimatedCost ?? 0m) >= 25000m,
                                        escalationPath: "SafetyManager->SafetyDirector")
                                    {
                                        ReportId = assessment.ReportCode ?? string.Empty
                                    };

                                    var approvalPublishResult = await _eventBus.PublishDomainEventAsync(approvalEvent, ct).ConfigureAwait(false);
                                    if (approvalPublishResult.IsFailure)
                                    {
                                        _logger.LogApplicationWarning("Failed to publish MitigationApprovalRequested event for {MitigationCode}: {Error}",
                                            mitigation.Code,
                                            approvalPublishResult.Error?.Message ?? "Unknown publish error");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                _logger.LogApplicationError("Failed to update RiskAssessment with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("UpdateRiskAssessmentCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating RiskAssessment with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    private static RiskLevel ResolveRiskLevel(string? riskLevel)
    {
        if (string.IsNullOrWhiteSpace(riskLevel))
        {
            return RiskLevel.Low;
        }

        return RiskLevel.GetAllValues().FirstOrDefault(level =>
                   level.Value.Equals(riskLevel, StringComparison.OrdinalIgnoreCase) ||
                   level.Name.Equals(riskLevel, StringComparison.OrdinalIgnoreCase))
               ?? RiskLevel.Low;
    }

    private async Task<List<string>> ResolveApproverEmailsAsync(string hazardCode, CancellationToken ct)
    {
        var hazardResult = await _mediator.SendAsync(new GetHazardByCodeQuery(new HazardID(hazardCode)), ct).ConfigureAwait(false);
        var riskLevel = hazardResult.IsSuccess && hazardResult.Value is not null
            ? hazardResult.Value.HazardRiskLevel
            : RiskLevel.Low;

        var groupsResult = await _mediator.SendAsync(new GetAllSMSOrganizationalGroupsQuery(), ct).ConfigureAwait(false);
        if (groupsResult.IsSuccess && groupsResult.Value is not null)
        {
            var eligibleGroups = groupsResult.Value
                .Where(g => g.IsActive)
                .Select(g => new
                {
                    GroupCode = g.Code?.Trim() ?? string.Empty,
                    Authority = ConvertGroupAuthorityToNumericLevel(g.AuthorityLevel)
                })
                .Where(g => !string.IsNullOrWhiteSpace(g.GroupCode))
                .Where(g => g.Authority >= riskLevel.RequiredAuthorityLevel)
                .ToList();

            if (eligibleGroups.Count > 0)
            {
                var minimumGroupAuthority = eligibleGroups.Min(g => g.Authority);
                return eligibleGroups
                    .Where(g => g.Authority == minimumGroupAuthority)
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

        var qualifiedUsers = usersResult.Value
            .Where(u => u.IsActive && CanApproveRiskLevel(u, riskLevel))
            .ToList();

        if (qualifiedUsers.Count == 0)
        {
            return new List<string>();
        }

        var minimumAuthority = qualifiedUsers
            .Select(GetAuthorityLevel)
            .DefaultIfEmpty(int.MaxValue)
            .Min();

        return qualifiedUsers
            .Where(u => GetAuthorityLevel(u) == minimumAuthority)
            .Select(u => u.Code)
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static int ConvertGroupAuthorityToNumericLevel(string? authorityLevel)
    {
        if (string.IsNullOrWhiteSpace(authorityLevel))
        {
            return 0;
        }

        return authorityLevel.Trim().ToUpperInvariant() switch
        {
            "EXECUTIVE" => 10,
            "STRATEGIC" => 9,
            "OPERATIONAL" => 8,
            "PROCESS" => 7,
            "SUPPORT" => 6,
            "STANDARD" => 5,
            _ => 0
        };
    }

    private static int GetAuthorityLevel(SMSOrganizationalUser user)
    {
        return user.AuthorityLevel
            ?? user.OrganizationLevel?.AuthorityLevel
            ?? int.MaxValue;
    }

    private static bool CanApproveRiskLevel(SMSOrganizationalUser user, RiskLevel riskLevel)
    {
        if (user.AuthorityLevel.HasValue && user.AuthorityLevel.Value >= riskLevel.RequiredAuthorityLevel)
        {
            return true;
        }

        if (user.OrganizationLevel?.AuthorityLevel >= riskLevel.RequiredAuthorityLevel)
        {
            return true;
        }

        if (user.OrganizationLevel is not null && riskLevel.ApproverRoles.Contains(user.OrganizationLevel.Value))
        {
            return true;
        }

        var userRiskAuthority = user.RiskApprovalAuthority?.ToUpperInvariant();
        if (!string.IsNullOrEmpty(userRiskAuthority))
        {
            return userRiskAuthority.Contains(riskLevel.Value.ToUpperInvariant());
        }

        return false;
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

public class DeleteRiskAssessmentCommandHandler : BaseCommandBundle, IBaseRequestHandler<DeleteRiskAssessmentCommand, Result<bool>>
{
    private readonly IRiskAssessmentService _riskAssessmentService;
    private readonly ILogger<DeleteRiskAssessmentCommandHandler> _logger;

    public DeleteRiskAssessmentCommandHandler(IRiskAssessmentService riskAssessmentService, ILogger<DeleteRiskAssessmentCommandHandler> logger)
    {
        _riskAssessmentService = riskAssessmentService ?? throw new ArgumentNullException(nameof(riskAssessmentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteRiskAssessmentCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.RiskAssessmentId is null)
            {
                _logger.LogApplicationError("DeleteRiskAssessmentCommand received with null RiskAssessmentId", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing DeleteRiskAssessmentCommand for ID: {Id}", request.RiskAssessmentId);

            var result = await _riskAssessmentService.DeleteRiskAssessmentAsync(request.RiskAssessmentId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully deleted RiskAssessment with ID: {Id}", request.RiskAssessmentId);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete RiskAssessment with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("DeleteRiskAssessmentCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deleting RiskAssessment with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.DeleteFailed);
        }
    }
}

// =============================================
// STEP-SPECIFIC COMMAND HANDLERS FOR STEPS 1-5 - Clean Architecture Pattern
// =============================================

public class SaveStep1CommandHandler : BaseCommandBundle, IBaseRequestHandler<SaveStep1Command, Result<RiskAssessment>>
{
    private readonly IRiskAssessmentService _riskAssessmentService;
    private readonly ILogger<SaveStep1CommandHandler> _logger;

    public SaveStep1CommandHandler(IRiskAssessmentService riskAssessmentService, ILogger<SaveStep1CommandHandler> logger)
    {
        _riskAssessmentService = riskAssessmentService ?? throw new ArgumentNullException(nameof(riskAssessmentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAssessment>> HandleAsync(SaveStep1Command request, CancellationToken ct = default)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("SaveStep1Command received with null request", ApplicationEventIds.Error, null);
                return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing SaveStep1Command for RiskAssessment: {Id}", request.RiskAssessmentId.Value);

            // Get the existing assessment first
            var assessmentResult = await _riskAssessmentService.GetRiskAssessmentByIdAsync(request.RiskAssessmentId, ct);
            if (assessmentResult.IsFailure)
            {
                return Result<RiskAssessment>.Failure<RiskAssessment>(assessmentResult.Error);
            }

            var assessment = assessmentResult.Value;

            // Update the assessment with Step 1 data
            assessment.LeadAssessorId = request.LeadAssessorId;
            assessment.SystemDescription = request.SystemDescription;
            assessment.SystemBoundaries = request.SystemBoundaries;
            assessment.SystemPurpose = request.SystemPurpose;
            assessment.FiveMPersonnel = request.FiveMPersonnel;
            assessment.FiveMEquipment = request.FiveMEquipment;
            assessment.FiveMProcedures = request.FiveMProcedures;
            assessment.FiveMResources = request.FiveMResources;
            assessment.FiveMPhysicalEnvironment = request.FiveMPhysicalEnvironment;
            assessment.FiveMOperationalEnvironment = request.FiveMOperationalEnvironment;
            assessment.UpdatedBy = request.UpdatedBy;
            assessment.UpdatedDate = DateTime.UtcNow;

            var result = await _riskAssessmentService.UpdateRiskAssessmentAsync(assessment, ct);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully saved Step 1 for RiskAssessment: {Id}", request.RiskAssessmentId);
            }
            else
            {
                _logger.LogApplicationError("Failed to save Step 1 for RiskAssessment: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("SaveStep1Command operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while saving Step 1 for RiskAssessment: {Id}", ApplicationEventIds.Error, ex);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }
}

public class SaveStep4CommandHandler : BaseCommandBundle, IBaseRequestHandler<SaveStep4Command, Result<RiskAssessment>>
{
    private readonly IRiskAssessmentService _riskAssessmentService;
    private readonly ILogger<SaveStep4CommandHandler> _logger;

    public SaveStep4CommandHandler(IRiskAssessmentService riskAssessmentService, ILogger<SaveStep4CommandHandler> logger)
    {
        _riskAssessmentService = riskAssessmentService ?? throw new ArgumentNullException(nameof(riskAssessmentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAssessment>> HandleAsync(SaveStep4Command request, CancellationToken ct = default)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("SaveStep4Command received with null request", ApplicationEventIds.Error, null);
                return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing SaveStep4Command for RiskAssessment: {Id}", request.RiskAssessmentId.Value);

            // Get the existing assessment first
            var assessmentResult = await _riskAssessmentService.GetRiskAssessmentByIdAsync(request.RiskAssessmentId, ct);
            if (assessmentResult.IsFailure)
            {
                return Result<RiskAssessment>.Failure<RiskAssessment>(assessmentResult.Error);
            }

            var assessment = assessmentResult.Value;

            // Update the assessment with Step 4 data
            assessment.FinalSeverityScore = request.FinalSeverityScore;
            assessment.FinalLikelihoodScore = request.FinalLikelihoodScore;
            assessment.FinalRiskLevel = request.FinalRiskLevel;
            assessment.AdditionalComments = request.AdditionalComments;
            assessment.UpdatedBy = request.UpdatedBy;
            assessment.UpdatedDate = DateTime.UtcNow;

            var result = await _riskAssessmentService.UpdateRiskAssessmentAsync(assessment, ct);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully saved Step 4 for RiskAssessment: {Id}", request.RiskAssessmentId);
            }
            else
            {
                _logger.LogApplicationError("Failed to save Step 4 for RiskAssessment: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("SaveStep4Command operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while saving Step 4 for RiskAssessment: {Id}", ApplicationEventIds.Error, ex);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }
}

public class SaveStep5CommandHandler : BaseCommandBundle, IBaseRequestHandler<SaveStep5Command, Result<RiskAssessment>>
{
    private readonly IRiskAssessmentService _riskAssessmentService;
    private readonly ILogger<SaveStep5CommandHandler> _logger;

    public SaveStep5CommandHandler(IRiskAssessmentService riskAssessmentService, ILogger<SaveStep5CommandHandler> logger)
    {
        _riskAssessmentService = riskAssessmentService ?? throw new ArgumentNullException(nameof(riskAssessmentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAssessment>> HandleAsync(SaveStep5Command request, CancellationToken ct = default)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("SaveStep5Command received with null request", ApplicationEventIds.Error, null);
                return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing SaveStep5Command for RiskAssessment: {Id}", request.RiskAssessmentId);

            // Get the existing assessment first
            var assessmentResult = await _riskAssessmentService.GetRiskAssessmentByIdAsync(request.RiskAssessmentId, ct);
            if (assessmentResult.IsFailure)
            {
                return Result<RiskAssessment>.Failure<RiskAssessment>(assessmentResult.Error);
            }

            var assessment = assessmentResult.Value;

            // Update the assessment with Step 5 data
            assessment.UpdatedBy = request.UpdatedBy;
            assessment.UpdatedDate = DateTime.UtcNow;

            var result = await _riskAssessmentService.UpdateRiskAssessmentAsync(assessment, ct);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully saved Step 5 for RiskAssessment: {Id}", request.RiskAssessmentId);
            }
            else
            {
                _logger.LogApplicationError("Failed to save Step 5 for RiskAssessment: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("SaveStep5Command operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while saving Step 5 for RiskAssessment: {Id}", ApplicationEventIds.Error, ex);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }
}

public class UpdateProgressCommandHandler : BaseCommandBundle, IBaseRequestHandler<UpdateProgressCommand, Result<RiskAssessment>>
{
    private readonly IRiskAssessmentService _riskAssessmentService;
    private readonly ILogger<UpdateProgressCommandHandler> _logger;

    public UpdateProgressCommandHandler(IRiskAssessmentService riskAssessmentService, ILogger<UpdateProgressCommandHandler> logger)
    {
        _riskAssessmentService = riskAssessmentService ?? throw new ArgumentNullException(nameof(riskAssessmentService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RiskAssessment>> HandleAsync(UpdateProgressCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request is null)
            {
                _logger.LogApplicationError("UpdateProgressCommand received with null request", ApplicationEventIds.Error, null);
                return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NullOrEmpty);
            }

            _logger.LogApplicationInformation(" Processing UpdateProgressCommand for RiskAssessment: {Id}, Step: {Step}",
                request.RiskAssessmentId, request.CurrentStep);

            // Get the existing assessment first
            var assessmentResult = await _riskAssessmentService.GetRiskAssessmentByIdAsync(request.RiskAssessmentId, ct);
            if (assessmentResult.IsFailure)
            {
                return Result<RiskAssessment>.Failure<RiskAssessment>(assessmentResult.Error);
            }

            var assessment = assessmentResult.Value;

            // Update the assessment with progress data
            assessment.CurrentStep = request.CurrentStep;

            // Handle Smart Enumeration status conversion using the proper method
            var statusFromString = RiskAssessmentStatus.GetAllValues()
                .FirstOrDefault(s => s.Value == request.Status || s.Name == request.Status);
            if (statusFromString != null)
            {
                assessment.Status = statusFromString;
            }

            // Handle Smart Enumeration stage conversion using the proper method  
            var stageFromString = RiskAssessmentStage.GetAllValues()
                .FirstOrDefault(s => s.Value == request.Stage || s.Name == request.Stage);
            if (stageFromString != null)
            {
                assessment.Stage = stageFromString;
            }

            // Use the CompleteStep method which will handle status and stage updates based on business rules
            assessment.CompleteStep(request.CurrentStep);

            assessment.UpdatedBy = request.UpdatedBy;
            assessment.UpdatedDate = DateTime.UtcNow;

            var result = await _riskAssessmentService.UpdateRiskAssessmentAsync(assessment, ct);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation(" Successfully updated progress for RiskAssessment: {Id}", request.RiskAssessmentId);
            }
            else
            {
                _logger.LogApplicationError("Failed to update progress for RiskAssessment: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("UpdateProgressCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating progress for RiskAssessment: {Id}", ApplicationEventIds.Error, ex);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }
}

