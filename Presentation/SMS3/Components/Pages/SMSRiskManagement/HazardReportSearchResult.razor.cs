using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SMS_Application.Messaging.Queries;
using SMS_Application.Interfaces;
using Microsoft.Extensions.Logging;
using Radzen;
using Radzen.Blazor;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Domain.Enums;

namespace SMS3.Components.Pages.SMSRiskManagement;

/// <summary>
/// Hazard Report Search Result page - shows detailed information for a specific tracking ID
/// </summary>
public partial class HazardReportSearchResult : ComponentBase
{
    #region Parameters
    /// <summary>
    /// Tracking code from route parameter
    /// </summary>
    [Parameter] public string TrackingCode { get; set; } = string.Empty;
    #endregion

    #region Dependencies
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<HazardReportSearchResult> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;

    [Inject] private ICurrentUserService CurrentUserService { get; set; } = default!;
    #endregion

    #region State Properties
    /// <summary>
    /// Loading state indicator
    /// </summary>
    public bool IsLoading { get; set; }

    /// <summary>
    /// Indicates if a search has been performed
    /// </summary>
    public bool HasSearched { get; set; }

    /// <summary>
    /// Main report details model
    /// </summary>
    public HazardReportDetails? ReportDetails { get; set; }

    /// <summary>
    /// Report validation information
    /// </summary>
    public SMS_Domain.Entities.ReportValidation? ReportValidation { get; set; }

    /// <summary>
    /// Hazard location information
    /// </summary>
    public HazardLocation? HazardLocation { get; set; }

    /// <summary>
    /// Attached files list
    /// </summary>
    public List<HazardFile> AttachedFiles { get; set; } = new();

    /// <summary>
    /// Current risk assessment information
    /// </summary>
    public RiskAssessment? CurrentRiskAssessment { get; set; }

    /// <summary>
    /// Current mitigation information
    /// </summary>
    public Mitigation? CurrentMitigation { get; set; }
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        if (!string.IsNullOrWhiteSpace(TrackingCode))
        {
            await LoadReportDetails();
        }
    }
    #endregion

    #region Data Loading Methods

    /// <summary>
    /// Load comprehensive report details for the tracking code
    /// </summary>
    private async Task LoadReportDetails()
    {
        try
        {
            IsLoading = true;
            HasSearched = false;
            StateHasChanged();

            Logger.LogInformation("Loading detailed report information for tracking code: {TrackingCode}", TrackingCode);

            // Step 1: Get tracking record
            var trackingQuery = new GetHazardReportTrackingByTrackingCodeQuery(TrackingCode);
            var trackingResult = await Mediator.SendAsync(trackingQuery, CancellationToken.None);

            if (trackingResult.IsFailure || trackingResult.Value == null)
            {
                Logger.LogWarning("No tracking record found for: {TrackingCode}", TrackingCode);
                HasSearched = true;
                return;
            }

            var tracking = trackingResult.Value;

            // Initialize report details
            ReportDetails = new HazardReportDetails
            {
                TrackingCode = tracking.TrackingCode,
                HazardCode = tracking.HazardCode,
                ReportCode = tracking.ReportCode,
                CreatedDate = tracking.CreatedDate
            };

            // Step 2: Load hazard information
            await LoadHazardInformation(tracking.HazardCode);

            // Step 3: Load report information
            await LoadReportInformation(tracking.ReportCode);

            // Step 4: Load validation information
            await LoadValidationInformation(tracking.ReportCode);

            // Step 5: Load risk assessment information if SMS_RISK validation
            if (ReportValidation?.ValidationDecision == "SMS_RISK")
            {
                await LoadRiskAssessmentInformation(tracking.HazardCode);
            }
            await LoadMitigationInformation(tracking.HazardCode);

            // Step 6: Load location information
            await LoadLocationInformation(tracking.HazardCode);

            // Step 7: Load attached files
            await LoadAttachedFiles(tracking.HazardCode);

            HasSearched = true;

            Logger.LogInformation("Successfully loaded all details for tracking code: {TrackingCode}", TrackingCode);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading report details for tracking code: {TrackingCode}", TrackingCode);
            ShowErrorNotification("Error loading report details. Please try again.");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Load hazard-specific information
    /// </summary>
    private async Task LoadHazardInformation(string hazardCode)
    {
        if (string.IsNullOrEmpty(hazardCode) || ReportDetails == null) return;

        try
        {
            var hazardQuery = new GetHazardByCodeQuery(new HazardID(hazardCode));
            var hazardResult = await Mediator.SendAsync(hazardQuery, CancellationToken.None);

            if (hazardResult.IsSuccess && hazardResult.Value != null)
            {
                var hazard = hazardResult.Value;

                ReportDetails.HazardType = hazard.HazardType ?? "Unknown";
                ReportDetails.HazardCategory = hazard.HazardCategory ?? "Unknown";
                ReportDetails.Description = hazard.Description ?? "";
                //ReportDetails.SubmittedBy = hazard.SubmittedBy ?? "Unknown";
                //ReportDetails.SubmittedDate = hazard.SubmittedDate;
                //ReportDetails.Department = hazard.ReportingDepartment ?? "";
                ReportDetails.CurrentStatus = hazard.Status ?? "Unknown";
                //ReportDetails.IsAnonymous = hazard.IsAnonymous;
                
                // Add location information for fallback display
                ReportDetails.LocationArea = hazard.LocationArea ?? "";
                ReportDetails.LocationSubArea = hazard.LocationSubArea ?? "";

                Logger.LogInformation("Loaded hazard information for code: {HazardCode}", hazardCode);
            }
            else
            {
                Logger.LogWarning("Could not load hazard information for code: {HazardCode}", hazardCode);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error loading hazard information for code: {HazardCode}", hazardCode);
        }
    }

    /// <summary>
    /// Load report-specific information
    /// </summary>
    private async Task LoadReportInformation(string reportCode)
    {
        if (string.IsNullOrEmpty(reportCode) || ReportDetails == null) return;

        try
        {
            var reportQuery = new GetReportByCodeQuery(new ReportID(reportCode));
            var reportResult = await Mediator.SendAsync(reportQuery, CancellationToken.None);

            if (reportResult.IsSuccess && reportResult.Value != null)
            {
                var report = reportResult.Value;

                ReportDetails.IsAnonymous = report.IsAnonymous;
                // Fill in any missing information from report if not already set by hazard
                if (string.IsNullOrEmpty(ReportDetails.SubmittedBy))
                {
                    ReportDetails.SubmittedBy = report.SubmittedBy ?? "Unknown";
                }
                if (!ReportDetails.SubmittedDate.HasValue)
                {
                    ReportDetails.SubmittedDate = report.SubmittedDate;
                }
                if (string.IsNullOrEmpty(ReportDetails.SubmittingDepartment))
                {
                    ReportDetails.SubmittingDepartment = report.SubmittingDepartment ?? "";
                }
                if (string.IsNullOrEmpty(ReportDetails.JobFunction))
                {
                    ReportDetails.JobFunction = report.SubmittingDepartmentJobFunction ?? "";
                }
                if (string.IsNullOrEmpty(ReportDetails.CurrentStatus))
                {
                    ReportDetails.CurrentStatus = report.Status ?? "Unknown";
                }
                if (string.IsNullOrEmpty(ReportDetails.Description))
                {
                    ReportDetails.Description = report.Description ?? "";
                }
                if (string.IsNullOrEmpty(ReportDetails.ContactName))
                {
                    ReportDetails.ContactName = report.ReportContactName ?? "";
                }
                if (string.IsNullOrEmpty(ReportDetails.ContactCell))
                {
                    ReportDetails.ContactCell = report.ReportContactCell ?? "";
                }
                if (string.IsNullOrEmpty(ReportDetails.ContactEmail))
                {
                    ReportDetails.ContactEmail = report.ReportContactEmail ?? "";
                }


                Logger.LogInformation("Loaded report information for code: {ReportCode}", reportCode);
            }
            else
            {
                Logger.LogWarning("Could not load report information for code: {ReportCode}", reportCode);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error loading report information for code: {ReportCode}", reportCode);
        }
    }

    /// <summary>
    /// Load validation information
    /// </summary>
    private async Task LoadValidationInformation(string reportCode)
    {
        if (string.IsNullOrEmpty(reportCode)) return;

        try
        {
            var validationQuery = new GetReportValidationByReportIdQuery(new ReportID(reportCode));
            var validationResult = await Mediator.SendAsync(validationQuery, CancellationToken.None);

            if (validationResult.IsSuccess && validationResult.Value != null)
            {
                ReportValidation = validationResult.Value;
                Logger.LogInformation("Loaded validation information for report: {ReportCode}", reportCode);
            }
            else
            {
                Logger.LogInformation("No validation information found for report: {ReportCode}", reportCode);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error loading validation information for report: {ReportCode}", reportCode);
        }
    }

    /// <summary>
    /// Load risk assessment information based on hazard code
    /// </summary>
    private async Task LoadRiskAssessmentInformation(string hazardCode)
    {
        if (string.IsNullOrEmpty(hazardCode)) return;

        try
        {
            var riskAssessmentQuery = new GetRiskAssessmentsByHazardCodeQuery(new HazardID(hazardCode));
            var riskAssessmentResult = await Mediator.SendAsync(riskAssessmentQuery, CancellationToken.None);

            if (riskAssessmentResult.IsSuccess && riskAssessmentResult.Value?.Any() == true)
            {
                var assessments = riskAssessmentResult.Value.ToList();
                
                // Find the Technical assessment first, fallback to any assessment
                CurrentRiskAssessment = assessments.FirstOrDefault(ra => ra.RiskAssessmentCategory == RiskAssessmentCategory.Technical) 
                                     ?? assessments.FirstOrDefault();

                Logger.LogInformation("Loaded risk assessment information for hazard: {HazardCode}, Found {Count} assessments", 
                    hazardCode, assessments.Count);
            }
            else
            {
                Logger.LogInformation("No risk assessment information found for hazard: {HazardCode}", hazardCode);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error loading risk assessment information for hazard: {HazardCode}", hazardCode);
        }
    }


    /// <summary>
    /// Load mitigation information based on hazard code
    /// </summary>
    private async Task LoadMitigationInformation(string hazardCode)
    {
        if (string.IsNullOrEmpty(hazardCode)) return;

        try
        {
            var mitigationQuery = new GetMitigationsByHazardCodeQuery(hazardCode);
            var mitigationResult = await Mediator.SendAsync(mitigationQuery, CancellationToken.None);

            if (mitigationResult.IsSuccess && mitigationResult.Value?.Any() == true)
            {
                var mitigations = mitigationResult.Value.ToList();

                // Find the Technical assessment first, fallback to any assessment
                CurrentMitigation = mitigations.FirstOrDefault() ?? mitigations.FirstOrDefault();

                Logger.LogInformation("Loaded mitigation information for hazard: {HazardCode}, Found {Count} mitigations",hazardCode, mitigations.Count);
            }
            else
            {
                Logger.LogInformation("No mitigation information information found for hazard: {HazardCode}", hazardCode);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error loading mitigation information information for hazard: {HazardCode}", hazardCode);
        }
    }
    /// <summary>
    /// Load location information
    /// </summary>
    private async Task LoadLocationInformation(string hazardCode)
    {
        if (string.IsNullOrEmpty(hazardCode)) return;

        try
        {
            var locationQuery = new GetHazardLocationsByHazardCodeQuery(hazardCode);
            var locationResult = await Mediator.SendAsync(locationQuery, CancellationToken.None);

            if (locationResult.IsSuccess && locationResult.Value?.Any() == true)
            {
                HazardLocation = locationResult.Value.FirstOrDefault();
                Logger.LogInformation("Loaded location information for hazard: {HazardCode}", hazardCode);
            }
            else
            {
                Logger.LogInformation("No location information found for hazard: {HazardCode}", hazardCode);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error loading location information for hazard: {HazardCode}", hazardCode);
        }
    }

    /// <summary>
    /// Load attached files
    /// </summary>
    private async Task LoadAttachedFiles(string hazardCode)
    {
        if (string.IsNullOrEmpty(hazardCode)) return;

        try
        {
            var filesQuery = new GetHazardFilesByHazardCodeQuery(hazardCode);
            var filesResult = await Mediator.SendAsync(filesQuery, CancellationToken.None);

            if (filesResult.IsSuccess && filesResult.Value?.Any() == true)
            {
                AttachedFiles = filesResult.Value.ToList();
                Logger.LogInformation("Loaded {Count} attached files for hazard: {HazardCode}", AttachedFiles.Count, hazardCode);
            }
            else
            {
                Logger.LogInformation("No attached files found for hazard: {HazardCode}", hazardCode);
                AttachedFiles = new List<HazardFile>();
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error loading attached files for hazard: {HazardCode}", hazardCode);
            AttachedFiles = new List<HazardFile>();
        }
    }

    #endregion

    #region UI Properties
    /// <summary>
    /// Page title for header component
    /// </summary>
    public string PageTitle => "Hazard Report Details";

    /// <summary>
    /// Page subtitle for header component
    /// </summary>
    public string PageSubtitle => $"Comprehensive information for tracking ID: {TrackingCode}";

    /// <summary>
    /// Additional header content with back button
    /// </summary>
    public RenderFragment AdditionalHeaderContent => builder =>
    {
        builder.OpenComponent<RadzenButton>(0);
        builder.AddAttribute(1, "ButtonType", ButtonType.Button);
        builder.AddAttribute(2, "Click", EventCallback.Factory.Create<MouseEventArgs>(this, BackToSearch));
        builder.AddAttribute(3, "Text", "Back to Search");
        builder.AddAttribute(4, "Icon", "arrow_back");
        builder.AddAttribute(5, "ButtonStyle", ButtonStyle.Light);
        builder.CloseComponent();
    };
    #endregion

    #region UI Helper Methods

    /// <summary>
    /// Get badge style for report status
    /// </summary>
    public BadgeStyle GetStatusBadgeStyle(string status)
    {
        return status?.ToLower() switch
        {
            "completed" or "closed" => BadgeStyle.Success,
            "in_progress" or "processing" => BadgeStyle.Success,
            "initial" or "draft" => BadgeStyle.Success,
            "cancelled" => BadgeStyle.Danger,
            _ => BadgeStyle.Secondary
        };
    }

    /// <summary>
    /// Get badge style for validation decision
    /// </summary>
    public BadgeStyle GetValidationBadgeStyle(string? decision)
    {
        return decision?.ToUpper() switch
        {
            "SMS_RISK" => BadgeStyle.Success,
            "NOT_SMS_RISK" => BadgeStyle.Danger,
            "NEEDS_INVESTIGATION" => BadgeStyle.Warning,
            _ => BadgeStyle.Secondary
        };
    }

    /// <summary>
    /// Get display text for validation decision
    /// </summary>
    public string GetValidationDecisionDisplay(string? decision)
    {
        return decision?.ToUpper() switch
        {
            "SMS_RISK" => "SMS Risk",
            "NOT_SMS_RISK" => "Not SMS Risk",
            "NEEDS_INVESTIGATION" => "Needs Investigation",
            _ => decision ?? "Unknown"
        };
    }

    /// <summary>
    /// Get processing status class for timeline
    /// </summary>
    public string GetProcessingStatusClass()
    {
        if (ReportValidation?.ValidationDecision == null) return "pending";

        return ReportValidation.ValidationDecision.ToUpper() switch
        {
            "SMS_RISK" => "in-progress",
            "NEEDS_INVESTIGATION" => "in-progress",
            "NOT_SMS_RISK" => "completed",
            _ => "pending"
        };
    }

    /// <summary>
    /// Get processing icon for timeline
    /// </summary>
    public string GetProcessingIcon()
    {
        if (ReportValidation?.ValidationDecision == null) return "schedule";

        return ReportValidation.ValidationDecision.ToUpper() switch
        {
            "SMS_RISK" => "assessment",
            "NEEDS_INVESTIGATION" => "search",
            "NOT_SMS_RISK" => "check_circle",
            _ => "schedule"
        };
    }

    /// <summary>
    /// Get processing stage title for timeline
    /// </summary>
    public string GetProcessingStageTitle()
    {
        if (ReportValidation?.ValidationDecision == null) return "Processing";

        return ReportValidation.ValidationDecision.ToUpper() switch
        {
            "SMS_RISK" => "Risk Assessment",
            "NEEDS_INVESTIGATION" => "Investigation",
            "NOT_SMS_RISK" => "Report Closed",
            _ => "Processing"
        };
    }

    /// <summary>
    /// Get processing status description for timeline
    /// </summary>
    

    /// <summary>
    /// Get processing status text CSS class
    /// </summary>
    public string GetProcessingStatusTextClass()
    {
        return GetProcessingStatusClass() switch
        {
            "completed" => "text-success",
            "in-progress" => "text-info",
            "pending" => "text-muted",
            _ => "text-muted"
        };
    }

    /// <summary>
    /// Get simple processing status description based on validation decision
    /// </summary>
    public string GetSimpleProcessingDescription()
    {
        if (ReportValidation?.ValidationDecision == null) 
            return "Processing status will be updated as the report progresses";

        return ReportValidation.ValidationDecision.ToUpper() switch
        {
            "SMS_RISK" => "Report identified as SMS Risk - proceeding to risk assessment",
            "NEEDS_INVESTIGATION" => "Report requires investigation - SMS investigators are reviewing", 
            "NOT_SMS_RISK" => "Report determined not to be SMS Risk - closed",
            _ => "Processing status will be updated as the report progresses"
        };
    }

    /// <summary>
    /// Get file icon based on file type
    /// </summary>
    public string GetFileIcon(string? fileType)
    {
        if (string.IsNullOrEmpty(fileType)) return "insert_drive_file";

        return fileType.ToLower() switch
        {
            "pdf" => "picture_as_pdf",
            "doc" or "docx" => "description",
            "xls" or "xlsx" => "grid_on",
            "ppt" or "pptx" => "slideshow",
            "jpg" or "jpeg" or "png" or "gif" or "bmp" => "image",
            "mp4" or "avi" or "mov" or "wmv" => "movie",
            "txt" => "text_snippet",
            "zip" or "rar" or "7z" => "folder_zip",
            _ => "insert_drive_file"
        };
    }

    /// <summary>
    /// Format file size for display
    /// </summary>
    public string FormatFileSize(long bytes)
    {
        const int scale = 1024;
        string[] orders = { "GB", "MB", "KB", "Bytes" };
        long max = (long)Math.Pow(scale, orders.Length - 1);

        foreach (string order in orders)
        {
            if (bytes > max)
                return $"{decimal.Divide(bytes, max):##.##} {order}";
            max /= scale;
        }
        return "0 Bytes";
    }

    #endregion

    #region Navigation Methods

    /// <summary>
    /// Navigate back to search page
    /// </summary>
    public void BackToSearch()
    {
        Navigation.NavigateTo("/SMSRiskManagement/HazardReportSearch");
    }

    #endregion

    #region Notification Methods

    /// <summary>
    /// Show error notification
    /// </summary>
    private void ShowErrorNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Error",
            Detail = message,
            Duration = 6000
        });
    }

    #endregion

    #region Models

    /// <summary>
    /// Comprehensive report details model for display
    /// </summary>
    public class HazardReportDetails
    {
        public string TrackingCode { get; set; } = string.Empty;
        public string HazardCode { get; set; } = string.Empty;
        public string ReportCode { get; set; } = string.Empty;
        public string HazardType { get; set; } = string.Empty;
        public string HazardCategory { get; set; } = string.Empty;
        public string SubmittedBy { get; set; } = string.Empty;
        public DateTime? SubmittedDate { get; set; }
        public string SubmittingDepartment { get; set; } = string.Empty;

        public string JobFunction { get; set; } = string.Empty;

        public string ContactName { get; set; } = string.Empty;

        public string ContactCell { get; set; } = string.Empty;

        public string ContactEmail { get; set; } = string.Empty;



        public string CurrentStatus { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsAnonymous { get; set; }
        public DateTime CreatedDate { get; set; }

        // Location fallback properties for text-based location info
        public string LocationArea { get; set; } = string.Empty;
        public string LocationSubArea { get; set; } = string.Empty;
    }

    #endregion
}