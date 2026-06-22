using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS_Domain.Events;

public sealed class RiskAssessmentStageChangedEvent : BaseDomainEvent
{
    public override string EventType => SMS_Domain.Enums.EventType.RiskAssessmentStageChanged.Value;

    public string RiskAssessmentId { get; }
    public string? RiskAssessmentCode { get; }
    public RiskAssessmentStage PreviousStage { get; }
    public RiskAssessmentStage NewStage { get; }
    public string ChangedBy { get; }
    public DateTime ChangedDate { get; }

    public RiskAssessmentStageChangedEvent(
        SMSEventID id,
        string riskAssessmentId,
        string? riskAssessmentCode,
        RiskAssessmentStage? previousStage,
        RiskAssessmentStage? newStage,
        string changedBy,
        DateTime changedDate) : base(id)
    {
        RiskAssessmentId = riskAssessmentId ?? throw new ArgumentNullException(nameof(riskAssessmentId));
        RiskAssessmentCode = riskAssessmentCode;
        PreviousStage = previousStage ?? RiskAssessmentStage.DescribingSystem;
        NewStage = newStage ?? RiskAssessmentStage.DescribingSystem;
        ChangedBy = string.IsNullOrWhiteSpace(changedBy) ? "SYSTEM" : changedBy;
        ChangedDate = changedDate;
    }
}
