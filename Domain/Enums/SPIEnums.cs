//-----------------------------------------------------------------------
// <copyright file="SPIEnums.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enumeration defining Safety Performance Indicator classifications and measurement types.
//                  Domain enumeration defining valid states and classifications
//                  for business entities and processes.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Enums;

/// <summary>
/// Safety Performance Indicator types based on SMS requirements
/// </summary>
public abstract class SPIType : BaseEnum<SPIType>
{
    protected SPIType(string value, string name, string description, string category) : base(value, name)
    {
        Description = description;
        Category = category;
    }

    public string Description { get; }
    public string Category { get; }

    #region SPI Types

    // Leading Indicators (Proactive)
    public static readonly SPIType HazardReportRate = new HazardReportRateType();
    public static readonly SPIType TrainingCompletionRate = new TrainingCompletionRateType();
    public static readonly SPIType AuditComplianceRate = new AuditComplianceRateType();
    public static readonly SPIType SafetyMeetingAttendance = new SafetyMeetingAttendanceType();
    public static readonly SPIType MitigationImplementationRate = new MitigationImplementationRateType();

    // Lagging Indicators (Reactive)
    public static readonly SPIType IncidentRate = new IncidentRateType();
    public static readonly SPIType AccidentRate = new AccidentRateType();
    public static readonly SPIType NearMissRate = new NearMissRateType();
    public static readonly SPIType RegulatoryViolations = new RegulatoryViolationsType();
    public static readonly SPIType WorkplaceSafetyIncidents = new WorkplaceSafetyIncidentsType();

    // Process Indicators
    public static readonly SPIType CorrectiveActionClosure = new CorrectiveActionClosureType();
    public static readonly SPIType RiskAssessmentCompletion = new RiskAssessmentCompletionType();
    public static readonly SPIType InvestigationTimeliness = new InvestigationTimelinessType();
    public static readonly SPIType SystemEffectiveness = new SystemEffectivenessType();

    // Compliance Indicators
    public static readonly SPIType RegulatoryCompliance = new RegulatoryComplianceType();
    public static readonly SPIType PolicyAdherence = new PolicyAdherenceType();
    public static readonly SPIType DocumentationCurrency = new DocumentationCurrencyType();

    #endregion

    #region Implementations

    // Leading Indicators
    private sealed class HazardReportRateType : SPIType
    {
        public HazardReportRateType() : base("HAZARD_REPORT_RATE", "Hazard Report Rate",
            "Number of hazard reports submitted per period", "Leading")
        { }
    }

    private sealed class TrainingCompletionRateType : SPIType
    {
        public TrainingCompletionRateType() : base("TRAINING_COMPLETION_RATE", "Training Completion Rate",
            "Percentage of required safety training completed on time", "Leading")
        { }
    }

    private sealed class AuditComplianceRateType : SPIType
    {
        public AuditComplianceRateType() : base("AUDIT_COMPLIANCE_RATE", "Audit Compliance Rate",
            "Percentage of audit requirements met during inspections", "Leading")
        { }
    }

    private sealed class SafetyMeetingAttendanceType : SPIType
    {
        public SafetyMeetingAttendanceType() : base("SAFETY_MEETING_ATTENDANCE", "Safety Meeting Attendance",
            "Percentage attendance at safety committee meetings", "Leading")
        { }
    }

    private sealed class MitigationImplementationRateType : SPIType
    {
        public MitigationImplementationRateType() : base("MITIGATION_IMPLEMENTATION_RATE", "Mitigation Implementation Rate",
            "Percentage of planned mitigations implemented on schedule", "Leading")
        { }
    }

    // Lagging Indicators
    private sealed class IncidentRateType : SPIType
    {
        public IncidentRateType() : base("INCIDENT_RATE", "Incident Rate",
            "Number of safety incidents per period", "Lagging")
        { }
    }

    private sealed class AccidentRateType : SPIType
    {
        public AccidentRateType() : base("ACCIDENT_RATE", "Accident Rate",
            "Number of accidents per period", "Lagging")
        { }
    }

    private sealed class NearMissRateType : SPIType
    {
        public NearMissRateType() : base("NEAR_MISS_RATE", "Near Miss Rate",
            "Number of near miss events per period", "Lagging")
        { }
    }

    private sealed class RegulatoryViolationsType : SPIType
    {
        public RegulatoryViolationsType() : base("REGULATORY_VIOLATIONS", "Regulatory Violations",
            "Number of regulatory violations identified", "Lagging")
        { }
    }

    private sealed class WorkplaceSafetyIncidentsType : SPIType
    {
        public WorkplaceSafetyIncidentsType() : base("WORKPLACE_SAFETY_INCIDENTS", "Workplace Safety Incidents",
            "Number of workplace safety incidents", "Lagging")
        { }
    }

    // Process Indicators
    private sealed class CorrectiveActionClosureType : SPIType
    {
        public CorrectiveActionClosureType() : base("CORRECTIVE_ACTION_CLOSURE", "Corrective Action Closure Rate",
            "Percentage of corrective actions closed on time", "Process")
        { }
    }

    private sealed class RiskAssessmentCompletionType : SPIType
    {
        public RiskAssessmentCompletionType() : base("RISK_ASSESSMENT_COMPLETION", "Risk Assessment Completion Rate",
            "Percentage of risk assessments completed within timeline", "Process")
        { }
    }

    private sealed class InvestigationTimelinessType : SPIType
    {
        public InvestigationTimelinessType() : base("INVESTIGATION_TIMELINESS", "Investigation Timeliness",
            "Average days to complete safety investigations", "Process")
        { }
    }

    private sealed class SystemEffectivenessType : SPIType
    {
        public SystemEffectivenessType() : base("SYSTEM_EFFECTIVENESS", "System Effectiveness Score",
            "Overall effectiveness rating of SMS implementation", "Process")
        { }
    }

    // Compliance Indicators
    private sealed class RegulatoryComplianceType : SPIType
    {
        public RegulatoryComplianceType() : base("REGULATORY_COMPLIANCE", "Regulatory Compliance Rate",
            "Percentage compliance with 14 CFR Part 139 requirements", "Compliance")
        { }
    }

    private sealed class PolicyAdherenceType : SPIType
    {
        public PolicyAdherenceType() : base("POLICY_ADHERENCE", "Policy Adherence Rate",
            "Percentage adherence to safety policies and procedures", "Compliance")
        { }
    }

    private sealed class DocumentationCurrencyType : SPIType
    {
        public DocumentationCurrencyType() : base("DOCUMENTATION_CURRENCY", "Documentation Currency Rate",
            "Percentage of safety documentation that is current", "Compliance")
        { }
    }

    #endregion

    public static IEnumerable<SPIType> GetByCategory(string category)
    {
        return GetAllValues().Where(spi => string.Equals(spi.Category, category, StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<SPIType> GetAllValues()
    {
        return typeof(SPIType)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(SPIType))
            .Select(f => (SPIType)f.GetValue(null)!)
            .Where(spi => spi != null);
    }

    public bool IsLeadingIndicator => Category == "Leading";
    public bool IsLaggingIndicator => Category == "Lagging";
    public bool IsProcessIndicator => Category == "Process";
    public bool IsComplianceIndicator => Category == "Compliance";
}

/// <summary>
/// SPI Status enumeration
/// </summary>
public abstract class SPIStatus : BaseEnum<SPIStatus>
{
    protected SPIStatus(string value, string name) : base(value, name) { }

    public static readonly SPIStatus Active = new ActiveStatus();
    public static readonly SPIStatus Inactive = new InactiveStatus();
    public static readonly SPIStatus UnderReview = new UnderReviewStatus();
    public static readonly SPIStatus Deprecated = new DeprecatedStatus();

    private sealed class ActiveStatus : SPIStatus
    {
        public ActiveStatus() : base("ACTIVE", "Active") { }
    }

    private sealed class InactiveStatus : SPIStatus
    {
        public InactiveStatus() : base("INACTIVE", "Inactive") { }
    }

    private sealed class UnderReviewStatus : SPIStatus
    {
        public UnderReviewStatus() : base("UNDER_REVIEW", "Under Review") { }
    }

    private sealed class DeprecatedStatus : SPIStatus
    {
        public DeprecatedStatus() : base("DEPRECATED", "Deprecated") { }
    }
}

/// <summary>
/// SPI Measurement Frequency enumeration
/// </summary>
public abstract class SPIMeasurementFrequency : BaseEnum<SPIMeasurementFrequency>
{
    protected SPIMeasurementFrequency(string value, string name, int daysInterval) : base(value, name)
    {
        DaysInterval = daysInterval;
    }

    public int DaysInterval { get; }

    public static readonly SPIMeasurementFrequency Daily = new DailyFrequency();
    public static readonly SPIMeasurementFrequency Weekly = new WeeklyFrequency();
    public static readonly SPIMeasurementFrequency Monthly = new MonthlyFrequency();
    public static readonly SPIMeasurementFrequency Quarterly = new QuarterlyFrequency();
    public static readonly SPIMeasurementFrequency Annually = new AnnuallyFrequency();

    private sealed class DailyFrequency : SPIMeasurementFrequency
    {
        public DailyFrequency() : base("DAILY", "Daily", 1) { }
    }

    private sealed class WeeklyFrequency : SPIMeasurementFrequency
    {
        public WeeklyFrequency() : base("WEEKLY", "Weekly", 7) { }
    }

    private sealed class MonthlyFrequency : SPIMeasurementFrequency
    {
        public MonthlyFrequency() : base("MONTHLY", "Monthly", 30) { }
    }

    private sealed class QuarterlyFrequency : SPIMeasurementFrequency
    {
        public QuarterlyFrequency() : base("QUARTERLY", "Quarterly", 90) { }
    }

    private sealed class AnnuallyFrequency : SPIMeasurementFrequency
    {
        public AnnuallyFrequency() : base("ANNUALLY", "Annually", 365) { }
    }
}

/// <summary>
/// SPI Trend Direction enumeration
/// </summary>
public abstract class SPITrendDirection : BaseEnum<SPITrendDirection>
{
    protected SPITrendDirection(string value, string name, string color) : base(value, name)
    {
        Color = color;
    }

    public string Color { get; }

    public static readonly SPITrendDirection Improving = new ImprovingTrend();
    public static readonly SPITrendDirection Stable = new StableTrend();
    public static readonly SPITrendDirection Declining = new DecliningTrend();

    private sealed class ImprovingTrend : SPITrendDirection
    {
        public ImprovingTrend() : base("IMPROVING", "Improving", "#28a745") { }
    }

    private sealed class StableTrend : SPITrendDirection
    {
        public StableTrend() : base("STABLE", "Stable", "#6c757d") { }
    }

    private sealed class DecliningTrend : SPITrendDirection
    {
        public DecliningTrend() : base("DECLINING", "Declining", "#dc3545") { }
    }
}
