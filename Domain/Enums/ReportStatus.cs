//-----------------------------------------------------------------------
// <copyright file="ReportStatus.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enumeration defining valid status values for SMS report workflows.
//                  Domain enumeration defining valid states and classifications
//                  for business entities and processes.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Enums;

public abstract class ReportStatus : BaseEnum<ReportStatus>
{
    protected ReportStatus(string value, string name) : base(value, name)
    {

    }



    #region Validation Decision Types

    
    /// <summary>Report New</summary>
    public static readonly ReportStatus Created = new ReportCreatedStatus();
    public static readonly ReportStatus Updated = new ReportUpdatedStatus();
    public static readonly ReportStatus Closed = new ReportClosedStatus();
    public static readonly ReportStatus InMitigation = new ReportInMitigationStatus();
    public static readonly ReportStatus UnderInvestigation  = new ReportUnderInvestigationStatus();
    public static readonly ReportStatus RiskAssessmentSubmitted  = new ReportRiskAssessmentSubmittedStatus();
    public static readonly ReportStatus RiskAssessmentInProgress  = new ReportRiskAssessmentInProgressStatus();
    public static readonly ReportStatus ValidationRevised  = new ReportValidationRevisedStatus();
    public static readonly ReportStatus ValidationCompleted = new ReportValidationCompletedStatus();
    public static readonly ReportStatus NeedsValidation  = new ReportNeedsValidationStatus();
    public static readonly ReportStatus ReadyForProcessing = new ReportReadyForProcessingStatus();
    public static readonly ReportStatus RiskRegistryOnly = new RiskRegistryOnlyStatus();
    public static readonly ReportStatus Unknown = new ReportUnknownStatus();
    #endregion

    #region Implementations
    private sealed class ReportUnknownStatus : ReportStatus
    {
        public ReportUnknownStatus() : base("REPORT_UNKNOWN", "REPORT_UNKNOWN")
        {
        }
    }
    private sealed class ReportCreatedStatus : ReportStatus
    {
        public ReportCreatedStatus() : base("REPORT_CREATED", "REPORT_CREATED")
        {
        }
    }
    private sealed class ReportUpdatedStatus : ReportStatus
    {
        public ReportUpdatedStatus() : base("REPORT_UPDATED", "REPORT_UPDATED")
        {
        }
    }
    private sealed class ReportClosedStatus : ReportStatus
    {
        public ReportClosedStatus() : base("REPORT_CLOSED", "REPORT_CLOSED")
        {
        }
    }
    private sealed class ReportNeedsValidationStatus : ReportStatus
    {
        public ReportNeedsValidationStatus() : base("REPORT_NEEDS_VALIDATION", "REPORT_NEEDS_VALIDATION")
        {
        }
    }
    private sealed class ReportReadyForProcessingStatus : ReportStatus
    {
        public ReportReadyForProcessingStatus() : base("READY_FOR_PROCESSING", "READY_FOR_PROCESSING")
        {
        }
    }
    private sealed class RiskRegistryOnlyStatus : ReportStatus
    {
        public RiskRegistryOnlyStatus() : base("RISK_REGISTRY_ONLY", "RISK_REGISTRY_ONLY")
        {
        }
    }
    private sealed class ReportValidationCompletedStatus : ReportStatus
    {
        public ReportValidationCompletedStatus() : base("REPORT_VALIDATION_COMPLETED", "REPORT_VALIDATION_COMPLETED")
        {
        }
    }
    private sealed class ReportValidationRevisedStatus : ReportStatus
    {
        public ReportValidationRevisedStatus() : base("REPORT_VALIDATION_REVISED", "REPORT_VALIDATION_REVISED")
        {
        }
    }
    private sealed class ReportRiskAssessmentInProgressStatus : ReportStatus
    {
        public ReportRiskAssessmentInProgressStatus() : base("REPORT_RISKASSESSMENT_IN_PROGRESS", "REPORT_RISKASSESSMENT_IN_PROGRESS")
        {
        }
    }
    private sealed class ReportRiskAssessmentSubmittedStatus : ReportStatus
    {
        public ReportRiskAssessmentSubmittedStatus() : base("REPORT_RISKASSESSMENT_SUBMITTED", "REPORT_RISKASSESSMENT_SUBMITTED")
        {
        }
    }
    private sealed class ReportUnderInvestigationStatus : ReportStatus
    {
        public ReportUnderInvestigationStatus() : base("REPORT_UNDER_INVESTIGATION", "REPORT_UNDER_INVESTIGATION")
        {
        }
    }
    private sealed class ReportInMitigationStatus : ReportStatus
    {
        public ReportInMitigationStatus() : base("REPORT_IN_MITIGATION", "REPORT_IN_MITIGATION")
        {
        }
    }
    #endregion

    /// <summary>
    /// Gets all available validation decision values
    /// </summary>
    public static IEnumerable<ReportStatus> GetAllValues()
    {
        return typeof(ReportStatus)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(ReportStatus))
            .Select(f => (ReportStatus)f.GetValue(null)!)
            .Where(vd => vd != null);
    }






    /// <summary>
    /// Parse a string value to ValidationDecision
    /// </summary>
    public static ReportStatus FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Validation decision value cannot be null or empty", nameof(value));

        return GetAllValues().FirstOrDefault(vd => vd.Value.Equals(value, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"Invalid validation decision value: {value}", nameof(value));
    }

    /// <summary>
    /// Try to parse a string value to ValidationDecision
    /// </summary>
    public static bool TryFromValue(string? value, out ReportStatus? validationDecision)
    {
        validationDecision = null;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        validationDecision = GetAllValues().FirstOrDefault(vd => vd.Value.Equals(value, StringComparison.OrdinalIgnoreCase));
        return validationDecision != null;
    }
}
