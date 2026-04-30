using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;
using SMS_Application.Services;

using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Domain.Events.UIEvents;
using SMS_Domain.Interfaces;

using SMS_Shared.Configuration;

using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration.Extensions;

namespace SMS3.Components.Pages.SMSRiskManagement;

#region Models

public enum ProcessingStatusCategory
{
    Validation,
    RiskAssessment,
    Investigation,
    Mitigation,
    Closed
}

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
    public string Location { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string SubmittedBy { get; set; } = string.Empty;
    public DateTime ReportedDate { get; set; }
    public bool IsAnonymous { get; set; }

    // ENHANCED: Risk Assessment Information
    public string? RiskAssessmentId { get; set; }
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
    public bool HasInvestigation => !string.IsNullOrEmpty(InvestigationId);

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

    // ENHANCED: Smart validation URL based on validation type and assessment progress
    public string SmartUrl
    {
        get
        {
            // NEW: Handle reports with default hazard classifications first
            if (RequiresHazardClassificationUpdate)
            {
                // Redirect to HazardReporting for hazard editing
                return $"/SMSRiskManagement/HazardReporting/{HazardId}";
            }

            // VALIDATION TAB: Reports without ReportValidation record
            if (StatusCategory == ProcessingStatusCategory.Validation)
            {
                return $"/SMSRiskManagement/ReportValidation/{ReportId}";
            }

            // RISK ASSESSMENT TAB: Reports with ReportValidation - route based on ValidationType
            if (StatusCategory == ProcessingStatusCategory.RiskAssessment)
            {
                if (ValidationType?.ToLower() == "technical")
                {
                    // Technical Assessment - multi-step, smart navigation
                    if (HasRiskAssessment && CurrentAssessmentStep > 0)
                    {
                        // Continue to next step of existing assessment
                        var nextStep = CurrentAssessmentStep < 5 ? CurrentAssessmentStep : CurrentAssessmentStep;
                        return $"/SMSRiskManagement/TechnicalAssessment/{ReportId}/{HazardId}/{nextStep}";
                    }
                    else if (!string.IsNullOrEmpty(HazardId))
                    {
                        // Start new technical assessment
                        return $"/SMSRiskManagement/TechnicalAssessment/{ReportId}/{HazardId}/1";
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
        // Determine button text based on ValidationType
        if (ValidationType?.ToLower() == "preliminary")
        {
            return "Start Preliminary Assessment";
        }
        else if (ValidationType?.ToLower() == "technical")
        {
            if (HasRiskAssessment && CurrentAssessmentStep > 0)
            {
                return CurrentAssessmentStep < 5 ? $"Continue Step {CurrentAssessmentStep}" : "Review Assessment";
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

/// <summary>
/// ? NEW: Summary information for a mitigation within a report context
/// </summary>
public class MitigationSummary
{
    public string MitigationCode { get; set; } = string.Empty;
    public string MitigationName { get; set; } = string.Empty;
    public string HazardCode { get; set; } = string.Empty;
    public RiskLevel HazardRiskLevel { get; set; } = default!;
    public string HazardDescription { get; set; } = string.Empty;
    public MitigationStatus Status { get; set; } 
    public string AssignedTo { get; set; } = string.Empty;

    public string ApprovedBy { get; set; } = string.Empty;

    public string AssignedDepartment { get; set; } = string.Empty;
    public DateTime? TargetDate { get; set; }
    
    public bool IsOverdue => TargetDate.HasValue && TargetDate.Value < DateTime.UtcNow && Status != MitigationStatus.Complete;
}

#endregion

/// <summary>
/// Code-behind for ReportProcessing page
/// Manages the comprehensive report processing workflow for all SMS reports
/// </summary>
public partial class ReportProcessing : ComponentBase
{
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<ReportProcessing> _logger { get; set; } = default!;
    [Inject] private IBaseEventBus _eventBus { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    [Inject] private SPIEventCoordinator _spiCoordinator { get; set; } = default!;



    [Inject] private ICurrentUserService CurrentUserService { get; set; } = default!;

    // Data Properties
    private List<ReportProcessingSummary> PendingValidation { get; set; } = new();
    private List<ReportProcessingSummary> PendingRiskAssessment { get; set; } = new();
    private List<ReportProcessingSummary> PendingInvestigation { get; set; } = new();
    private List<ReportProcessingSummary> PendingMitigation { get; set; } = new();
    private List<ReportProcessingSummary> ClosedReferred { get; set; } = new();

    private int selectedTabIndex = 0;
    private bool IsLoading { get; set; } = true;
    private List<SMSOrganizationalUser> AvailableApprovers { get; set; } = new();
    private string? SelectedApprover { get; set; }

    private bool ShowBulkApprovalDialog { get; set; } = false;
    private ReportProcessingSummary? SelectedReportForApproval { get; set; }
    private bool IsProcessingApproval { get; set; } = false;

    // NEW: Description modal dialog properties
    private bool ShowDescriptionModal { get; set; } = false;
    private string SelectedDescription { get; set; } = string.Empty;
    private string SelectedReportId { get; set; } = string.Empty;


    private string BasicTextStyle = "font-size:smaller;font-weight: 600";
    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    #region Data Loading
    /// <summary>
    /// ?? Load organizational users who can approve mitigations
    /// </summary>
    private async Task LoadAvailableApprovers()
    {
        try
        {
            var usersQuery = new GetAllSMSOrganizationalUsersQuery(); // You may need to adjust this query name
            var usersResult = await _mediator.SendAsync(usersQuery, CancellationToken.None);

            if (usersResult.IsSuccess && usersResult.Value is not null)
            {
                AvailableApprovers = usersResult.Value
                .Where(u => u.IsActive &&
                           (u.AuthorityLevel.HasValue || // Has integer authority level
                            u.OrganizationLevel is not null || // Has organization level enum
                            !string.IsNullOrEmpty(u.RiskApprovalAuthority))) // Has risk approval authority string
                .ToList();

                _logger.LogInformation("Loaded {Count} available approvers", AvailableApprovers.Count);
            }
            else
            {
                _logger.LogWarning("Failed to load approvers: {Error}", usersResult.Error?.Message);
                AvailableApprovers = new List<SMSOrganizationalUser>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading available approvers");
            AvailableApprovers = new List<SMSOrganizationalUser>();
        }
    }
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            await LoadAvailableApprovers();


            // Load core entities using CQRS - ENHANCED to include Investigations and Interviews
            var (reports, hazards, riskAssessments, reportValidations, investigations, interviews) 
                = await LoadCoreEntitiesAsync();

            if (!reports.Any())
            {
                _logger.LogWarning("? No reports found - initializing empty lists");
                InitializeEmptyLists();
                return;
            }

            // Create report summaries and categorize - ENHANCED with investigations and interviews
            var reportSummaries = await CreateReportSummariesAsync(reports, hazards, riskAssessments, reportValidations, investigations, interviews);

            CategorizeReports(reportSummaries);
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error loading report processing data");
            InitializeEmptyLists();
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task<(List<Report> reports, List<Hazard> hazards, List<RiskAssessment> riskAssessments, List<SMS_Domain.Entities.ReportValidation> reportValidations, List<Investigation> investigations, List<Interview> interviews)> LoadCoreEntitiesAsync()
    {
        var reports = new List<Report>();
        var hazards = new List<Hazard>();
        var riskAssessments = new List<RiskAssessment>();
        var reportValidations = new List<SMS_Domain.Entities.ReportValidation>();
        var investigations = new List<Investigation>();
        var interviews = new List<Interview>();

        try
        {
            
            // Get all reports
            var reportsQuery = new GetAllReportsQuery();
            var reportsResult = await _mediator.SendAsync(reportsQuery, CancellationToken.None);
            if (reportsResult.IsSuccess)
            {
                reports = reportsResult.Value ?? new List<Report>();
                //_logger.LogWarning("?? DIAGNOSTIC: Successfully loaded {Count} reports from database", reports.Count);

                // Log details of first few reports for debugging
                //foreach (var report in reports.Take(3))
                //{
                //    _logger.LogWarning("?? Report: {Code} | Status: {Status} | Stage: {Stage} | SubmittedBy: {SubmittedBy}", 
                //        report.Code, report.Status, report.Stage, report.SubmittedBy);
                //}


            }
            else
            {
                _logger.LogError("? DIAGNOSTIC: Failed to retrieve reports: {Error}", reportsResult.Error?.Message);
            }

            
            // Get all hazards
            var hazardsQuery = new GetAllHazardsQuery();
            var hazardsResult = await _mediator.SendAsync(hazardsQuery, CancellationToken.None);
            if (hazardsResult.IsSuccess)
            {
                hazards = hazardsResult.Value ?? new List<Hazard>();
                //_logger.LogWarning("? Successfully loaded {Count} hazards from database", hazards.Count);
            }
            else
            {
                _logger.LogError("? Failed to retrieve hazards: {Error}", hazardsResult.Error?.Message);
            }

            

            // CRITICAL: Get all report validations to determine which reports have been validated
            var reportValidationsQuery = new GetAllReportValidationsQuery();
            var reportValidationsResult = await _mediator.SendAsync(reportValidationsQuery, CancellationToken.None);
            if (reportValidationsResult.IsSuccess && reportValidationsResult.Value is not null)
            {
                reportValidations = reportValidationsResult.Value.ToList();
                //_logger.LogWarning("? Successfully loaded {Count} report validations", reportValidations.Count);

                // Log which reports have been validated for debugging
                //foreach (var validation in reportValidations.Take(3))
                //{
                //    _logger.LogWarning("?? Validation: Report {ReportCode} | Decision: {Decision} | Type: {Type}",validation.ReportCode, validation.ValidationDecision, validation.ValidationType);
                //}
            }
            else
            {
                _logger.LogWarning("?? No report validations found or failed to retrieve: {Error}", reportValidationsResult.Error?.Message);
            }

            // Get all risk assessments
            var riskAssessmentsQuery = new GetAllRiskAssessmentsQuery();
            var riskAssessmentsResult = await _mediator.SendAsync(riskAssessmentsQuery, CancellationToken.None);
            if (riskAssessmentsResult.IsSuccess)
            {
                riskAssessments = riskAssessmentsResult.Value ?? new List<RiskAssessment>();
                //_logger.LogInformation("Loaded {Count} risk assessments", riskAssessments.Count);
            }
            else
            {
                _logger.LogError("Failed to retrieve risk assessments: {Error}", riskAssessmentsResult.Error?.Message);
            }

            // NEW: Get all investigations
            var investigationsQuery = new GetAllInvestigationsQuery();
            var investigationsResult = await _mediator.SendAsync(investigationsQuery, CancellationToken.None);
            if (investigationsResult.IsSuccess && investigationsResult.Value is not null)
            {
                investigations = investigationsResult.Value.ToList();
                //_logger.LogInformation("Loaded {Count} investigations", investigations.Count);
            }
            else
            {
                _logger.LogError("Failed to retrieve investigations: {Error}", investigationsResult.Error?.Message);
            }

            // NEW: Get all interviews
            var interviewsQuery = new GetAllInterviewsQuery();
            var interviewsResult = await _mediator.SendAsync(interviewsQuery, CancellationToken.None);
            if (interviewsResult.IsSuccess && interviewsResult.Value is not null)
            {
                interviews = interviewsResult.Value.ToList();
                //_logger.LogInformation("Loaded {Count} interviews", interviews.Count);
            }
            else
            {
                _logger.LogError("Failed to retrieve interviews: {Error}", interviewsResult.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Exception in LoadCoreEntitiesAsync");
        }

       
        return (reports, hazards, riskAssessments, reportValidations, investigations, interviews);
    }

    #endregion

    #region Report Processing

    
    private async Task<List<ReportProcessingSummary>> CreateReportSummariesAsync(List<Report> reports, List<Hazard> hazards, List<RiskAssessment> riskAssessments, List<SMS_Domain.Entities.ReportValidation> reportValidations, List<Investigation> investigations, List<Interview> interviews)
    {
        var summaries = new List<ReportProcessingSummary>();

        
        foreach (var report in reports)
        {
            try
            {
                // Find ALL hazards for this report
                var reportHazards = hazards.Where(h => h.ReportCode?.Trim() == report.Code?.Trim()).ToList();

                if (!reportHazards.Any())
                {
                    _logger.LogWarning("No hazards found for report {ReportCode}, skipping", report.Code);
                    continue;
                }

                // ? FIXED: Get unique risk assessments for this report (not one per hazard)
                var uniqueRiskAssessments = riskAssessments
                    .Where(ra => reportHazards.Any(h => h.Code?.Trim() == ra.HazardCode?.Trim()))
                    .GroupBy(ra => ra.Code) // Group by assessment code to get unique assessments
                    .Select(g => g.First()) // Take first from each group
                    .ToList();

                // CRITICAL: Find matching report validation (same for all assessments in this report)
                var reportValidation = reportValidations.FirstOrDefault(rv => rv.ReportCode?.Trim() == report.Code?.Trim());

                if (uniqueRiskAssessments.Any())
                {
                    // ? FIXED: Create ONE summary per unique RiskAssessment (not per hazard)
                    foreach (var riskAssessment in uniqueRiskAssessments)
                    {
                        // Find the primary hazard for this assessment
                        var primaryHazard = reportHazards.FirstOrDefault(h => h.Code?.Trim() == riskAssessment.HazardCode?.Trim())
                                           ?? reportHazards.First(); // Fallback to first hazard

                        // Find matching investigation for the primary hazard
                        var investigation = investigations.FirstOrDefault(inv => inv.HazardCode?.Trim() == primaryHazard.Code?.Trim());

                        // Find matching interviews for this investigation
                        var investigationInterviews = investigation is not null ?
                            interviews.Where(iv => iv.InvestigationCode?.Trim() == investigation.Code?.Trim()).ToList() :
                            new List<Interview>();

                        // ? Load mitigations for ALL hazards in this report (since one assessment covers all)
                        var allReportMitigations = new List<MitigationSummary>();
                        var processedMitigationCodes = new HashSet<string>(); // ? Track processed mitigations
                        try
                        {
                            foreach (var hazard in reportHazards)
                            {
                                var mitigationQuery = new GetMitigationsByHazardCodeQuery(hazard.Code);
                                var mitigationResult = await _mediator.SendAsync(mitigationQuery, CancellationToken.None);

                                if (mitigationResult.IsSuccess && mitigationResult.Value?.Any() == true)
                                {
                                    var hazardMitigations = mitigationResult.Value
                                        .Where(m => m.Status != MitigationStatus.Approved && !string.IsNullOrEmpty(m.Code) && !processedMitigationCodes.Contains(m.Code))
                                        .Select(m => new MitigationSummary
                                        {
                                            MitigationCode = m.Code ?? "Unknown",
                                            MitigationName = m.Name ?? "Unnamed Mitigation",
                                            HazardCode = hazard.Code,
                                            HazardDescription = hazard.Description ?? "No description",
                                            HazardRiskLevel = hazard.HazardRiskLevel,
                                            Status = m.Status,
                                            AssignedTo = m.AssignedTo ?? "Not Assigned",
                                            AssignedDepartment = m.AssignedDepartment ?? "Not Assigned",
                                            TargetDate = m.TargetDate,

                                        }).ToList();

                                    // ? Track mitigation codes to prevent duplicates
                                    foreach (var mitigation in hazardMitigations)
                                    {
                                        processedMitigationCodes.Add(mitigation.MitigationCode);
                                    }

                                    allReportMitigations.AddRange(hazardMitigations);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error loading mitigations for report {ReportCode}", report.Code);
                        }

                        // ? Create ONE summary for this RiskAssessment covering all hazards
                        var summary = new ReportProcessingSummary
                        {
                            ReportId = report.Code ?? "Unknown",
                            ReportDescription = report.Description ?? "No description",
                            ReportStatus = report.Status ?? ReportStatus.Created,
                            ReportStage = report.Stage ?? "NEW",
                            CreatedBy = report.CreatedBy ?? "Unknown",
                            CreatedDate = report.CreatedDate ?? DateTime.UtcNow,

                            // Use PRIMARY hazard info for display
                            HazardId = primaryHazard.Code,
                            HazardType = primaryHazard.HazardType ?? "Unknown",
                            HazardCategory = primaryHazard.HazardCategory ?? "Unknown", // NEW: Added for default detection
                            HazardDescription = primaryHazard.Description ?? "No description",
                            Location = primaryHazard.HazardLocation?.Description ?? primaryHazard.LocationArea ?? "Not specified",
                            SubmittedBy = report.SubmittedBy ?? report.CreatedBy ?? report.UpdatedBy ?? "Unknown",
                            //ReportedDate = primaryHazard.SubmittedDate,
                            //IsAnonymous = primaryHazard.IsAnonymous,

                            // Risk Assessment Information
                            RiskAssessmentId = riskAssessment.Code,
                            CurrentAssessmentStep = riskAssessment.CurrentStep,
                            RiskAssessmentStatus = riskAssessment.Status?.ToString() ?? "",
                            AssessmentStage = riskAssessment.Stage ?? "",
                            AssessmentType = riskAssessment.RiskAssessmentCategory?.ToString() ?? "",

                            // Report Validation Information
                            ReportValidationId = reportValidation?.Code,
                            ValidationType = reportValidation?.ValidationType ?? "",
                            ValidationDecision = reportValidation?.ValidationDecision ?? "",

                            // Investigation Information
                            InvestigationId = investigation?.Code,
                            InvestigationStatus = investigation?.Status?.ToString() ?? "",
                            AssignedInvestigator = investigation?.AssignedInvestigatorId,
                            InvestigationNotes = investigation?.InvestigationNotes,
                            InterviewCount = investigationInterviews.Count,

                            // ? ALL mitigations from ALL hazards in this report
                            AllMitigations = allReportMitigations,

                            // Status determination
                            StatusCategory = DetermineStatusCategory(report, primaryHazard, riskAssessment, reportValidation, investigation),
                            DaysInStage = CalculateDaysInStage(report, primaryHazard, riskAssessment, reportValidation),
                            AssignedTo = DetermineAssignedTo(report, primaryHazard, riskAssessment, reportValidation, investigation),
                            ValidationUrl = GetValidationUrl(report, primaryHazard, reportValidation)
                        };

                        // NEW: Log when default hazard classification is detected
                        if (summary.RequiresHazardClassificationUpdate)
                        {
                            _logger.LogWarning("?? DEFAULT CLASSIFICATION DETECTED - Report {ReportId}, Hazard {HazardId}: Category='{Category}', Type='{Type}'",
                                summary.ReportId, summary.HazardId, summary.HazardCategory, summary.HazardType);
                        }

                        summaries.Add(summary);
                        
                        _logger.LogInformation("Created summary for Report {ReportCode} - RiskAssessment {AssessmentCode} covering {HazardCount} hazards with {MitigationCount} total mitigations",
                            report.Code, riskAssessment.Code, reportHazards.Count, allReportMitigations.Count);
                    }
                }
                else
                {
                    // No risk assessments - create summary with first hazard for other processing categories
                    var primaryHazard = reportHazards.First();
                    var investigation = investigations.FirstOrDefault(inv => inv.HazardCode?.Trim() == primaryHazard.Code?.Trim());
                    var investigationInterviews = investigation is not null ?
                        interviews.Where(iv => iv.InvestigationCode?.Trim() == investigation.Code?.Trim()).ToList() :
                        new List<Interview>();

                    var summary = new ReportProcessingSummary
                    {
                        ReportId = report.Code ?? "Unknown",
                        ReportDescription = report.Description ?? "No description",
                        ReportStatus = report.Status ?? ReportStatus.Created,
                        ReportStage = report.Stage ?? "New",
                        CreatedBy = report.CreatedBy ?? "Unknown",
                        CreatedDate = report.CreatedDate ?? DateTime.UtcNow,

                        HazardId = primaryHazard.Code,
                        HazardType = primaryHazard.HazardType ?? "Unknown",
                        HazardCategory = primaryHazard.HazardCategory ?? "Unknown",
                        HazardDescription = primaryHazard.Description ?? "No description",
                        Location = primaryHazard.HazardLocation?.Description ?? primaryHazard.LocationArea ?? "Not specified",
                        SubmittedBy = report.SubmittedBy ?? report.CreatedBy ?? report.UpdatedBy ?? "Unknown",
                        //ReportedDate = primaryHazard.SubmittedDate,
                        //IsAnonymous = primaryHazard.IsAnonymous,

                        // No risk assessment information
                        RiskAssessmentId = null,
                        CurrentAssessmentStep = 0,
                        RiskAssessmentStatus = "",
                        AssessmentStage = "",
                        AssessmentType = "",

                        // Report Validation Information
                        ReportValidationId = reportValidation?.Code,
                        ValidationType = reportValidation?.ValidationType ?? "",
                        ValidationDecision = reportValidation?.ValidationDecision ?? "",

                        // Investigation Information
                        InvestigationId = investigation?.Code,
                        InvestigationStatus = investigation?.Status.Value ?? "",
                        AssignedInvestigator = investigation?.AssignedInvestigatorId,
                        InvestigationNotes = investigation?.InvestigationNotes,
                        InterviewCount = investigationInterviews.Count,

                        // Empty mitigations
                        AllMitigations = new List<MitigationSummary>(),

                        // Status determination
                        StatusCategory = DetermineStatusCategory(report, primaryHazard, null, reportValidation, investigation),
                        DaysInStage = CalculateDaysInStage(report, primaryHazard, null, reportValidation),
                        AssignedTo = DetermineAssignedTo(report, primaryHazard, null, reportValidation, investigation),
                        ValidationUrl = GetValidationUrl(report, primaryHazard, reportValidation)
                    };

                    // NEW: Log when default hazard classification is detected
                    if (summary.RequiresHazardClassificationUpdate)
                    {
                        _logger.LogWarning("?? DEFAULT CLASSIFICATION DETECTED - Report {ReportId}, Hazard {HazardId}: Category='{Category}', Type='{Type}'",
                            summary.ReportId, summary.HazardId, summary.HazardCategory, summary.HazardType);
                    }

                    summaries.Add(summary);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating summary for report {ReportCode}", report.Code);
            }
        }

        _logger.LogInformation("Created {SummaryCount} report summaries with {TotalMitigationCount} total mitigations",
            summaries.Count, summaries.Sum(s => s.MitigationCount));
        return summaries;
    }

    private void CategorizeReports(List<ReportProcessingSummary> reports)
    {
        PendingValidation = reports.Where(r => r.StatusCategory == ProcessingStatusCategory.Validation).ToList();
        PendingRiskAssessment = reports.Where(r => r.StatusCategory == ProcessingStatusCategory.RiskAssessment).ToList();
        PendingInvestigation = reports.Where(r => r.StatusCategory == ProcessingStatusCategory.Investigation).ToList();
        PendingMitigation = reports.Where(r => r.StatusCategory == ProcessingStatusCategory.Mitigation  && r.MitigationCount >0).ToList();

        ClosedReferred = reports.Where(r => r.StatusCategory == ProcessingStatusCategory.Closed).ToList();

        // ? ADD DEBUG LOGGING to see what's being categorized
        //_logger.LogWarning("?? CATEGORIZATION RESULTS:");
        //_logger.LogWarning("   ?? Pending Validation: {Count}", PendingValidation.Count);
        //_logger.LogWarning("   ?? Pending Risk Assessment: {Count}", PendingRiskAssessment.Count);
        //_logger.LogWarning("   ?? Pending Investigation: {Count}", PendingInvestigation.Count);
        //_logger.LogWarning("   ??? In Mitigation: {Count}", PendingMitigation.Count);
        //_logger.LogWarning("   ? Closed/Referred: {Count}", ClosedReferred.Count);

        // ? LOG EACH REPORT'S CATEGORIZATION
        //foreach (var report in reports)
        //{
        //    _logger.LogWarning("   ?? Report {ReportId}-{HazardId}: {Category} (HasRA: {HasRA}, RAStatus: {RAStatus}, RAStep: {RAStep}, MitigationCount: {MC})",
        //        report.ReportId, report.HazardId, report.StatusCategory, 
        //        report.HasRiskAssessment, report.RiskAssessmentStatus, report.CurrentAssessmentStep, report.MitigationCount);
        //}
    }

    #endregion

    #region Helper Methods

    private ProcessingStatusCategory DetermineStatusCategory(Report report, Hazard? hazard, RiskAssessment? riskAssessment, SMS_Domain.Entities.ReportValidation? reportValidation, Investigation? investigation)
    {
        //_logger.LogWarning("?? CATEGORIZING Report: {ReportCode} | HasValidation: {HasValidation} | ValidationType: {ValidationType} | ValidationDecision: {ValidationDecision} | HasRiskAssessment: {HasRA} | HasInvestigation: {HasInv} | InvStatus: {InvStatus} | InvDecision: {InvDecision}",
         //   report.Code, reportValidation is not null, reportValidation?.ValidationType ?? "NULL", reportValidation?.ValidationDecision ?? "NULL", riskAssessment is not null, investigation is not null, investigation?.Status ?? "NULL", investigation?.DecisionType ?? "NULL");

        // CRITICAL DESIGN CONCEPT: 
        // 1. If Investigation exists and is active -> INVESTIGATION tab (HIGHEST PRIORITY)
        // 2. If no ReportValidation exists -> VALIDATION tab (needs initial validation)
        // 3. If ReportValidation exists but ValidationType is NULL or ValidationDecision is NULL -> VALIDATION tab (reset validation)
        // 4. If ReportValidation exists with decision but no RiskAssessment -> RISK ASSESSMENT tab (validated, needs risk assessment)  
        // 5. If RiskAssessment exists and in progress -> RISK ASSESSMENT tab (assessment in progress)
        // 6. If RiskAssessment complete -> MITIGATION tab

        // HIGHEST PRIORITY: Check for active investigation first - but exclude completed investigations that returned to validation
        if (investigation is not null)
        {
            // FIXED: Use more robust status checking to handle different status formats
            var investigationStatus = InvestigationStatus.FromValue(investigation.Status);

            var isActiveInvestigation = investigationStatus?.IsActiveStatus == true;


            // NEW: Check if investigation completed with ReturnToValidation - SKIP active investigation logic
            if (investigationStatus == InvestigationStatus.InvestigationComplete &&
                string.Equals(investigation.DecisionType, "ReturnToValidation", StringComparison.OrdinalIgnoreCase))
            {
                
                // Continue with validation logic below - do NOT return Investigation
            }
            else if (isActiveInvestigation)
            {
                return ProcessingStatusCategory.Investigation;
            }
            else if (investigationStatus == InvestigationStatus.InvestigationComplete)
            {
                
                // Continue with normal flow below
            }
        }

        // No validation record = needs validation
        if (reportValidation is null)
        {
            //_logger.LogWarning("? Report {ReportId} -> VALIDATION (no validation record)", report.Code);
            return ProcessingStatusCategory.Validation;
        }

        // ENHANCED: Check if validation was reset (ValidationType or ValidationDecision is null) - THIS SHOULD CATCH IT!
        if (string.IsNullOrEmpty(reportValidation.ValidationType) || string.IsNullOrEmpty(reportValidation.ValidationDecision))
        {
            //_logger.LogWarning("? Report {ReportId} -> VALIDATION (reset validation - ValidationType: {ValidationType}, ValidationDecision: {ValidationDecision}) ? EXPECTED PATH",
            //    report.Code, reportValidation.ValidationType ?? "NULL", reportValidation.ValidationDecision ?? "NULL");
            return ProcessingStatusCategory.Validation;
        }

        // Has validation with decision but no risk assessment = validated, needs risk assessment
        if (riskAssessment is null)
        {
            //_logger.LogWarning("? Report {ReportId} -> RISK ASSESSMENT (validated but no assessment)", report.Code);
            return ProcessingStatusCategory.RiskAssessment;
        }

        // Has risk assessment - determine stage based on progress
        if (riskAssessment.Status == RiskAssessmentStatus.AssessmentCreate || riskAssessment.Status == RiskAssessmentStatus.AssessmentUnderway)
        {
            // Risk assessment in progress
            var category = riskAssessment.CurrentStep switch
            {
                1 => ProcessingStatusCategory.RiskAssessment, // System Description
                2 => ProcessingStatusCategory.RiskAssessment, // Hazard Identification  
                3 => ProcessingStatusCategory.RiskAssessment, // Risk Analysis
                4 => ProcessingStatusCategory.RiskAssessment, // Risk Assessment
                5 => ProcessingStatusCategory.Mitigation,     // Risk Mitigation
                _ => ProcessingStatusCategory.RiskAssessment
            };

            //_logger.LogWarning("? Report {ReportId} -> {Category} (assessment step {Step})",
            //    report.Code, category, riskAssessment.CurrentStep);
            return category;
        }
        else if (riskAssessment.Status == RiskAssessmentStatus.AssessmentComplete)
        {
            //_logger.LogWarning("? Report {ReportId} -> MITIGATION (assessment complete)", report.Code);
            return ProcessingStatusCategory.Mitigation;
        }

        // ? NEW: Check if this report-hazard has mitigations - if so, it should be in Mitigation tab
        // This handles cases where risk assessment might be missing but mitigations exist
        if (hazard is not null)
        {
            //_logger.LogWarning("? Report {ReportId} -> Checking if should be MITIGATION (no clear RA status but hazard exists)", report.Code);
            
            // ? ENHANCED: If this is being called from CreateReportSummariesAsync, check mitigation count
            // For now, let's assume any report with an associated hazard that has made it this far
            // should be in mitigation phase unless explicitly in another category
            // This is a temporary fix - you might want to add mitigation count checking here
            return ProcessingStatusCategory.Mitigation;
        }

        // Fallback to hazard/report status for edge cases
        var effectiveStatus = hazard?.Status?.ToString() ?? report.Status ?? "New";

        _logger.LogWarning("? Report {ReportId} -> Fallback logic with status: {Status}", report.Code, effectiveStatus);

        return effectiveStatus switch
        {
            "Under Investigation - Information Needed" or "Investigation Required" or "Pending Investigation" or "UNDER_INVESTIGATION" => ProcessingStatusCategory.Investigation,
            "Mitigation Planning" or "In Progress" or "Implementation" or "Tracking" => ProcessingStatusCategory.Mitigation,
            "Closed" or "Referred" or "Completed" or "Closed - Not SMS Risk" or "CLOSED" or "CANCELLED" => ProcessingStatusCategory.Closed,
            _ => ProcessingStatusCategory.RiskAssessment
        };
    }

    

    private int CalculateDaysInStage(Report report, Hazard? hazard, RiskAssessment? riskAssessment, SMS_Domain.Entities.ReportValidation? reportValidation)
    {
        // Use the most recent update date to calculate days in current stage
        var referenceDate = riskAssessment?.UpdatedDate ??
                           reportValidation?.UpdatedDate ??
                           hazard?.UpdatedDate ?? hazard?.CreatedDate ??
                           report.UpdatedDate ?? report.CreatedDate ?? DateTime.UtcNow;
        return (DateTime.UtcNow - referenceDate).Days;
    }

    private string? DetermineAssignedTo(Report report, Hazard? hazard, RiskAssessment? riskAssessment, SMS_Domain.Entities.ReportValidation? reportValidation, Investigation? investigation)
    {
        // NEW: Use Investigation assigned investigator if available
        if (investigation is not null && !string.IsNullOrEmpty(investigation.AssignedInvestigatorId))
        {
            return investigation.AssignedInvestigatorId;
        }

        // Use RiskAssessment lead assessor if available
        if (riskAssessment is not null && !string.IsNullOrEmpty(riskAssessment.LeadAssessorId))
        {
            return riskAssessment.LeadAssessorId;
        }

        // Use ReportValidation validator if available
        if (reportValidation is not null && !string.IsNullOrEmpty(reportValidation.ValidatedBy))
        {
            return reportValidation.ValidatedBy;
        }

        // Fallback to category-based assignment
        var category = DetermineStatusCategory(report, hazard, riskAssessment, reportValidation, investigation);
        return category switch
        {
            ProcessingStatusCategory.Validation => "SMS Validation Team",
            ProcessingStatusCategory.RiskAssessment => "Risk Assessment Team",
            ProcessingStatusCategory.Investigation => "Investigative Team",
            ProcessingStatusCategory.Mitigation => "Mitigation Team",
            ProcessingStatusCategory.Closed => "Closed Cases Team",
            _ => "General Review Team"
        };
    }

    private string GetValidationUrl(Report report, Hazard? hazard, SMS_Domain.Entities.ReportValidation? reportValidation)
    {
        // If no validation exists, go to report validation
        if (reportValidation is null)
        {
            return $"/SMSRiskManagement/ReportValidation/{report.Code}";
        }

        // If validation exists but no risk assessment, start risk assessment
        // This would be for the risk assessment tab
        if (!string.IsNullOrEmpty(hazard?.Code))
        {
            return $"/SMSRiskManagement/TechnicalAssessment/{report.Code}/{hazard.Code}/1";
        }

        // Fallback
        return $"/SMSRiskManagement/ReportValidation/{report.Code}";
    }

    private void InitializeEmptyLists()
    {
        PendingValidation = new List<ReportProcessingSummary>();
        PendingRiskAssessment = new List<ReportProcessingSummary>();
        PendingInvestigation = new List<ReportProcessingSummary>();
        PendingMitigation = new List<ReportProcessingSummary>();
        ClosedReferred = new List<ReportProcessingSummary>();
    }

    #endregion

    #region Tab Rendering

    private RenderFragment RenderValidationTab()
    {
        return builder =>
        {
            if (IsLoading)
            {
                builder.OpenComponent<RadzenProgressBarCircular>(0);
                builder.AddAttribute(1, "ShowValue", false);
                builder.CloseComponent();
                return;
            }

            if (!PendingValidation.Any())
            {
                RenderEmptyState(builder, "check_circle", "No reports pending validation", "New reports will appear here for SMS risk determination");
                return;
            }

            builder.OpenComponent<RadzenDataGrid<ReportProcessingSummary>>(0);
            builder.AddAttribute(1, "Data", PendingValidation);
            builder.AddAttribute(2, "AllowSorting", true);
            builder.AddAttribute(3, "AllowPaging", true);
            builder.AddAttribute(4, "PageSize", 10);
            builder.AddAttribute(5, "Columns", (RenderFragment)(columnsBuilder =>
            {
                RenderValidationColumns(columnsBuilder);
            }));
            builder.CloseComponent();
        };
    }

    private RenderFragment RenderRiskAssessmentTab()
    {
        return builder =>
        {
            if (IsLoading)
            {
                builder.OpenComponent<RadzenProgressBarCircular>(0);
                builder.AddAttribute(1, "ShowValue", false);
                builder.CloseComponent();
                return;
            }

            if (!PendingRiskAssessment.Any())
            {
                RenderEmptyState(builder, "assessment", "No reports pending risk assessment", "Validated reports will appear here for risk assessment");
                return;
            }

            builder.OpenComponent<RadzenDataGrid<ReportProcessingSummary>>(0);
            builder.AddAttribute(1, "Data", PendingRiskAssessment);
            builder.AddAttribute(2, "AllowSorting", true);
            builder.AddAttribute(3, "AllowPaging", true);
            builder.AddAttribute(4, "PageSize", 10);
            builder.AddAttribute(5, "Columns", (RenderFragment)(columnsBuilder =>
            {
                RenderRiskAssessmentColumns(columnsBuilder);
            }));
            builder.CloseComponent();
        };
    }

    private RenderFragment RenderInvestigationTab()
    {
        return builder =>
        {
            if (IsLoading)
            {
                builder.OpenComponent<RadzenProgressBarCircular>(0);
                builder.AddAttribute(1, "ShowValue", false);
                builder.CloseComponent();
                return;
            }

            if (!PendingInvestigation.Any())
            {
                RenderEmptyState(builder, "search", "No reports requiring investigation", "Reports needing more information will appear here");
                return;
            }

            builder.OpenComponent<RadzenDataGrid<ReportProcessingSummary>>(0);
            builder.AddAttribute(1, "Data", PendingInvestigation);
            builder.AddAttribute(2, "AllowSorting", true);
            builder.AddAttribute(3, "AllowPaging", true);
            builder.AddAttribute(4, "PageSize", 10);
            builder.AddAttribute(5, "Columns", (RenderFragment)(columnsBuilder =>
            {
                RenderInvestigationColumns(columnsBuilder);
            }));
            builder.CloseComponent();
        };
    }

    private RenderFragment RenderMitigationTab()
    {
        return builder =>
        {
            if (IsLoading)
            {
                builder.OpenComponent<RadzenProgressBarCircular>(0);
                builder.AddAttribute(1, "ShowValue", false);
                builder.CloseComponent();
                return;
            }

            if (!PendingMitigation.Any())
            {
                RenderEmptyState(builder, "build", "No reports in mitigation phase", "Approved risk assessments will appear here");
                return;
            }

            // ?? NEW: Render detailed mitigation view instead of simple data grid
            RenderDetailedMitigationView(builder);
        };
    }

    private void RenderDetailedMitigationView(RenderTreeBuilder builder)
    {
        builder.OpenComponent<RadzenStack>(0);
        builder.AddAttribute(1, "Gap", "1.5rem");
        builder.AddAttribute(2, "ChildContent", (RenderFragment)(stackBuilder =>
        {
            foreach (var report in PendingMitigation)
            {
                // Render each report as a card with its mitigations
                RenderReportMitigationCard(stackBuilder, report);
            }
        }));
        builder.CloseComponent(); // ? FIXED: Close RadzenStack
    }

    private void RenderReportMitigationCard(RenderTreeBuilder builder, ReportProcessingSummary report)
    {
        builder.OpenComponent<RadzenCard>(0);
        builder.AddAttribute(1, "Variant", Variant.Outlined);
        builder.AddAttribute(2, "Class", "mb-3");
        builder.AddAttribute(3, "Style", "border-left: 4px solid var(--rz-primary);");
        builder.AddAttribute(4, "ChildContent", (RenderFragment)(cardBuilder =>
        {
            cardBuilder.OpenComponent<RadzenStack>(0);
            cardBuilder.AddAttribute(1, "Gap", "1rem");
            cardBuilder.AddAttribute(2, "ChildContent", (RenderFragment)(stackBuilder =>
            {
                // Header Row with Report/Hazard info and Bulk Approve
                stackBuilder.OpenComponent<RadzenRow>(0);
                stackBuilder.AddAttribute(1, "AlignItems", AlignItems.Center);
                stackBuilder.AddAttribute(2, "JustifyContent", JustifyContent.SpaceBetween);
                stackBuilder.AddAttribute(3, "ChildContent", (RenderFragment)(headerBuilder =>
                {
                    // Left side - Report/Hazard info
                    headerBuilder.OpenComponent<RadzenColumn>(0);
                    headerBuilder.AddAttribute(1, "Size", 8); // ? FIXED: Use int instead of string
                    headerBuilder.AddAttribute(2, "ChildContent", (RenderFragment)(leftBuilder =>
                    {
                        leftBuilder.OpenComponent<RadzenStack>(0);
                        leftBuilder.AddAttribute(1, "Gap", "0.5rem");
                        leftBuilder.AddAttribute(2, "ChildContent", (RenderFragment)(infoBuilder =>
                        {
                            infoBuilder.OpenComponent<RadzenText>(0);
                            infoBuilder.AddAttribute(1, "TextStyle", TextStyle.H6);
                            infoBuilder.AddAttribute(2, "Style", "color: #212e61; margin: 0;");
                            infoBuilder.AddAttribute(3, "Text", $"Report {report.ReportId} - Hazard {report.HazardId}");
                            infoBuilder.CloseComponent(); // ? Close RadzenText

                            infoBuilder.OpenComponent<RadzenText>(5);
                            infoBuilder.AddAttribute(6, "TextStyle", TextStyle.Body2);
                            infoBuilder.AddAttribute(7, "Style", "color: var(--rz-text-secondary-color);");
                            infoBuilder.AddAttribute(8, "Text", report.HazardDescription);
                            infoBuilder.CloseComponent(); // ? Close RadzenText

                            infoBuilder.OpenComponent<RadzenStack>(10);
                            infoBuilder.AddAttribute(11, "Orientation", Orientation.Horizontal);
                            infoBuilder.AddAttribute(12, "Gap", "0.5rem");
                            infoBuilder.AddAttribute(13, "AlignItems", AlignItems.Center);
                            infoBuilder.AddAttribute(14, "ChildContent", (RenderFragment)(badgeBuilder =>
                            {
                                badgeBuilder.OpenComponent<RadzenBadge>(0);
                                badgeBuilder.AddAttribute(1, "Text", $"{report.MitigationCount} Mitigations");
                                badgeBuilder.AddAttribute(2, "BadgeStyle", BadgeStyle.Base);
                                badgeBuilder.CloseComponent(); // ? Close RadzenBadge

                                
                            }));
                            infoBuilder.CloseComponent(); // ? Close RadzenStack
                        }));
                        leftBuilder.CloseComponent(); // ? Close RadzenStack
                    }));
                    headerBuilder.CloseComponent(); // ? Close RadzenColumn

                    // Right side - Bulk Approve button
                    headerBuilder.OpenComponent<RadzenColumn>(10);
                    headerBuilder.AddAttribute(11, "Size", 4); // ? FIXED: Use int instead of string
                    headerBuilder.AddAttribute(12, "Style", "text-align: right;");
                    headerBuilder.AddAttribute(13, "ChildContent", (RenderFragment)(rightBuilder =>
                    {
                        if (report.HasMitigations)
                        {
                            rightBuilder.OpenComponent<RadzenButton>(0);
                            rightBuilder.AddAttribute(1, "Text", $"Bulk Approve ({GetApprovableMitigationCount(report)})");
                            rightBuilder.AddAttribute(2, "Icon", "verified");
                            rightBuilder.AddAttribute(3, "ButtonStyle", ButtonStyle.Success);
                            rightBuilder.AddAttribute(4, "Size", ButtonSize.Medium);
                            rightBuilder.AddAttribute(5, "Click", EventCallback.Factory.Create<MouseEventArgs>(this,
                                (args) => ShowBulkApprovalConfirmation(report)));
                            rightBuilder.AddAttribute(6, "Disabled", IsProcessingApproval || GetApprovableMitigationCount(report) == 0);
                            rightBuilder.CloseComponent(); // ? Close RadzenButton
                        }
                    }));
                    headerBuilder.CloseComponent(); // ? Close RadzenColumn
                }));
                stackBuilder.CloseComponent(); // ? Close RadzenRow

                // Mitigations List
                if (report.HasMitigations && report.AllMitigations.Any())
                {
                    stackBuilder.OpenComponent<RadzenDataGrid<MitigationSummary>>(20);
                    stackBuilder.AddAttribute(21, "Data", report.AllMitigations);
                    stackBuilder.AddAttribute(22, "AllowSorting", true);
                    stackBuilder.AddAttribute(23, "AllowPaging", false);
                    stackBuilder.AddAttribute(24, "Columns", (RenderFragment)(mitigationColumnsBuilder =>
                    {
                        RenderMitigationDetailColumns(mitigationColumnsBuilder, report);
                    }));
                    stackBuilder.CloseComponent(); // ? Close RadzenDataGrid
                }
                else
                {
                    stackBuilder.OpenComponent<RadzenAlert>(30);
                    stackBuilder.AddAttribute(31, "AlertStyle", AlertStyle.Base);
                    stackBuilder.AddAttribute(32, "Icon", "info");
                    stackBuilder.AddAttribute(33, "ShowIcon", true);
                    stackBuilder.AddAttribute(34, "Text", "No mitigations found for this report-hazard combination.");
                    stackBuilder.CloseComponent(); // ? Close RadzenAlert
                }
            }));
            cardBuilder.CloseComponent(); // ? Close RadzenStack (inner)
        }));
        builder.CloseComponent(); // ? Close RadzenCard
    }

    private void RenderMitigationDetailColumns(RenderTreeBuilder builder, ReportProcessingSummary report)
    {
        // Mitigation Code Column
        builder.OpenComponent<RadzenDataGridColumn<MitigationSummary>>(0);
        builder.AddAttribute(1, "Property", "MitigationCode");
        builder.AddAttribute(2, "Title", "Mitigation ID");
        builder.AddAttribute(3, "Width", "110px");
        builder.AddAttribute(4, "Template", (RenderFragment<MitigationSummary>)(mitigation =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenText>(0);
                templateBuilder.AddAttribute(1, "style", BasicTextStyle);
                templateBuilder.AddAttribute(2, "Text", !string.IsNullOrEmpty(mitigation.MitigationCode) ? mitigation.MitigationCode : "Not Specified");
                templateBuilder.CloseComponent();
            }
            )));
        builder.CloseComponent();
        
        

        // Mitigation Name Column
        builder.OpenComponent<RadzenDataGridColumn<MitigationSummary>>(10);
        builder.AddAttribute(11, "Property", "MitigationName");
        builder.AddAttribute(12, "Title", "Name");
        builder.AddAttribute(13, "Width", "250px");
        builder.AddAttribute(14, "Template", (RenderFragment<MitigationSummary>)(mitigation =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenText>(0);
                templateBuilder.AddAttribute(1, "style", BasicTextStyle);
                templateBuilder.AddAttribute(2, "Text", !string.IsNullOrEmpty(mitigation.MitigationName) ? mitigation.MitigationName : "Not Specified");
                templateBuilder.CloseComponent();
            }
            )));
        builder.CloseComponent();

        // Status Column
        builder.OpenComponent<RadzenDataGridColumn<MitigationSummary>>(20);
        builder.AddAttribute(21, "Property", "Status");
        builder.AddAttribute(22, "Title", "Status");
        builder.AddAttribute(23, "Width", "120px");
        builder.AddAttribute(14, "Template", (RenderFragment<MitigationSummary>)(mitigation =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenText>(0);
                templateBuilder.AddAttribute(1, "style", BasicTextStyle);
                templateBuilder.AddAttribute(2, "Text", !string.IsNullOrEmpty(mitigation.Status) ? mitigation.Status : "Not Specified");
                templateBuilder.CloseComponent();
            }
            )));
        builder.CloseComponent();

        
        // Target Date Column
        builder.OpenComponent<RadzenDataGridColumn<MitigationSummary>>(40);
        builder.AddAttribute(41, "Property", "TargetDate");
        builder.AddAttribute(42, "Title", "Target Date");
        builder.AddAttribute(43, "Width", "120px");
        builder.AddAttribute(44, "FormatString", "{0:MM/dd/yyyy}");
        builder.AddAttribute(45, "Template", (RenderFragment<MitigationSummary>)(mitigation =>
            (templateBuilder =>
            {
                if (mitigation.TargetDate.HasValue)
                {
                    templateBuilder.OpenComponent<RadzenText>(0);
                    templateBuilder.AddAttribute(1, "style", BasicTextStyle);
                    templateBuilder.AddAttribute(2, "Text", mitigation.TargetDate.Value.ToString("MM/dd/yyyy"));
                    templateBuilder.CloseComponent(); 

        
                }
                else
                {
                    templateBuilder.OpenComponent<RadzenText>(10);
                    templateBuilder.AddAttribute(11, "style", BasicTextStyle);
                    templateBuilder.AddAttribute(12, "Text", "Not set");
                    templateBuilder.CloseComponent(); 
                }
            })));
        builder.CloseComponent(); // ? Close RadzenDataGridColumn

        builder.OpenComponent<RadzenDataGridColumn<MitigationSummary>>(50);
        builder.AddAttribute(51, "Property", "AssignedDepartment");
        builder.AddAttribute(52, "Title", "Assigned Department");
        builder.AddAttribute(53, "Width", "250px");
        builder.AddAttribute(54, "Template", (RenderFragment<MitigationSummary>)(mitigation =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenText>(0);
                templateBuilder.AddAttribute(1, "style", BasicTextStyle);
                templateBuilder.AddAttribute(2, "Text", !string.IsNullOrEmpty(mitigation.AssignedDepartment) ? mitigation.AssignedDepartment : "Not assigned");
                templateBuilder.CloseComponent(); // ? Close RadzenText
            })));
        builder.CloseComponent(); // ? Close RadzenDataGridColumn

        // Responsible Party Column
        builder.OpenComponent<RadzenDataGridColumn<MitigationSummary>>(60);
        builder.AddAttribute(61, "Property", "AssignedTo");
        builder.AddAttribute(62, "Title", "Assigned To");
        builder.AddAttribute(63, "Width", "150px");
        builder.AddAttribute(64, "Template", (RenderFragment<MitigationSummary>)(mitigation =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenText>(0);
                templateBuilder.AddAttribute(1, "style", BasicTextStyle);
                templateBuilder.AddAttribute(2, "Text", !string.IsNullOrEmpty(mitigation.AssignedTo) ? mitigation.AssignedTo : "Not assigned");
                templateBuilder.CloseComponent(); // ? Close RadzenText
            })));
        builder.CloseComponent(); // ? Close RadzenDataGridColumn

        // Actions Column - Individual Edit buttons
        builder.OpenComponent<RadzenDataGridColumn<MitigationSummary>>(70);
        builder.AddAttribute(71, "Title", "Actions");
        builder.AddAttribute(72, "Width", "200px");
        builder.AddAttribute(73, "Sortable", false);
        builder.AddAttribute(74, "Template", (RenderFragment<MitigationSummary>)(mitigation =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenStack>(0);
                templateBuilder.AddAttribute(1, "Orientation", Orientation.Horizontal);
                templateBuilder.AddAttribute(2, "Gap", "0.25rem");
                templateBuilder.AddAttribute(3, "ChildContent", (RenderFragment)(actionBuilder =>
                {
                    // Edit Button
                    actionBuilder.OpenComponent<RadzenButton>(0);
                    actionBuilder.AddAttribute(1, "Text", "Edit");
                    actionBuilder.AddAttribute(2, "Icon", "edit");
                    actionBuilder.AddAttribute(3, "ButtonStyle", ButtonStyle.Primary);
                    actionBuilder.AddAttribute(4, "Size", ButtonSize.Small);
                    actionBuilder.AddAttribute(5, "Click", EventCallback.Factory.Create<MouseEventArgs>(this,
                        (args) => NavigateToMitigationEdit(mitigation)));
                    actionBuilder.CloseComponent(); // ? Close RadzenButton

                   
                    // View Button
                    actionBuilder.OpenComponent<RadzenButton>(10);
                    actionBuilder.AddAttribute(11, "Text", "View");
                    actionBuilder.AddAttribute(12, "Icon", "visibility");
                    actionBuilder.AddAttribute(13, "ButtonStyle", ButtonStyle.Base);
                    actionBuilder.AddAttribute(14, "Size", ButtonSize.Small);
                    actionBuilder.AddAttribute(15, "Click", EventCallback.Factory.Create<MouseEventArgs>(this,
                        (args) => ViewMitigationDetails(mitigation)));
                    actionBuilder.CloseComponent(); // ? Close RadzenButton
                }));
                templateBuilder.CloseComponent(); // ? Close RadzenStack
            })));
        builder.CloseComponent(); // ? Close RadzenDataGridColumn
    }

    // Helper methods for mitigation actions
    private void NavigateToMitigationEdit(MitigationSummary mitigation)
    {
        try
        {
            _logger.LogInformation("Navigating to edit mitigation: {Code}", mitigation.MitigationCode);
            _navigation.NavigateToSecure($"/SMSRiskManagement/HazardMitigation/Edit/{mitigation.MitigationCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to mitigation edit for {Code}", mitigation.MitigationCode);
            ShowErrorAsyncNotification("Error navigating to mitigation editor");
        }
    }

    private async Task QuickApproveMitigation(MitigationSummary mitigation)
    {
        try
        {
            IsProcessingApproval = true;
            StateHasChanged();

            _logger.LogInformation("Quick approving mitigation: {Code}", mitigation.MitigationCode);

            // Load the full mitigation entity
            var mitigationQuery = new GetMitigationByCodeQuery(new MitigationID(mitigation.MitigationCode));
            var mitigationResult = await _mediator.SendAsync(mitigationQuery, CancellationToken.None);

            if (mitigationResult.IsSuccess && mitigationResult.Value is not null)
            {
                var fullMitigation = mitigationResult.Value;
                fullMitigation.Status = MitigationStatus.Approved;
                fullMitigation.ApprovedBy = mitigation.ApprovedBy;
                fullMitigation.UpdatedDate = DateTime.UtcNow;
                fullMitigation.UpdatedBy = CurrentUserService.UserCode; // You might want to get the current user

                var updateCommand = new UpdateMitigationCommand(fullMitigation);
                var updateResult = await _mediator.SendAsync(updateCommand, CancellationToken.None);

                if (updateResult.IsSuccess)
                {
                    ShowSuccessAsyncNotification($"Mitigation {mitigation.MitigationCode} approved successfully");

                    // Update the local summary
                    mitigation.Status = MitigationStatus.Approved;

                    // Reload data to reflect changes
                    await LoadDataAsync();
                }
                else
                {
                    ShowErrorAsyncNotification($"Failed to approve mitigation: {updateResult.Error?.Message}");
                }
            }
            else
            {
                ShowErrorAsyncNotification($"Failed to load mitigation details: {mitigationResult.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error quick approving mitigation {Code}", mitigation.MitigationCode);
            ShowErrorAsyncNotification("Error approving mitigation");
        }
        finally
        {
            IsProcessingApproval = false;
            StateHasChanged();
        }
    }

    private void ViewMitigationDetails(MitigationSummary mitigation)
    {
        try
        {
            _logger.LogInformation("Viewing mitigation details: {Code}", mitigation.MitigationCode);
            // You might want to show a details dialog or navigate to a details page
            ShowInfoAsyncNotification($"Details for mitigation {mitigation.MitigationCode} - Feature to be implemented");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error viewing mitigation details for {Code}", mitigation.MitigationCode);
            ShowErrorAsyncNotification("Error viewing mitigation details");
        }
    }

    private int GetApprovableMitigationCount(ReportProcessingSummary report)
    {
        return report.AllMitigations?.Count(m => m.Status == MitigationStatus.PendingApproval.Value) ?? 0;
    }
    private List<ApproverOption> GetApproversForHighestRiskLevel(ReportProcessingSummary report)
    {
        var highestRiskLevel = GetHighestRiskLevelAcrossAllHazards(report);

        var authorizedApprovers = AvailableApprovers
            .Where(user => CanApproveRiskLevel(user, highestRiskLevel))
            .Select(user => ApproverOption.FromUser(user))
            .OrderBy(a => a.DisplayName)
            .ToList();

        _logger.LogInformation("Found {Count} approvers authorized for {RiskLevel} risk level approval",
            authorizedApprovers.Count, highestRiskLevel);

        return authorizedApprovers;
    }
    /// <summary>
    /// ?? Get count of unique hazards in the report
    /// </summary>
    private int GetUniqueHazardCount(ReportProcessingSummary report)
    {
        return report.AllMitigations
            .Select(m => m.HazardCode)
            .Distinct()
            .Count();
    }
    /// <summary>
    /// ?? Get approver details for display with enhanced information
    /// </summary>
    private ApproverOption GetApproverDetails(string approverCode)
    {
        var approver = AvailableApprovers.FirstOrDefault(a => a.Code == approverCode);
        if (approver is null)
            return new ApproverOption { Code = approverCode, DisplayName = "Unknown" };

        return new ApproverOption
        {
            Code = approver.Code,
            DisplayName = $"{approver.FirstName?.Value} {approver.LastName?.Value} ({approver.Position})",
            AuthorityLevel = approver.AuthorityLevel?.ToString() ?? "Not specified",
            RiskApprovalAuthority = approver.RiskApprovalAuthority ?? "Not specified",
            Department = approver.Department?.Name ?? "Not specified",
            Position = approver.Position ?? "Not specified"
        };
    }
    // Notification helper methods (EventBus-driven)
    private async Task ShowSuccessAsyncNotification(string message)
    {
        await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", message));
    }

    private async Task ShowErrorAsyncNotification(string message)
    {
        await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", message));
    }

    private async Task ShowInfoAsyncNotification(string message)
    {
        await _eventBus.PublishUIEventAsync(UINotificationEvent.Info("Information", message));
    }

    private void RenderEmptyState(RenderTreeBuilder builder, string icon, string title, string description)
    {
        builder.OpenComponent<RadzenStack>(0);
        builder.AddAttribute(1, "Orientation", Orientation.Vertical);
        builder.AddAttribute(2, "AlignItems", AlignItems.Center);
        builder.AddAttribute(3, "Gap", "1rem");
        builder.AddAttribute(4, "Style", "padding: 3rem;");
        builder.AddAttribute(5, "ChildContent", (RenderFragment)(stackBuilder =>
        {
            stackBuilder.OpenComponent<RadzenIcon>(0);
            stackBuilder.AddAttribute(1, "Icon", icon);
            stackBuilder.AddAttribute(2, "Style", "font-size: 4rem; color: var(--rz-text-disabled-color);");
            stackBuilder.CloseComponent();

            stackBuilder.OpenComponent<RadzenText>(3);
            stackBuilder.AddAttribute(4, "TextStyle", TextStyle.H6);
            stackBuilder.AddAttribute(5, "Style", "color: var(--rz-text-disabled-color);");
            stackBuilder.AddAttribute(6, "Text", title);
            stackBuilder.CloseComponent();

            stackBuilder.OpenComponent<RadzenText>(7);
            stackBuilder.AddAttribute(8, "TextStyle", TextStyle.Body1);
            stackBuilder.AddAttribute(9, "Style", "color: var(--rz-text-disabled-color);");
            stackBuilder.AddAttribute(10, "Text", description);
            stackBuilder.CloseComponent();
        }));
        builder.CloseComponent();
    }

    private void RenderValidationColumns(RenderTreeBuilder builder)
    {
        RenderReportIdColumn(builder);
        RenderHazardIdColumn(builder);

        // NEW: Add hazard classification indicator column
        RenderHazardCategoryTypeColumn(builder);

        RenderHazardDescriptionColumn(builder);
        //Status
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(30);
        builder.AddAttribute(31, "Property", "ReportStatus");
        builder.AddAttribute(32, "Title", "Status");
        builder.AddAttribute(33, "Width", "150px");
        builder.AddAttribute(34, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenText>(0);
                templateBuilder.AddAttribute(1, "style", "font-size:smaller");
                templateBuilder.AddAttribute(2, "Text", !string.IsNullOrEmpty(report.ReportStatus) ? report.ReportStatus : "Not Specified");
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();
        // Stage Column DON"T THINK WE NEED THIS FOR NOW
        //////////builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(40);
        //////////builder.AddAttribute(41, "Property", "ReportStage");
        //////////builder.AddAttribute(42, "Title", "Stage");
        //////////builder.AddAttribute(43, "Width", "120px");
        //////////builder.AddAttribute(44, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
        //////////    (templateBuilder =>
        //////////    {
        //////////        templateBuilder.OpenComponent<RadzenText>(0);
        //////////        templateBuilder.AddAttribute(1, "style", "font-size:smaller");
        //////////        templateBuilder.AddAttribute(2, "Text", !string.IsNullOrEmpty(report.ReportStage.ToUpper()) ? report.ReportStage.ToUpper() : "Not Specified");
        //////////        templateBuilder.CloseComponent();
        //////////    })));
        //////////builder.CloseComponent();

        // Reported By Column (FIXED: Ensure proper data binding)
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(60);
        builder.AddAttribute(61, "Property", "SubmittedBy");
        builder.AddAttribute(62, "Title", "Submitted By");
        builder.AddAttribute(63, "Width", "120px");
        builder.AddAttribute(64, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenText>(0);
                templateBuilder.AddAttribute(1, "style", "font-size:smaller");
                templateBuilder.AddAttribute(2, "Text", !string.IsNullOrEmpty(report.SubmittedBy) ? report.SubmittedBy : "Not Specified");
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

           RenderValidationActionColumn(builder);
    }

    private void RenderHazardCategoryTypeColumn(RenderTreeBuilder builder)
    {
        // NEW: Hazard Classification Status Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(7);
        builder.AddAttribute(8, "Title", "Category / Type");
        builder.AddAttribute(9, "Width", "375px");
        builder.AddAttribute(10, "Sortable", false);
        builder.AddAttribute(11, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                if (report.RequiresHazardClassificationUpdate)
                {
                    // Show orange warning badge for default values
                    templateBuilder.OpenComponent<RadzenBadge>(0);
                    templateBuilder.AddAttribute(1, "BadgeStyle", BadgeStyle.Warning);
                    //templateBuilder.AddAttribute(2, "Text", "DEFAULTS");
                    templateBuilder.AddAttribute(2, "Text", $"Category: {report.HazardCategory}, Type: {report.HazardType}");
                    templateBuilder.AddAttribute(3, "Title", $"Category: {report.HazardCategory}, Type: {report.HazardType}");
                    templateBuilder.CloseComponent();
                }
                else
                {
                    // Show green success badge for properly classified
                    templateBuilder.OpenComponent<RadzenBadge>(10);
                    templateBuilder.AddAttribute(11, "BadgeStyle", BadgeStyle.Success);
                    //templateBuilder.AddAttribute(12, "Text", "CLASSIFIED");
                    templateBuilder.AddAttribute(12, "Text", $"Category: {report.HazardCategory}, Type: {report.HazardType}");
                    templateBuilder.AddAttribute(13, "Title", $"Category: {report.HazardCategory}, Type: {report.HazardType}");
                    templateBuilder.CloseComponent();
                }
            })));
        builder.CloseComponent();
    }

    private void RenderValidationActionColumn(RenderTreeBuilder builder)
    {
        // Actions Column - NEW: Enhanced to handle default hazard classification scenario
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(80);
        builder.AddAttribute(81, "Title", "Actions");
        builder.AddAttribute(82, "Width", "170px"); // Slightly wider for "Validate Report" text
        builder.AddAttribute(83, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                // NEW: Determine button properties based on default hazard classification
                var buttonText = report.ActionButtonText;
                var buttonStyle = report.RequiresHazardClassificationUpdate ? ButtonStyle.Warning : ButtonStyle.Success;
                var buttonIcon = report.RequiresHazardClassificationUpdate ? "warning" : "check_circle";
                var navigationUrl = report.SmartUrl;

                templateBuilder.OpenComponent<RadzenButton>(0);
                templateBuilder.AddAttribute(1, "Text", buttonText);
                templateBuilder.AddAttribute(2, "Icon", buttonIcon);
                templateBuilder.AddAttribute(3, "ButtonStyle", buttonStyle);
                templateBuilder.AddAttribute(4, "Size", ButtonSize.Small);
                templateBuilder.AddAttribute(5, "Click", EventCallback.Factory.Create<MouseEventArgs>(this,
                    (args) => _navigation.NavigateToSecure(navigationUrl)));
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();
    }

    private void RenderReportIdColumn(RenderTreeBuilder builder)
    {
        // Report ID Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(0);
        builder.AddAttribute(1, "Property", "ReportId");
        builder.AddAttribute(2, "Title", "Report ID");
        builder.AddAttribute(3, "Width", "100px");
        builder.AddAttribute(4, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
                {
                    templateBuilder.OpenComponent<RadzenText>(0);
                    templateBuilder.AddAttribute(1, "style", BasicTextStyle);
                    templateBuilder.AddAttribute(2, "Text", !string.IsNullOrEmpty(report.ReportId) ? report.ReportId : "Not Specified");
                    templateBuilder.CloseComponent();
                }
            )));
        builder.CloseComponent();
    }
    private void RenderInvestigationIdColumn(RenderTreeBuilder builder)
    {
        // Report ID Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(0);
        builder.AddAttribute(1, "Property", "InvestigationId");
        builder.AddAttribute(2, "Title", "Investigation ID");
        builder.AddAttribute(3, "Width", "150px");
        builder.AddAttribute(4, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenText>(0);
                templateBuilder.AddAttribute(1, "style", BasicTextStyle);
                templateBuilder.AddAttribute(2, "Text", !string.IsNullOrEmpty(report.InvestigationId) ? report.InvestigationId : "Not Specified");
                templateBuilder.CloseComponent();
            }
            )));
        builder.CloseComponent();
    }
    private void RenderHazardIdColumn(RenderTreeBuilder builder)
    {
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(5);
        builder.AddAttribute(6, "Property", "HazardId");
        builder.AddAttribute(7, "Title", "Hazard ID");
        builder.AddAttribute(8, "Width", "100px");
        builder.AddAttribute(9, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenText>(0);
                templateBuilder.AddAttribute(1, "style", BasicTextStyle);
                templateBuilder.AddAttribute(2, "Text", !string.IsNullOrEmpty(report.HazardId) ? report.HazardId : "Not Specified");
                templateBuilder.CloseComponent();
            }
            )));
        builder.CloseComponent();
    }

    private void RenderRiskAssessmentIdColumn(RenderTreeBuilder builder)
    {
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(10);
        builder.AddAttribute(11, "Property", "RiskAssessmentId");
        builder.AddAttribute(12, "Title", "Risk Assessment ID");
        builder.AddAttribute(13, "Width", "100px");
        builder.AddAttribute(14, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenText>(0);
                templateBuilder.AddAttribute(1, "style", BasicTextStyle);
                templateBuilder.AddAttribute(2, "Text", !string.IsNullOrEmpty(report.RiskAssessmentId) ? report.RiskAssessmentId : "Not Specified");
                templateBuilder.CloseComponent();
            }
            )));
        builder.CloseComponent();
    }

    private void RenderHazardDescriptionColumn(RenderTreeBuilder builder, bool includeActions = true)
    {
        // Description Column - NEW: Changed to button for modal dialog
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(10);
        builder.AddAttribute(11, "Title", "Description");
        builder.AddAttribute(12, "Width", "100px");
        builder.AddAttribute(13, "Sortable", false);
        builder.AddAttribute(14, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenButton>(0);
                templateBuilder.AddAttribute(1, "Text", "Description");
                templateBuilder.AddAttribute(2, "Icon", "description");
                templateBuilder.AddAttribute(3, "ButtonStyle", ButtonStyle.Base);
                templateBuilder.AddAttribute(4, "Variant", Variant.Text);
                templateBuilder.AddAttribute(5, "Size", ButtonSize.ExtraSmall);
                templateBuilder.AddAttribute(6, "Title", "Click to view full description");
                templateBuilder.AddAttribute(7, "Class", "description-button");
                templateBuilder.AddAttribute(8, "Click", EventCallback.Factory.Create<MouseEventArgs>(this,
                    (args) => ShowDescriptionDialog(report)));
                templateBuilder.CloseComponent();
            }
             )));
        builder.CloseComponent();
    }

    private void RenderRiskAssessmentColumns(RenderTreeBuilder builder)
    {
        RenderReportIdColumn(builder);
        RenderHazardIdColumn(builder);
        RenderRiskAssessmentIdColumn(builder);
        RenderHazardDescriptionColumn(builder, false);
        // Assigned Investigator Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(20);
        builder.AddAttribute(21, "Property", "AssignedTo");
        builder.AddAttribute(22, "Title", "Assigned To");
        builder.AddAttribute(23, "Width", "125px");
        builder.AddAttribute(24, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenText>(0);
                templateBuilder.AddAttribute(1, "style", BasicTextStyle);
                templateBuilder.AddAttribute(2, "Text", report.AssignedTo ?? "Not Assigned");
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();
        RenderRiskAssessmentActionColumn(builder);
    }

    private void RenderRiskAssessmentActionColumn(RenderTreeBuilder builder)
    {
        // Actions Column - NEW: Moved the action button to the far right
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(100);
        builder.AddAttribute(101, "Title", "Actions");
        builder.AddAttribute(102, "Width", "200px");
        builder.AddAttribute(103, "Sortable", false);
        builder.AddAttribute(104, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenButton>(0);
                templateBuilder.AddAttribute(1, "Text", report.ActionButtonText);
                templateBuilder.AddAttribute(2, "Icon", GetAssessmentIcon(report));
                templateBuilder.AddAttribute(3, "ButtonStyle", GetAssessmentButtonStyle(report));
                templateBuilder.AddAttribute(4, "Size", ButtonSize.Small);
                templateBuilder.AddAttribute(5, "Click", EventCallback.Factory.Create<MouseEventArgs>(this,
                    (args) => _navigation.NavigateToSecure(report.SmartUrl)));
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();
    }

    private void RenderInvestigationColumns(RenderTreeBuilder builder)
    {
        RenderReportIdColumn(builder);
        RenderHazardIdColumn(builder);
        RenderInvestigationIdColumn(builder);
        RenderHazardDescriptionColumn(builder);

        // Investigation Status Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(15);
        builder.AddAttribute(16, "Property", "InvestigationStatus");
        builder.AddAttribute(17, "Title", "Status");
        builder.AddAttribute(18, "Width", "150px");
        builder.AddAttribute(19, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                var statusText = report.HasInvestigation ? report.InvestigationStatus : "Not Started";

                templateBuilder.OpenComponent<RadzenText>(0);
                templateBuilder.AddAttribute(1, "style", BasicTextStyle);
                templateBuilder.AddAttribute(2, "Text", !string.IsNullOrEmpty(report.InvestigationStatus) ? report.InvestigationStatus : "Not Specified");
                templateBuilder.CloseComponent();
            }
            )));
        builder.CloseComponent();

        // Assigned Investigator Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(20);
        builder.AddAttribute(21, "Property", "AssignedInvestigator");
        builder.AddAttribute(22, "Title", "Assigned To");
        builder.AddAttribute(23, "Width", "125px");
        builder.AddAttribute(24, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenText>(0);
                templateBuilder.AddAttribute(1, "style", BasicTextStyle);
                templateBuilder.AddAttribute(2, "Text", report.AssignedInvestigator ?? "Not Assigned");
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

        // Interview Count Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(25);
        builder.AddAttribute(26, "Property", "InterviewCount");
        builder.AddAttribute(27, "Title", "Interviews");
        builder.AddAttribute(28, "Width", "80px");
        builder.AddAttribute(29, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenText>(0);
                templateBuilder.AddAttribute(1, "style", BasicTextStyle);
                templateBuilder.AddAttribute(2, "Text", report.InterviewCount.ToString() ?? "Not Assigned");
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

        // Actions Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(50);
        builder.AddAttribute(51, "Title", "Actions");
        builder.AddAttribute(52, "Width", "175px");
        builder.AddAttribute(53, "Sortable", false);
        builder.AddAttribute(54, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                var buttonText = report.HasInvestigation ? "Continue Investigation" : "Start Investigation";
                var buttonIcon = report.HasInvestigation ? "edit" : "search";
                var buttonStyle = report.HasInvestigation ? ButtonStyle.Primary : ButtonStyle.Warning;

                templateBuilder.OpenComponent<RadzenButton>(0);
                templateBuilder.AddAttribute(1, "Text", buttonText);
                templateBuilder.AddAttribute(2, "Icon", buttonIcon);
                templateBuilder.AddAttribute(3, "ButtonStyle", buttonStyle);
                templateBuilder.AddAttribute(4, "Size", ButtonSize.Small);
                templateBuilder.AddAttribute(5, "Click", EventCallback.Factory.Create<MouseEventArgs>(this,(args) => _navigation.NavigateTo(report.SmartUrl)));
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();
    }


    #endregion

    #region Helper Methods for Rendering

    
    private void ShowBulkApprovalConfirmation(ReportProcessingSummary report)
    {
        try
        {
            SelectedReportForApproval = report;
            ShowBulkApprovalDialog = true;
            StateHasChanged();

            _logger.LogInformation("Showing bulk approval confirmation for report {ReportId} - hazard {HazardId}",
                report.ReportId, report.HazardId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing bulk approval confirmation for report {ReportId}", report.ReportId);
            ShowErrorAsyncNotification("Error showing approval confirmation dialog");
        }
    }

    private string GetAssessmentIcon(ReportProcessingSummary report)
    {
        if (report.StatusCategory == ProcessingStatusCategory.Validation)
        {
            return "check";
        }
        else if (report.StatusCategory == ProcessingStatusCategory.RiskAssessment)
        {
            return "assessment";
        }
        else if (report.StatusCategory == ProcessingStatusCategory.Investigation)
        {
            return "search";
        }
        else if (report.StatusCategory == ProcessingStatusCategory.Mitigation)
        {
            return "build";
        }
        else if (report.StatusCategory == ProcessingStatusCategory.Closed)
        {
            return "archive";
        }

        // Default icon
        return "description";
    }

    private ButtonStyle GetAssessmentButtonStyle(ReportProcessingSummary report)
    {
        // NEW: Handle default hazard classification first
        if (report.RequiresHazardClassificationUpdate)
        {
            return ButtonStyle.Warning; // Orange for defaults requiring attention
        }

        if (report.StatusCategory == ProcessingStatusCategory.Validation)
        {
            return ButtonStyle.Success;
        }
        else if (report.StatusCategory == ProcessingStatusCategory.RiskAssessment)
        {
            return ButtonStyle.Primary;
        }
        else if (report.StatusCategory == ProcessingStatusCategory.Investigation)
        {
            return ButtonStyle.Warning;
        }
        else if (report.StatusCategory == ProcessingStatusCategory.Mitigation)
        {
            return ButtonStyle.Secondary;
        }
        else if (report.StatusCategory == ProcessingStatusCategory.Closed)
        {
            return ButtonStyle.Base;
        }

        // Default style
        return ButtonStyle.Secondary;
    }

    #endregion

    #region Bulk Approval Methods

    private async Task BulkApproveAllMitigationsForReport(string reportId, string approverCode)
    {
        try
        {
            IsProcessingApproval = true;
            StateHasChanged();

            _logger.LogInformation("Starting bulk approval for ALL mitigations in report: {ReportId}", reportId);

            var hazardsQuery = new GetAllHazardsQuery();
            var hazardsResult = await _mediator.SendAsync(hazardsQuery, CancellationToken.None);

            if (!hazardsResult.IsSuccess || hazardsResult.Value is null)
            {
                ShowErrorAsyncNotification("Failed to load hazard data");
                return;
            }

            var reportHazards = hazardsResult.Value.Where(h => h.ReportCode?.Trim() == reportId?.Trim()).ToList();

            if (!reportHazards.Any())
            {
                ShowErrorAsyncNotification($"No hazards found for report {reportId}");
                return;
            }

            var successCount = 0;
            var errorCount = 0;
            var processedMitigationCodes = new HashSet<string>();

            foreach (var hazard in reportHazards)
            {
                try
                {
                    _logger.LogInformation("Processing mitigations for hazard: {HazardCode}", hazard.Code);

                    var mitigationQuery = new GetMitigationsByHazardCodeQuery(hazard.Code);
                    var mitigationResult = await _mediator.SendAsync(mitigationQuery, CancellationToken.None);

                    if (mitigationResult.IsSuccess && mitigationResult.Value?.Any() == true)
                    {
                        // ? FIXED: Filter for PENDING_APPROVAL using enum value
                        var pendingMitigations = mitigationResult.Value
                            .Where(m => !string.IsNullOrEmpty(m.Code) &&
                                       !processedMitigationCodes.Contains(m.Code) &&
                                       m.Status == MitigationStatus.PendingApproval) 
                            .ToList();

                        _logger.LogInformation("Found {Count} pending mitigations for hazard {HazardCode}: {MitigationCodes}",
                            pendingMitigations.Count, hazard.Code,
                            string.Join(", ", pendingMitigations.Select(m => $"{m.Code}({m.Status})")));

                        // Approve each pending mitigation
                        foreach (var mitigation in pendingMitigations)
                        {
                            try
                            {
                                processedMitigationCodes.Add(mitigation.Code);

                                // ? Update mitigation status using enum value
                                mitigation.Status = MitigationStatus.Approved; 
                                mitigation.UpdatedDate = DateTime.UtcNow;
                                mitigation.UpdatedBy = CurrentUserService?.UserDisplayName;
                                mitigation.ApprovedBy = approverCode;
                                var updateCommand = new UpdateMitigationCommand(mitigation);
                                var updateResult = await _mediator.SendAsync(updateCommand, CancellationToken.None);

                                if (updateResult.IsSuccess)
                                {
                                    successCount++;
                                    _logger.LogInformation("? Approved mitigation: {Code} for hazard {HazardCode}", mitigation.Code, hazard.Code);

                                    // NEW: SPI AUTOMATION - Trigger mitigation completion SPI ??
                                    await TriggerMitigationApprovalSPIAutomation(mitigation, approverCode);
                                }
                                else
                                {
                                    errorCount++;
                                    _logger.LogError("? Failed to approve mitigation {Code}: {Error}", mitigation.Code, updateResult.Error?.Message);
                                }
                            }
                            catch (Exception ex)
                            {
                                errorCount++;
                                _logger.LogError(ex, "Error approving mitigation {Code} for hazard {HazardCode}", mitigation.Code, hazard.Code);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No mitigations found for hazard {HazardCode}", hazard.Code);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing mitigations for hazard {HazardCode}", hazard.Code);
                }
            }

            bool flowControl = await UpdateReportStatus(reportId, ReportStatus.InMitigation);
            if (!flowControl)
            {
                return;
            }

            if (successCount > 0)
            {
                ShowSuccessAsyncNotification($"Successfully approved {successCount} mitigation(s) across {reportHazards.Count} hazard(s) for report {reportId}");
                await LoadDataAsync();
            }
            else if (errorCount == 0)
            {
                ShowInfoAsyncNotification($"No pending mitigations found for report {reportId}");
            }

            if (errorCount > 0)
            {
                ShowErrorAsyncNotification($"Failed to approve {errorCount} mitigation(s). Please check logs for details.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk approval for report {ReportId}", reportId);
            ShowErrorAsyncNotification($"Error during bulk approval for report {reportId}: {ex.Message}");
        }
        finally
        {
            IsProcessingApproval = false;
            StateHasChanged();
        }
    }

        
    private async Task<bool> UpdateReportStatus(string reportId, ReportStatus status)
    {
        
            var cmd = new UpdateReportStatusCommand(reportId, status, CurrentUserService?.UserDisplayName);
            var cmdResult = await _mediator.SendAsync(cmd, CancellationToken.None);
            if (!cmdResult.IsSuccess)
            {
                ShowErrorAsyncNotification($"Report{reportId} Status Was not Updated");
                return false;
            }
            else
            {
                ShowSuccessAsyncNotification($"Report{reportId} Status Was Updated");
                return true;
            }


            
    }

    /// <summary>
    /// ?? KEY METHOD: Get the highest risk level across ALL hazards in the report
    /// This considers Critical > High > Medium > Low priority
    /// </summary>
    private string GetHighestRiskLevelAcrossAllHazards(ReportProcessingSummary report)
    {
        if (!report.HasMitigations || !report.AllMitigations.Any())
            return RiskLevel.Low.Name; // ? Use enum instead of "Low"

        var riskLevels = report.AllMitigations
            .Select(m => m.HazardRiskLevel?.Name ?? RiskLevel.Low.Name) // ? Use enum instead of "Low"
            .Distinct()
            .ToList();

        // ?? CRITICAL BUSINESS LOGIC: Prioritize risk levels using enum values
        if (riskLevels.Any(r => r.Equals(RiskLevel.Critical.Name, StringComparison.OrdinalIgnoreCase))) return RiskLevel.Critical.Name;
        if (riskLevels.Any(r => r.Equals(RiskLevel.High.Name, StringComparison.OrdinalIgnoreCase))) return RiskLevel.High.Name;
        if (riskLevels.Any(r => r.Equals(RiskLevel.Medium.Name, StringComparison.OrdinalIgnoreCase))) return RiskLevel.Medium.Name;
        //if (riskLevels.Any(r => r.Equals(RiskLevel.Low.Name, StringComparison.OrdinalIgnoreCase))) return RiskLevel.Low.Name;

        return RiskLevel.Low.Name; // ? Use enum instead of "Low"
    }

    /// <summary>
    /// ?? BUSINESS RULES: Determine if a user can approve mitigations for a given risk level
    /// Uses SMSOrganizationalLevel enum and integer AuthorityLevel with proper enum lookups
    /// </summary>
    private bool CanApproveRiskLevel(SMSOrganizationalUser user, string riskLevel)
    {
        // Get the risk level enum to check required authority level
        var riskLevelEnum = RiskLevel.GetAllValues()
            .FirstOrDefault(rl => rl.Name.Equals(riskLevel, StringComparison.OrdinalIgnoreCase));

        if (riskLevelEnum is null)
            return false;

        // Primary check: User's integer AuthorityLevel
        if (user.AuthorityLevel.HasValue && user.AuthorityLevel.Value >= riskLevelEnum.RequiredAuthorityLevel)
        {
            return true;
        }

        // Secondary check: User's OrganizationLevel enum authority
        if (user.OrganizationLevel?.AuthorityLevel >= riskLevelEnum.RequiredAuthorityLevel)
        {
            return true;
        }

        // Tertiary check: Specific role-based approval using RiskLevel's ApproverRoles
        if (user.OrganizationLevel is not null && riskLevelEnum.ApproverRoles.Contains(user.OrganizationLevel.Value))
        {
            return true;
        }

        // Fallback check: String-based RiskApprovalAuthority (for legacy compatibility)
        var userRiskAuthority = user.RiskApprovalAuthority?.ToUpper();
        if (!string.IsNullOrEmpty(userRiskAuthority))
        {
            // Check if user's risk approval authority includes this risk level
            return userRiskAuthority.Contains(riskLevelEnum.Value.ToUpper());
        }

        return false;
    }
    /// <summary>
    /// Get Radzen BadgeStyle for risk level (presentation layer extension)
    /// </summary>
    private BadgeStyle GetRiskLevelBadgeStyle(RiskLevel? riskLevel)
    {
        if (riskLevel is null)
            return BadgeStyle.Secondary;

        return riskLevel.BootstrapClass switch
        {
            "danger" => BadgeStyle.Danger,
            "warning" => BadgeStyle.Warning,
            "success" => BadgeStyle.Success,
            "secondary" => BadgeStyle.Secondary,
            _ => BadgeStyle.Secondary
        };
    }
    

    /// <summary>
    /// Get Radzen BadgeStyle for risk level by name (presentation layer logic)
    /// </summary>
    private BadgeStyle GetRiskLevelBadgeStyle(string? riskLevelName)
    {
        if (string.IsNullOrEmpty(riskLevelName))
            return BadgeStyle.Secondary;

        var riskLevel = RiskLevel.GetAllValues()
            .FirstOrDefault(rl => rl.Name.Equals(riskLevelName, StringComparison.OrdinalIgnoreCase));

        return GetRiskLevelBadgeStyle(riskLevel);
    }

    /// <summary>
    /// Helper method for risk level priority ordering using enum values
    /// </summary>
    private int GetRiskPriority(string riskLevel)
    {
        return riskLevel?.ToUpper() switch
        {
            _ when riskLevel.Equals(RiskLevel.Critical.Name, StringComparison.OrdinalIgnoreCase) => 4,
            _ when riskLevel.Equals(RiskLevel.High.Name, StringComparison.OrdinalIgnoreCase) => 3,
            _ when riskLevel.Equals(RiskLevel.Medium.Name, StringComparison.OrdinalIgnoreCase) => 2,
            _ when riskLevel.Equals(RiskLevel.Low.Name, StringComparison.OrdinalIgnoreCase) => 1,
            _ => 0
        };
    }

    /// <summary>
    /// ?? ENHANCED: Updated bulk approval logic with proper enum-based approver validation
    /// </summary>
    private async Task ProcessBulkApprovalConfirmation()
    {
        if (SelectedReportForApproval is null)
        {
            ShowErrorAsyncNotification("No report selected for approval.");
            return;
        }

        if (string.IsNullOrEmpty(SelectedApprover))
        {
            ShowErrorAsyncNotification("Please select an authorized approver before proceeding.");
            return;
        }

        // ?? CRITICAL: Verify approver has authority for the highest risk level using enum
        var highestRiskLevel = GetHighestRiskLevelAcrossAllHazards(SelectedReportForApproval);
        var approver = AvailableApprovers.FirstOrDefault(a => a.Code == SelectedApprover);

        if (approver is null || !CanApproveRiskLevel(approver, highestRiskLevel))
        {
            var riskLevelDisplay = highestRiskLevel switch
            {
                _ when highestRiskLevel == RiskLevel.Critical.Name => RiskLevel.Critical.Name,
                _ when highestRiskLevel == RiskLevel.High.Name => RiskLevel.High.Name,
                _ when highestRiskLevel == RiskLevel.Medium.Name => RiskLevel.Medium.Name,
                _ when highestRiskLevel == RiskLevel.Low.Name => RiskLevel.Low.Name,
                _ => "Unknown"
            };

            ShowErrorAsyncNotification($"Selected approver does not have sufficient authority to approve {riskLevelDisplay} risk level mitigations.");
            return;
        }

        _logger.LogInformation("Bulk approval authorized: {ApproverName} ({ApproverCode}) approving {RiskLevel} risk mitigations for report {ReportId}",
            $"{approver.FirstName?.Value} {approver.LastName?.Value}", SelectedApprover, highestRiskLevel, SelectedReportForApproval.ReportId);

        await BulkApproveAllMitigationsForReport(SelectedReportForApproval.ReportId, approver.DisplayName); //SelectedApprover);
        await CloseBulkApprovalConfirmation();
    }

    /// <summary>
    /// ?? ENHANCED: Updated bulk approval logic with approver validation
    /// </summary>
    private async Task CloseBulkApprovalConfirmation()
    {
        ShowBulkApprovalDialog = false;
        SelectedReportForApproval = null;
        SelectedApprover = null;
        StateHasChanged();
    }

    #endregion

    #region Description Modal Methods

    /// <summary>
    /// Show description modal with full hazard description
    /// </summary>
    private async Task ShowDescriptionDialog(ReportProcessingSummary report)
    {
        try
        {
            SelectedDescription = report.HazardDescription ?? "No description available";
            SelectedReportId = report.ReportId ?? "Unknown";
            ShowDescriptionModal = true;
            StateHasChanged();

            _logger.LogInformation("Showing description modal for report {ReportId}", report.ReportId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing description modal for report {ReportId}", report.ReportId);
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Error showing description details"));
        }
    }

    /// <summary>
    /// Close description modal
    /// </summary>
    private void CloseDescriptionModal()
    {
        ShowDescriptionModal = false;
        SelectedDescription = string.Empty;
        SelectedReportId = string.Empty;
        StateHasChanged();
    }

    #endregion

    #region SPI Automation Integration

    /// <summary>
    /// Triggers SPI automation when mitigations are approved
    /// Updates Mitigation Implementation Rate SPI based on completion timing
    /// </summary>
    private async Task TriggerMitigationApprovalSPIAutomation(Mitigation mitigation, string approverCode)
    {
        try
        {
            _logger.LogInformation("?? SPI Automation: Triggering mitigation approval events for {MitigationCode}", mitigation.Code);

            // Determine target completion date and actual completion date  
            var targetDate = mitigation.TargetDate ?? DateTime.UtcNow.AddDays(30); // Default 30 days if no target
            var completedDate = DateTime.UtcNow; // Approval date = completion date
            var completedBy = approverCode ?? CurrentUserService?.UserDisplayName ?? "Unknown";

            // Trigger Mitigation Implementation Rate SPI
            await _spiCoordinator.OnMitigationCompleted(
                mitigationId: mitigation.Code,
                mitigationCode: mitigation.Code,
                hazardId: mitigation.HazardCode ?? "",
                targetCompletionDate: targetDate,
                completedDate: completedDate,
                completedBy: completedBy,
                reportId: "", // Could be enhanced to include report ID
                completionNotes: $"Bulk approved by {completedBy}",
                effectivenessRating: "Approved");

            _logger.LogInformation("? SPI Automation: Successfully processed mitigation approval events for {MitigationCode}", mitigation.Code);
        }
        catch (Exception spiEx)
        {
            // Don't fail the mitigation approval if SPI automation fails
            _logger.LogWarning(spiEx, "?? SPI Automation: Failed to process mitigation approval events for {MitigationCode} - continuing with approval", mitigation.Code);
        }
    }

    #endregion

    public class ApproverOption
    {
        public string Code { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string AuthorityLevel { get; set; } = string.Empty;
        public string RiskApprovalAuthority { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;

        public static ApproverOption FromUser(SMSOrganizationalUser user)
        {
            return new ApproverOption
            {
                Code = user.Code,
                DisplayName = $"{user.FirstName?.Value} {user.LastName?.Value} ({user.OrganizationLevel.Value})",
                AuthorityLevel = user.AuthorityLevel?.ToString() ?? "Not specified",
                RiskApprovalAuthority = user.RiskApprovalAuthority ?? "Not specified",
                Department = user.Department?.Name ?? "Not specified",
                Position = user.Position ?? "Not specified"
            };
        }
    }

    public class RiskLevelBreakdown
    {
        public string RiskLevel { get; set; } = string.Empty;
        public int HazardCount { get; set; }
        public int MitigationCount { get; set; }
    }
}