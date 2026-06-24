using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS_Domain.Events;

public sealed class RiskAssessmentStageChangedEvent : BaseDomainEvent
{
    public override string EventType => SMS_Domain.Enums.EventType.RiskAssessmentStageChanged.Value;

    public string RiskAssessmentId { get; set; } = string.Empty;
    public string? RiskAssessmentCode { get; set; }
    public RiskAssessmentStage PreviousStage { get; set; } = RiskAssessmentStage.DescribingSystem;
    public RiskAssessmentStage NewStage { get; set; } = RiskAssessmentStage.DescribingSystem;
    public string ChangedBy { get; set; } = "SYSTEM";
    public DateTime ChangedDate { get; set; }

    public RiskAssessmentStageChangedEvent(SMSEventID id) : base(id)
    {
    }

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
