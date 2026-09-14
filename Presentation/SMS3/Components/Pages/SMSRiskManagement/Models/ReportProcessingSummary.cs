namespace SMS3.Components.Pages.SMSRiskManagement.Models;

public class ReportProcessingSummary
{
    public string ReportId { get; set; } = string.Empty;
    public string ReportDescription { get; set; } = string.Empty;
    public string ReportStatus { get; set; } = string.Empty;

    public string ReportStage { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }

    public string? HazardId { get; set; }
    public string HazardType { get; set; } = string.Empty;
    public string HazardCategory { get; set; } = string.Empty;
    public string HazardDescription { get; set; } = string.Empty;
    public decimal? HazardInitialAverageScore { get; set; }
    public decimal? HazardResidualAverageScore { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string SubmittedBy { get; set; } = string.Empty;
    public DateTime ReportedDate { get; set; }
    public bool IsAnonymous { get; set; }

    // ENHANCED: Risk Assessment Information
    public string? RiskAssessmentId { get; set; }
    public string? RiskAssessmentCreatedBy { get; set; }
    public DateTime? RiskAssessmentCreatedDate { get; set; }
    public int CurrentAssessmentStep { get; set; } = 0;
    public string RiskAssessmentStatus { get; set; } = string.Empty;
    public string AssessmentStage { get; set; } = string.Empty;
    public string AssessmentType { get; set; } = string.Empty; // NEW: Technical vs Preliminary
    public bool HasRiskAssessment => !string.IsNullOrEmpty(RiskAssessmentId);

    // ENHANCED: Report Validation Information
    public string? ReportValidationId { get; set; }
    public string ValidationType { get; set; } = string.Empty; // NEW: Preliminary vs Technical
    public string ValidationDecision { get; set; } = string.Empty;
    public bool HasReportValidation => !string.IsNullOrEmpty(ReportValidationId);

    // NEW: Investigation Information
    public string? InvestigationId { get; set; }
    public string InvestigationStatus { get; set; } = string.Empty;
    public string? AssignedInvestigator { get; set; }
    public string? InvestigationNotes { get; set; }
    public int InterviewCount { get; set; } = 0;
    public DateTime? InvestigationStartDate { get; set; }
    public bool HasInvestigation => !string.IsNullOrEmpty(InvestigationId);
    public int DaysInInvestigation => InvestigationStartDate.HasValue 
        ? (DateTime.UtcNow - InvestigationStartDate.Value).Days 
        : 0;

    // ? NEW: All Mitigations for All Hazards in Report
    public List<MitigationSummary> AllMitigations { get; set; } = new();
    public bool HasMitigations => AllMitigations?.Any() == true;
    public int MitigationCount => AllMitigations?.Count ?? 0;

    public ProcessingStatusCategory StatusCategory { get; set; }
    public int DaysInStage { get; set; }
    public string? AssignedTo { get; set; }
    public string ValidationUrl { get; set; } = string.Empty;

    public string DisplayId => !string.IsNullOrEmpty(HazardId) ? HazardId : ReportId;

    // NEW: Default hazard classification detection
    public bool HasDefaultHazardCategory => HazardCategory == SMS_Domain.Enums.HazardCategory.Default.Value;
    public bool HasDefaultHazardType => HazardType == SMS_Domain.Enums.HazardType.Default.Value; 
    public bool RequiresHazardClassificationUpdate => HasDefaultHazardCategory || HasDefaultHazardType;
    public bool IsRiskRegistryOnly =>
        string.Equals(AssessmentType?.Trim(), SMS_Domain.Enums.RiskAssessmentType.RiskRegistryOnly.Value, StringComparison.OrdinalIgnoreCase)
        || string.Equals(AssessmentType?.Trim(), SMS_Domain.Enums.RiskAssessmentType.RiskRegistryOnly.Name, StringComparison.OrdinalIgnoreCase)
        || string.Equals(ReportStatus?.Trim(), SMS_Domain.Enums.ReportStatus.RiskRegistryOnly, StringComparison.OrdinalIgnoreCase)
        || string.Equals(ReportStatus?.Trim(), SMS_Domain.Enums.ReportStatus.RiskRegistryOnly.Value, StringComparison.OrdinalIgnoreCase);
    public bool IsRiskRegistryOnlyScored =>
        HazardInitialAverageScore.HasValue
        && HazardResidualAverageScore.HasValue
        && HazardInitialAverageScore.Value > 0
        && HazardResidualAverageScore.Value > 0;

    // ENHANCED: Smart validation URL based on validation type and assessment progress
    public string SmartUrl
    {
        get
        {
            // NEW: Handle reports with default hazard classifications first
            if (RequiresHazardClassificationUpdate)
            {
                // Redirect to HazardReporting for hazard editing
                return $"/SMSRiskManagement/HazardReporting/{HazardId}?returnTo=report-processing";
            }

            // VALIDATION TAB: Reports without ReportValidation record
            if (StatusCategory == ProcessingStatusCategory.Validation)
            {
                return $"/SMSRiskManagement/ReportValidation/{ReportId}";
            }

            // RISK ASSESSMENT TAB: Reports with ReportValidation - route based on ValidationType
            if (StatusCategory == ProcessingStatusCategory.RiskAssessment)
            {
                if (IsRiskRegistryOnly)
                {
                    var encodedReportId = Uri.EscapeDataString((ReportId ?? string.Empty).Trim());
                    var encodedHazardId = Uri.EscapeDataString((HazardId ?? string.Empty).Trim());
                    return $"/SMSRiskManagement/TechnicalAssessment/{encodedReportId}/{encodedHazardId}/4?returnTo=report-processing";
                }

                if (ValidationType?.ToLower() == "technical")
                {
                    // Technical Assessment - multi-step, smart navigation
                    if (HasRiskAssessment && CurrentAssessmentStep > 0)
                    {
                        // Continue to next step of existing assessment
                        var nextStep = CurrentAssessmentStep < 5 ? CurrentAssessmentStep : CurrentAssessmentStep;
                        return $"/SMSRiskManagement/TechnicalAssessment/{ReportId}/{HazardId}/{nextStep}?returnTo=report-processing";
                    }
                    else if (!string.IsNullOrEmpty(HazardId))
                    {
                        // Start new technical assessment
                        return $"/SMSRiskManagement/TechnicalAssessment/{ReportId}/{HazardId}/1?returnTo=report-processing";
                    }
                }
            }

            // INVESTIGATION TAB: Navigate to investigation if available
            if (StatusCategory == ProcessingStatusCategory.Investigation)
            {
                if (HasInvestigation && !string.IsNullOrEmpty(HazardId))
                {
                    return $"/SMSRiskManagement/Investigations/{InvestigationId}/{HazardId}";
                }
            }

            // Fallback to report validation
            return $"/SMSRiskManagement/ReportValidation/{ReportId}";
        }
    }

    // ENHANCED: Smart button text based on validation type and progress
    public string ActionButtonText
    {
        get
        {
            // NEW: Handle reports with default hazard classifications first
            if (RequiresHazardClassificationUpdate)
            {
                return "Validate Report";
            }

            return StatusCategory switch
            {
                ProcessingStatusCategory.Validation => "Start Processing",
                ProcessingStatusCategory.RiskAssessment => GetRiskAssessmentButtonText(),
                ProcessingStatusCategory.Investigation => HasInvestigation ? "Continue Investigation" : "Start Investigation",
                ProcessingStatusCategory.Mitigation => "View Mitigation",
                ProcessingStatusCategory.Closed => "View Closed",
                _ => "Process"
            };
        }
    }

    // NEW: Button style for default hazard classification
    public string ActionButtonStyle
    {
        get
        {
            if (RequiresHazardClassificationUpdate)
            {
                return "ButtonStyle.Warning"; // Orange for defaults requiring attention
            }

            return StatusCategory switch
            {
                ProcessingStatusCategory.Validation => "ButtonStyle.Primary",
                ProcessingStatusCategory.RiskAssessment => "ButtonStyle.Success", 
                ProcessingStatusCategory.Investigation => "ButtonStyle.Info",
                ProcessingStatusCategory.Mitigation => "ButtonStyle.Secondary",
                ProcessingStatusCategory.Closed => "ButtonStyle.Light",
                _ => "ButtonStyle.Primary"
            };
        }
    }

    private string GetRiskAssessmentButtonText()
    {
        if (IsRiskRegistryOnly)
        {
            return "Score Risk Only";
        }

        // Determine button text based on ValidationType
        if (ValidationType?.ToLower() == "preliminary")
        {
            return "Start Preliminary Assessment";
        }
        else if (ValidationType?.ToLower() == "technical")
        {
            if (HasRiskAssessment && CurrentAssessmentStep > 0)
            {
                return $"Continue Step {CurrentAssessmentStep}";
            }
            else
            {
                return "Start Technical Assessment";
            }
        }
        else
        {
            return "Start Risk Assessment";
        }
    }
}


