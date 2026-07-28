using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS_Domain.Events;

public sealed class ReportStatusChangedEvent : BaseDomainEvent
{
    public override string EventType => SMS_Domain.Enums.EventType.ReportStatusChanged.Value;

    public string? ReportCode { get; }
    public ReportStatus PreviousStatus { get; }
    public ReportStatus NewStatus { get; }
    public string ChangedBy { get; }
    public DateTime ChangedDate { get; }

    public ReportStatusChangedEvent(
        SMSEventID id,
        string reportId,
        string? reportCode,
        ReportStatus? previousStatus,
        ReportStatus? newStatus,
        string changedBy,
        DateTime changedDate) : base(id)
    {
        ReportId = reportId ?? throw new ArgumentNullException(nameof(reportId));
        ReportCode = reportCode;
        PreviousStatus = previousStatus ?? ReportStatus.Unknown;
        NewStatus = newStatus ?? ReportStatus.Unknown;
        ChangedBy = string.IsNullOrWhiteSpace(changedBy) ? string.Empty : changedBy;
        ChangedDate = changedDate;
    }
}
