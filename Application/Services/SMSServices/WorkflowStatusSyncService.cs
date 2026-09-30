//-----------------------------------------------------------------------
// <copyright file="WorkflowStatusSyncService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Synchronizes root report and initial-hazard statuses from related workflow entities.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Application.Commands;
using SMS_Application.Queries;
using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS_Application.Services;

public sealed class WorkflowStatusSyncService
{
    private readonly IBaseMediator _mediator;
    private readonly ReportService _reportService;
    private readonly HazardService _hazardService;
    private readonly InvestigationService _investigationService;
    private readonly InvestigationDataService _investigationDataService;
    private readonly ILogger<WorkflowStatusSyncService> _logger;

    public WorkflowStatusSyncService(
        IBaseMediator mediator,
        ReportService reportService,
        HazardService hazardService,
        InvestigationService investigationService,
        InvestigationDataService investigationDataService,
        ILogger<WorkflowStatusSyncService> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        _hazardService = hazardService ?? throw new ArgumentNullException(nameof(hazardService));
        _investigationService = investigationService ?? throw new ArgumentNullException(nameof(investigationService));
        _investigationDataService = investigationDataService ?? throw new ArgumentNullException(nameof(investigationDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> SyncForHazardAsync(string hazardCode, string? updatedBy, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hazardCode))
            {
                return Result.Failure(new Error("WORKFLOW_SYNC_INVALID_HAZARD", "Hazard code is required for workflow sync."));
            }

            var normalizedHazardCode = hazardCode.Trim();

            var sourceHazardResult = await _mediator
                .SendAsync(new GetHazardByCodeQuery(new HazardID(normalizedHazardCode)), ct)
                .ConfigureAwait(false);

            if (sourceHazardResult.IsFailure || sourceHazardResult.Value is null)
            {
                return Result.Failure(sourceHazardResult.Error);
            }

            var sourceHazard = sourceHazardResult.Value;
            var reportCode = sourceHazard.ReportCode?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(reportCode))
            {
                return Result.Success();
            }

            var hazardsResult = await _mediator
                .SendAsync(new GetHazardsByReportCodeQuery(new ReportID(reportCode)), ct)
                .ConfigureAwait(false);

            if (hazardsResult.IsFailure || hazardsResult.Value is null)
            {
                return Result.Failure(hazardsResult.Error);
            }

            var initialHazard = hazardsResult.Value.FirstOrDefault(h => h.IsInitialHazard);
            if (initialHazard is null)
            {
                return Result.Success();
            }

            var assessmentsResult = await _mediator
                .SendAsync(new GetRiskAssessmentsByHazardCodeQuery(new HazardID(normalizedHazardCode)), ct)
                .ConfigureAwait(false);

            var riskAssessments = assessmentsResult.IsSuccess && assessmentsResult.Value is not null
                ? assessmentsResult.Value.ToList()
                : new List<RiskAssessment>();

            var investigationsResult = await _investigationDataService
                .GetByHazardCodeAsync(normalizedHazardCode, ct)
                .ConfigureAwait(false);

            var investigations = investigationsResult.IsSuccess && investigationsResult.Value is not null
                ? investigationsResult.Value.ToList()
                : new List<Investigation>();

            var hasCompletedAssessment = riskAssessments.Any(ra => ra.Status == RiskAssessmentStatus.AssessmentComplete);
            if (hasCompletedAssessment)
            {
                foreach (var investigation in investigations.Where(i => i.Status != InvestigationStatus.InvestigationComplete))
                {
                    investigation.Status = InvestigationStatus.InvestigationComplete;
                    investigation.CompletedDate ??= DateTime.Now;
                    investigation.UpdatedBy = ResolveActor(updatedBy, investigation.UpdatedBy, investigation.CreatedBy);
                    investigation.UpdatedDate = DateTime.Now;

                    var updateInvestigationResult = await _investigationService
                        .UpdateInvestigationAsync(investigation, ct)
                        .ConfigureAwait(false);

                    if (updateInvestigationResult.IsFailure)
                    {
                        _logger.LogApplicationWarning(
                            "Workflow sync could not complete investigation {InvestigationCode} for hazard {HazardCode}: {Error}",
                            investigation.Code,
                            normalizedHazardCode,
                            updateInvestigationResult.Error?.Message ?? "Unknown error");
                    }
                }

                investigations = investigations
                    .Select(i =>
                    {
                        if (i.Status == InvestigationStatus.InvestigationComplete)
                        {
                            return i;
                        }

                        i.Status = InvestigationStatus.InvestigationComplete;
                        i.CompletedDate ??= DateTime.Now;
                        return i;
                    })
                    .ToList();
            }

            var mitigationsResult = await _mediator
                .SendAsync(new GetMitigationsByHazardCodeQuery(normalizedHazardCode), ct)
                .ConfigureAwait(false);

            var mitigations = mitigationsResult.IsSuccess && mitigationsResult.Value is not null
                ? mitigationsResult.Value.ToList()
                : new List<Mitigation>();

            var reportResult = await _mediator
                .SendAsync(new GetReportByCodeQuery(new ReportID(reportCode)), ct)
                .ConfigureAwait(false);

            var reportValidationResult = await _mediator
                .SendAsync(new GetReportValidationByReportIdQuery(new ReportID(reportCode)), ct)
                .ConfigureAwait(false);

            var currentReportStatus = reportResult.IsSuccess && reportResult.Value is not null
                ? reportResult.Value.Status
                : string.Empty;

            var validationDecision = reportValidationResult.IsSuccess && reportValidationResult.Value is not null
                ? reportValidationResult.Value.ValidationDecision
                : string.Empty;

            var targetStatuses = ResolveTargetStatuses(riskAssessments, investigations, mitigations, currentReportStatus, validationDecision);
            var actor = ResolveActor(updatedBy, sourceHazard.UpdatedBy, sourceHazard.CreatedBy);

            if (reportResult.IsSuccess && reportResult.Value is not null)
            {
                var normalizedCurrentReportStatus = reportResult.Value.Status?.Trim() ?? string.Empty;
                if (!string.Equals(normalizedCurrentReportStatus, targetStatuses.ReportStatus.Value, StringComparison.OrdinalIgnoreCase))
                {
                    var reportUpdateResult = await _reportService
                        .UpdateReportStatusAsync(reportCode, targetStatuses.ReportStatus, actor, ct)
                        .ConfigureAwait(false);

                    if (reportUpdateResult.IsFailure)
                    {
                        return Result.Failure(reportUpdateResult.Error);
                    }
                }
            }

            if (initialHazard.Status != targetStatuses.HazardStatus)
            {
                initialHazard.Status = targetStatuses.HazardStatus;
                initialHazard.UpdatedBy = actor;
                initialHazard.UpdatedDate = DateTime.Now;

                var hazardUpdateResult = await _hazardService.UpdateHazardAsync(initialHazard, ct).ConfigureAwait(false);
                if (hazardUpdateResult.IsFailure)
                {
                    return Result.Failure(hazardUpdateResult.Error);
                }
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error during workflow status sync for hazard {HazardCode}", hazardCode);
            return Result.Failure(new Error("WORKFLOW_SYNC_FAILED", $"Workflow status sync failed: {ex.Message}"));
        }
    }

    private static (ReportStatus ReportStatus, HazardStatus HazardStatus) ResolveTargetStatuses(
        IReadOnlyCollection<RiskAssessment> riskAssessments,
        IReadOnlyCollection<Investigation> investigations,
        IReadOnlyCollection<Mitigation> mitigations,
        string? currentReportStatusValue,
        string? validationDecisionValue)
    {
        if (string.Equals(validationDecisionValue?.Trim(), ValidationDecision.NotSmsRisk.Value, StringComparison.OrdinalIgnoreCase))
        {
            return (ReportStatus.ReportCloserNonSMSRisk, HazardStatus.HazardValidated);
        }

        if (mitigations.Any(m => m.Status == MitigationStatus.MitigationImplemented))
        {
            return (ReportStatus.MitigationComplete, HazardStatus.ResidualHazardScoring);
        }

        if (mitigations.Count > 0)
        {
            return (ReportStatus.InMitigation, HazardStatus.ResidualRiskMitigation);
        }

        var hasActiveInvestigation = investigations.Any(i =>
            i.Status == InvestigationStatus.InvestigatorAssigned ||
            i.Status == InvestigationStatus.InvestigationUnderway);

        if (hasActiveInvestigation)
        {
            return (ReportStatus.UnderInvestigation, HazardStatus.InitialRiskAnalysis);
        }

        if (riskAssessments.Count > 0)
        {
            var latestAssessment = riskAssessments
                .OrderByDescending(ra => ra.UpdatedDate ?? ra.CreatedDate ?? DateTime.MinValue)
                .First();

            var hazardStatus = MapRiskAssessmentStageToHazardStatus(latestAssessment.Stage);

            if (latestAssessment.Status == RiskAssessmentStatus.AssessmentComplete)
            {
                if (latestAssessment.AssessmentType == RiskAssessmentType.RiskRegistryOnly)
                {
                    return (ReportStatus.RiskRegistryOnly, hazardStatus);
                }

                return (ReportStatus.RiskAssessmentSubmitted, hazardStatus);
            }

            return (ReportStatus.RiskAssessmentInProgress, hazardStatus);
        }

        if (string.Equals(currentReportStatusValue?.Trim(), ReportStatus.ReadyForProcessing.Value, StringComparison.OrdinalIgnoreCase))
        {
            return (ReportStatus.ReadyForProcessing, HazardStatus.HazardValidated);
        }

        if (string.Equals(currentReportStatusValue?.Trim(), ReportStatus.NeedsValidation.Value, StringComparison.OrdinalIgnoreCase))
        {
            return (ReportStatus.NeedsValidation, HazardStatus.HazardValidationRequired);
        }

        return (ReportStatus.NeedsValidation, HazardStatus.HazardValidationRequired);
    }

    private static HazardStatus MapRiskAssessmentStageToHazardStatus(RiskAssessmentStage stage)
    {
        if (stage == RiskAssessmentStage.AnalyizingRisk)
        {
            return HazardStatus.InitialRiskAnalysis;
        }

        if (stage == RiskAssessmentStage.AssessingRisk)
        {
            return HazardStatus.InitialHazardScoring;
        }

        if (stage == RiskAssessmentStage.MitigatingRisk || stage == RiskAssessmentStage.Completed)
        {
            return HazardStatus.ResidualRiskAssessment;
        }

        return HazardStatus.InitialRiskAssessment;
    }

    private static string ResolveActor(string? preferred, string? secondary, string? fallback)
    {
        if (!string.IsNullOrWhiteSpace(preferred))
        {
            return preferred.Trim();
        }

        if (!string.IsNullOrWhiteSpace(secondary))
        {
            return secondary.Trim();
        }

        if (!string.IsNullOrWhiteSpace(fallback))
        {
            return fallback.Trim();
        }

        return string.Empty;
    }
}

