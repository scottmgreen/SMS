using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS_Domain.Events;

public sealed class InvestigationStatusChangedEvent : BaseDomainEvent
{
    public override string EventType => SMS_Domain.Enums.EventType.InvestigationStatusChanged.Value;

    public string InvestigationId { get; }
    public string? InvestigationCode { get; }
    public InvestigationStatus PreviousStatus { get; }
    public InvestigationStatus NewStatus { get; }
    public string ChangedBy { get; }
    public DateTime ChangedDate { get; }

    public InvestigationStatusChangedEvent(
        SMSEventID id,
        string investigationId,
        string? investigationCode,
        InvestigationStatus? previousStatus,
        InvestigationStatus? newStatus,
        string changedBy,
        DateTime changedDate) : base(id)
    {
        InvestigationId = investigationId ?? throw new ArgumentNullException(nameof(investigationId));
        InvestigationCode = investigationCode;
        PreviousStatus = previousStatus ?? InvestigationStatus.StatusUnknown;
        NewStatus = newStatus ?? InvestigationStatus.StatusUnknown;
        ChangedBy = string.IsNullOrWhiteSpace(changedBy) ? string.Empty : changedBy;
        ChangedDate = changedDate;
    }
}
