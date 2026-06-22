namespace SMS_Domain.Enums;

public abstract class EventType : BaseEnum<EventType>
{
    protected EventType(string value, string name) : base(value, name) { }

    #region Domain Events
    public static readonly EventType ReportCreated = new ReportCreatedEventType();
    public static readonly EventType ReportUpdated = new ReportUpdatedEventType();
    public static readonly EventType ReportClosed = new ReportClosedEventType();
    public static readonly EventType ReportStatusChanged = new ReportStatusChangedEventType();
    public static readonly EventType ReportStageChanged = new ReportStageChangedEventType();

    public static readonly EventType InvestigationCreated = new InvestigationCreatedEventType();
    public static readonly EventType InvestigationUpdated = new InvestigationUpdatedEventType();
    public static readonly EventType InvestigationStatusChanged = new InvestigationStatusChangedEventType();

    public static readonly EventType HazardCreated = new HazardCreatedEventType();
    public static readonly EventType HazardUpdated = new HazardUpdatedEventType();
    public static readonly EventType HazardDeleted = new HazardDeletedEventType();
    public static readonly EventType HazardStatusChanged = new HazardStatusChangedEventType();
    public static readonly EventType HighRiskIdentified = new HighRiskIdentifiedEventType();

    public static readonly EventType RiskAssessmentCreated = new RiskAssessmentCreatedEventType();
    public static readonly EventType RiskAssessmentUpdated = new RiskAssessmentUpdatedEventType();
    public static readonly EventType RiskAssessmentStatusChanged = new RiskAssessmentStatusChangedEventType();
    public static readonly EventType RiskAssessmentStageChanged = new RiskAssessmentStageChangedEventType();
    public static readonly EventType RiskAssessmentCompleted = new RiskAssessmentCompletedEventType();

    public static readonly EventType MitigationCreated = new MitigationCreatedEventType();
    public static readonly EventType MitigationApprovalRequested = new MitigationApprovalRequestedEventType();
    public static readonly EventType MitigationApprovalApproved = new MitigationApprovalApprovedEventType();
    public static readonly EventType MitigationStatusChanged = new MitigationStatusChangedEventType();
    public static readonly EventType MitigationOverdue = new MitigationOverdueEventType();
    public static readonly EventType ValidationDecisionMade = new ValidationDecisionMadeEventType();
    public static readonly EventType SPIComplianceChanged = new SPIComplianceChangedEventType();
    #endregion

    #region Integration Events
    public static readonly EventType EmailNotification = new EmailNotificationEventType();
    #endregion
    #region UI Events
    public static readonly EventType UINotification = new UINotificationEventType();
    #endregion
    private sealed class UINotificationEventType : EventType
    {
        public UINotificationEventType() : base("UI_NOTIFICATION", "UI_NOTIFICATION") { }
    }
    private sealed class EmailNotificationEventType : EventType
    {
        public EmailNotificationEventType() : base("EMAIL_NOTIFICATION", "EMAIL_NOTIFICATION") { }
    }
    private sealed class MitigationCreatedEventType : EventType
    {
        public MitigationCreatedEventType() : base("MITIGATION_CREATED", "MITIGATION_CREATED") { }
    }
    private sealed class MitigationApprovalRequestedEventType : EventType
    {
        public MitigationApprovalRequestedEventType() : base("MITIGATION_APPROVAL_REQUESTED", "MITIGATION_APPROVAL_REQUESTED") { }
    }
    private sealed class MitigationApprovalApprovedEventType : EventType
    {
        public MitigationApprovalApprovedEventType() : base("MITIGATION_APPROVAL_APPROVED", "MITIGATION_APPROVAL_APPROVED") { }
    }
    private sealed class MitigationStatusChangedEventType : EventType
    {
        public MitigationStatusChangedEventType() : base("MITIGATION_STATUS_CHANGED", "MITIGATION_STATUS_CHANGED") { }
    }









    private sealed class ReportCreatedEventType : EventType
    {
        public ReportCreatedEventType() : base("REPORT_CREATED", "REPORT_CREATED") { }
    }
    private sealed class ReportUpdatedEventType : EventType
    {
        public ReportUpdatedEventType() : base("REPORT_UPDATED", "REPORT_UPDATED") { }
    }
    private sealed class ReportClosedEventType : EventType
    {
        public ReportClosedEventType() : base("REPORT_CLOSED", "REPORT_CLOSED") { }
    }
    private sealed class ReportStatusChangedEventType : EventType
    {
        public ReportStatusChangedEventType() : base("REPORT_STATUS_CHANGED", "REPORT_STATUS_CHANGED") { }
    }
    private sealed class ReportStageChangedEventType : EventType
    {
        public ReportStageChangedEventType() : base("REPORT_STAGE_CHANGED", "REPORT_STAGE_CHANGED") { }
    }

    private sealed class InvestigationCreatedEventType : EventType
    {
        public InvestigationCreatedEventType() : base("INVESTIGATION_CREATED", "INVESTIGATION_CREATED") { }
    }
    private sealed class InvestigationUpdatedEventType : EventType
    {
        public InvestigationUpdatedEventType() : base("INVESTIGATION_UPDATED", "INVESTIGATION_UPDATED") { }
    }
    private sealed class InvestigationStatusChangedEventType : EventType
    {
        public InvestigationStatusChangedEventType() : base("INVESTIGATION_STATUS_CHANGED", "INVESTIGATION_STATUS_CHANGED") { }
    }

    private sealed class HazardCreatedEventType : EventType
    {
        public HazardCreatedEventType() : base("HAZARD_CREATED", "HAZARD_CREATED") { }
    }
    private sealed class HazardUpdatedEventType : EventType
    {
        public HazardUpdatedEventType() : base("HAZARD_UPDATED", "HAZARD_UPDATED") { }
    }
    private sealed class HazardDeletedEventType : EventType
    {
        public HazardDeletedEventType() : base("HAZARD_DELETED", "HAZARD_DELETED") { }
    }
    private sealed class HazardStatusChangedEventType : EventType
    {
        public HazardStatusChangedEventType() : base("HAZARD_STATUS_CHANGED", "HAZARD_STATUS_CHANGED") { }
    }
    private sealed class HighRiskIdentifiedEventType : EventType
    {
        public HighRiskIdentifiedEventType() : base("HIGH_RISK_IDENTIFIED", "HIGH_RISK_IDENTIFIED") { }
    }

    private sealed class RiskAssessmentCreatedEventType : EventType
    {
        public RiskAssessmentCreatedEventType() : base("RISKASSESSMENT_CREATED", "RISKASSESSMENT_CREATED") { }
    }
    private sealed class RiskAssessmentUpdatedEventType : EventType
    {
        public RiskAssessmentUpdatedEventType() : base("RISKASSESSMENT_UPDATED", "RISKASSESSMENT_UPDATED") { }
    }
    private sealed class RiskAssessmentStatusChangedEventType : EventType
    {
        public RiskAssessmentStatusChangedEventType() : base("RISKASSESSMENT_STATUS_CHANGED", "RISKASSESSMENT_STATUS_CHANGED") { }
    }
    private sealed class RiskAssessmentStageChangedEventType : EventType
    {
        public RiskAssessmentStageChangedEventType() : base("RISKASSESSMENT_STAGE_CHANGED", "RISKASSESSMENT_STAGE_CHANGED") { }
    }
    private sealed class RiskAssessmentCompletedEventType : EventType
    {
        public RiskAssessmentCompletedEventType() : base("RISKASSESSMENT_COMPLETED", "RISKASSESSMENT_COMPLETED") { }
    }
    private sealed class MitigationOverdueEventType : EventType
    {
        public MitigationOverdueEventType() : base("MITIGATION_OVERDUE", "MITIGATION_OVERDUE") { }
    }
    private sealed class ValidationDecisionMadeEventType : EventType
    {
        public ValidationDecisionMadeEventType() : base("VALIDATION_DECISION_MADE", "VALIDATION_DECISION_MADE") { }
    }
    private sealed class SPIComplianceChangedEventType : EventType
    {
        public SPIComplianceChangedEventType() : base("SPI_COMPLIANCE_CHANGED", "SPI_COMPLIANCE_CHANGED") { }
    }
}
