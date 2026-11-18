using SMS_Application.Interfaces;
using SMS_Domain.Enums;

namespace SMS_Application.Services;

public sealed class EventLifecycleRegistry : IEventLifecycleRegistry
{
    private static readonly IReadOnlyList<EventLifecycleEntry> EventLifecycleEntries =
    [
        new("HazardCreatedEvent", EventCategory.DomainEvent, EventLifecycleState.Active, "Published and handled in active production chain."),
        new("HazardDeletedEvent", EventCategory.DomainEvent, EventLifecycleState.Active, "Published and handled in active production chain."),
        new("HazardStatusChangedEvent", EventCategory.DomainEvent, EventLifecycleState.Active, "Published and handled in active production chain."),
        new("HazardUpdatedEvent", EventCategory.DomainEvent, EventLifecycleState.Active, "Published and handled in active production chain."),
        new("HighRiskIdentifiedEvent", EventCategory.DomainEvent, EventLifecycleState.Active, "Published and handled in active production chain."),
        new("InvestigationStatusChangedEvent", EventCategory.DomainEvent, EventLifecycleState.PlannedStub, "Published transition event; downstream handler chain is pending."),
        new("MitigationApprovalApprovedEvent", EventCategory.DomainEvent, EventLifecycleState.PlannedStub, "Reserved for future approval-complete workflow chain."),
        new("MitigationApprovalRequestedEvent", EventCategory.DomainEvent, EventLifecycleState.Active, "Published and handled in active production chain."),
        new("MitigationCompletedEvent", EventCategory.DomainEvent, EventLifecycleState.Active, "Published and handled in active production chain."),
        new("MitigationCreatedEvent", EventCategory.DomainEvent, EventLifecycleState.PlannedStub, "Reserved for future mitigation-created workflow chain."),
        new("MitigationOverdueEvent", EventCategory.DomainEvent, EventLifecycleState.PlannedStub, "Reserved for scheduled overdue chain activation."),
        new("MitigationStatusChangedEvent", EventCategory.DomainEvent, EventLifecycleState.Active, "Published and handled in active production chain."),
        new("ReportClosedEvent", EventCategory.DomainEvent, EventLifecycleState.PlannedStub, "Reserved for future report-close workflow chain."),
        new("ReportCreatedEvent", EventCategory.DomainEvent, EventLifecycleState.Active, "Published and handled in active production chain."),
        new("ReportStageChangedEvent", EventCategory.DomainEvent, EventLifecycleState.PlannedStub, "Published transition event; downstream handler chain is pending."),
        new("ReportStatusChangedEvent", EventCategory.DomainEvent, EventLifecycleState.PlannedStub, "Published transition event; downstream handler chain is pending."),
        new("ReportUpdatedEvent", EventCategory.DomainEvent, EventLifecycleState.PlannedStub, "Reserved for future report-update workflow chain."),
        new("RiskAssessmentCompletedEvent", EventCategory.DomainEvent, EventLifecycleState.Active, "Published and handled in active production chain."),
        new("RiskAssessmentCreatedEvent", EventCategory.DomainEvent, EventLifecycleState.PlannedStub, "Reserved for future risk-assessment-created workflow chain."),
        new("RiskAssessmentStageChangedEvent", EventCategory.DomainEvent, EventLifecycleState.PlannedStub, "Published transition event; downstream handler chain is pending."),
        new("RiskAssessmentStatusChangedEvent", EventCategory.DomainEvent, EventLifecycleState.PlannedStub, "Published transition event; downstream handler chain is pending."),
        new("RiskAssessmentUpdatedEvent", EventCategory.DomainEvent, EventLifecycleState.Active, "Published and handled in active production chain."),
        new("SPIComplianceChangedEvent", EventCategory.DomainEvent, EventLifecycleState.Active, "Published and handled in active production chain."),
        new("SPIThresholdExceededEvent", EventCategory.DomainEvent, EventLifecycleState.Active, "Published and handled in active production chain."),
        new("ValidationDecisionMadeEvent", EventCategory.DomainEvent, EventLifecycleState.Active, "Published and handled in active production chain."),

        new("EmailNotificationEvent", EventCategory.IntegrationEvent, EventLifecycleState.Active, "Primary integration event for email notification delivery."),

        new("SPIDashboardRefreshEvent", EventCategory.UIEvent, EventLifecycleState.Active, "Published and handled in active production chain."),
        new("UINotificationEvent", EventCategory.UIEvent, EventLifecycleState.Active, "UI notification event owned by Presentation handler path.")
    ];

    private static readonly IReadOnlyDictionary<string, EventLifecycleEntry> EventLifecycleEntriesByClassName =
        EventLifecycleEntries.ToDictionary(x => x.EventClassName, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<EventLifecycleEntry> GetAll()
    {
        return EventLifecycleEntries;
    }

    public IReadOnlyList<EventLifecycleEntry> GetByCategory(EventCategory eventCategory)
    {
        ArgumentNullException.ThrowIfNull(eventCategory);

        return EventLifecycleEntries
            .Where(x => x.EventCategory == eventCategory)
            .ToList();
    }

    public bool TryGet(string eventClassName, out EventLifecycleEntry entry)
    {
        if (string.IsNullOrWhiteSpace(eventClassName))
        {
            entry = default!;
            return false;
        }

        return EventLifecycleEntriesByClassName.TryGetValue(eventClassName.Trim(), out entry!);
    }

    public EventLifecycleState ResolveState(string eventClassName, EventLifecycleState fallbackState)
    {
        ArgumentNullException.ThrowIfNull(fallbackState);

        return TryGet(eventClassName, out var entry)
            ? entry.LifecycleState
            : fallbackState;
    }

    public bool IsActive(string eventClassName)
    {
        return TryGet(eventClassName, out var entry)
               && entry.LifecycleState == EventLifecycleState.Active;
    }
}
