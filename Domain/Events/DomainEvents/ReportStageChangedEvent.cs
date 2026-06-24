using SMS_Domain.Entities;

namespace SMS_Domain.Events;

public sealed class ReportStageChangedEvent : BaseDomainEvent
{
    public override string EventType => SMS_Domain.Enums.EventType.ReportStageChanged.Value;

    public string? ReportCode { get; }
    public string PreviousStage { get; }
    public string NewStage { get; }
    public string ChangedBy { get; }
    public DateTime ChangedDate { get; }

    public ReportStageChangedEvent(
        SMSEventID id,
        string reportId,
        string? reportCode,
        string? previousStage,
        string? newStage,
        string changedBy,
        DateTime changedDate) : base(id)
    {
        ReportId = reportId ?? throw new ArgumentNullException(nameof(reportId));
        ReportCode = reportCode;
        PreviousStage = previousStage ?? string.Empty;
        NewStage = newStage ?? string.Empty;
        ChangedBy = string.IsNullOrWhiteSpace(changedBy) ? "SYSTEM" : changedBy;
        ChangedDate = changedDate;
    }
}
