using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS_Domain.Events;

public sealed class RiskAssessmentStatusChangedEvent : BaseDomainEvent
{
    public override string EventType => SMS_Domain.Enums.EventType.RiskAssessmentStatusChanged.Value;

    public string RiskAssessmentId { get; }
    public string? RiskAssessmentCode { get; }
    public RiskAssessmentStatus PreviousStatus { get; }
    public RiskAssessmentStatus NewStatus { get; }
    public string ChangedBy { get; }
    public DateTime ChangedDate { get; }

    public RiskAssessmentStatusChangedEvent(
        SMSEventID id,
        string riskAssessmentId,
        string? riskAssessmentCode,
        RiskAssessmentStatus? previousStatus,
        RiskAssessmentStatus? newStatus,
        string changedBy,
        DateTime changedDate) : base(id)
    {
        RiskAssessmentId = riskAssessmentId ?? throw new ArgumentNullException(nameof(riskAssessmentId));
        RiskAssessmentCode = riskAssessmentCode;
        PreviousStatus = previousStatus ?? RiskAssessmentStatus.AssessmentCreate;
        NewStatus = newStatus ?? RiskAssessmentStatus.AssessmentCreate;
        ChangedBy = string.IsNullOrWhiteSpace(changedBy) ? "SYSTEM" : changedBy;
        ChangedDate = changedDate;
    }
}
