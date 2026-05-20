using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums;

public abstract class EventCategogy : BaseEnum<EventCategogy>
{
    protected EventCategogy(string value, string name) : base(value, name)
    {
       
    }

    

    // Leading Indicators (Proactive)
    public static readonly EventCategogy DomainEvent = new DomainEventCategogy();
    public static readonly EventCategogy IntegrationEvent = new IntegrationEventCategogy();
    public static readonly EventCategogy UIEvent = new UIEventCategogy();
    
    

   

    // Leading Indicators
    private sealed class DomainEventCategogy : EventCategogy
    {
        public DomainEventCategogy() : base("DOMAIN_EVENT", "DOMAIN_EVENT")
        { }
    }

    private sealed class IntegrationEventCategogy : EventCategogy
    {
        public IntegrationEventCategogy() : base("INTEGRATION_EVENT", "INTEGRATION_EVENT")
        { }
    }

    private sealed class UIEventCategogy : EventCategogy
    {
        public UIEventCategogy() : base("UI_EVENT", "UI_EVENT")
        { }
    }

   
}
public abstract class EventType : BaseEnum<EventType>
{
    protected EventType(string value, string name) : base(value, name) { }

    #region Domain Events
    public static readonly EventType ReportCreated = new ReportCreatedEventType();
    public static readonly EventType ReportUpdated = new ReportUpdatedEventType();
    public static readonly EventType ReportClosed = new ReportClosedEventType();

    public static readonly EventType HazardCreated = new HazardCreatedEventType();
    public static readonly EventType HazardUpdated = new HazardUpdatedEventType();
    public static readonly EventType HazardDeleted = new HazardDeletedEventType();

    public static readonly EventType RiskAssessmentCreated = new RiskAssessmentCreatedEventType();
    public static readonly EventType RiskAssessmentUpdated = new RiskAssessmentUpdatedEventType();

    public static readonly EventType MitigationCreated = new MitigationCreatedEventType();
    public static readonly EventType MitigationApprovalRequested = new MitigationApprovalRequestedEventType();
    public static readonly EventType MitigationApprovalApproved = new MitigationApprovalApprovedEventType();
    public static readonly EventType MitigationStatusChanged = new MitigationStatusChangedEventType();
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

    private sealed class RiskAssessmentCreatedEventType : EventType
    {
        public RiskAssessmentCreatedEventType() : base("RISKASSESSMENT_CREATED", "RISKASSESSMENT_CREATED") { }
    }
    private sealed class RiskAssessmentUpdatedEventType : EventType
    {
        public RiskAssessmentUpdatedEventType() : base("RISKASSESSMENT_UPDATED", "RISKASSESSMENT_UPDATED") { }
    }
}
