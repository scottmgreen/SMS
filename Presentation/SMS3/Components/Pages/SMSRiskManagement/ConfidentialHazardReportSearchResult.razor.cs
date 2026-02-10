using Microsoft.AspNetCore.Components.Web;

namespace SMS3.Components.Pages.SMSRiskManagement;

/// <summary>
/// Anonymous/Confidential Hazard Report Search Result Details page
/// Shows detailed status information for a specific tracking ID without requiring login
/// Designed for users who submitted confidential reports to check status anonymously
/// </summary>
public partial class ConfidentialHazardReportSearchResult : ComponentBase
{
    #region Dependencies
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<ConfidentialHazardReportSearchResult> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    #endregion

    #region Parameters
    /// <summary>
    /// Tracking code from the route
    /// </summary>
    [Parameter] public string TrackingCode { get; set; } = string.Empty;
    #endregion

    #region State Properties
    /// <summary>
    /// Loading state indicator
    /// </summary>
    public bool IsLoading { get; set; } = true;

    /// <summary>
    /// Report details for display
    /// </summary>
    public HazardReportSearchResult? ReportDetails { get; set; }

    /// <summary>
    /// Hazard location information
    /// </summary>
    public HazardLocation? HazardLocation { get; set; }
    #endregion

    #region UI Properties
    /// <summary>
    /// Page title for header component
    /// </summary>
    public string PageTitle => "Confidential Report Status";

    /// <summary>
    /// Page subtitle for header component
    /// </summary>
    public string PageSubtitle => $"Status information for tracking ID: {TrackingCode}";

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
        builder.AddAttribute(6, "Size", ButtonSize.Small);
        builder.CloseComponent();
    };
    #endregion

    #region Lifecycle Methods

    /// <summary>
    /// Initialize component and load report details
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        Logger.LogInformation("Anonymous report details view initialized for tracking code: {TrackingCode}", TrackingCode);
        await LoadReportDetails();
    }

    #endregion

    #region Data Loading Methods

    /// <summary>
    /// Load report details for the tracking code (anonymous version with filtered information)
    /// </summary>
    private async Task LoadReportDetails()
    {
        if (string.IsNullOrWhiteSpace(TrackingCode))
        {
            Logger.LogWarning("No tracking code provided for anonymous report details");
            return;
        }

        try
        {
            IsLoading = true;
            StateHasChanged();

            Logger.LogInformation("Loading anonymous report details for tracking code: {TrackingCode}", TrackingCode);

            // Get tracking information
            var trackingQuery = new GetHazardReportTrackingByTrackingCodeQuery(TrackingCode);
            var trackingResult = await Mediator.SendAsync(trackingQuery, CancellationToken.None);

            if (trackingResult.IsSuccess && trackingResult.Value != null)
            {
                ReportDetails = await BuildAnonymousReportDetails(trackingResult.Value);

                if (ReportDetails != null)
                {
                    Logger.LogInformation("Successfully loaded anonymous report details for tracking code: {TrackingCode}", TrackingCode);
                }
                else
                {
                    Logger.LogWarning("Could not build report details for tracking code: {TrackingCode}", TrackingCode);
                }
            }
            else
            {
                Logger.LogInformation("No report found for anonymous tracking code: {TrackingCode}", TrackingCode);
                ReportDetails = null;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading anonymous report details for tracking code: {TrackingCode}", TrackingCode);
            ReportDetails = null;
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Build anonymous report details with filtered/anonymized information
    /// </summary>
    /// <param name="tracking">Tracking information from database</param>
    /// <returns>Anonymized report details</returns>
    private async Task<HazardReportSearchResult?> BuildAnonymousReportDetails(HazardReportTrackingDetails tracking)
    {
        try
        {
            var reportDetails = new HazardReportSearchResult
            {
                TrackingCode = tracking.TrackingCode,
                HazardCode = tracking.HazardCode,
                ReportCode = tracking.ReportCode,
                CreatedDate = tracking.CreatedDate,
                // Anonymous-specific defaults
                SubmittedBy = "Anonymous Reporter",
                CurrentStatus = "Under Review",
                HazardType = "Confidential Safety Report",
                Description = "Report details are confidential and available to authorized safety personnel only."
            };

            // Get basic hazard information (filtered for anonymous access)
            if (!string.IsNullOrEmpty(tracking.HazardCode))
            {
                try
                {
                    var hazardQuery = new GetHazardByCodeQuery(new HazardID(tracking.HazardCode));
                    var hazardResult = await Mediator.SendAsync(hazardQuery, CancellationToken.None);

                    if (hazardResult.IsSuccess && hazardResult.Value != null)
                    {
                        var hazard = hazardResult.Value;

                        // Only show non-sensitive information
                        

                        // Add location information for fallback display
                        reportDetails.LocationArea = hazard.LocationArea ?? "";
                        reportDetails.LocationSubArea = hazard.LocationSubArea ?? "";

                        // Show generic hazard type rather than specific details
                        if (!string.IsNullOrEmpty(hazard.HazardType))
                        {
                            reportDetails.HazardType = GetGenericHazardType(hazard.HazardType);
                        }

                        // Set appropriate status based on hazard info
                        if (!string.IsNullOrEmpty(hazard.Status))
                        {
                            reportDetails.CurrentStatus = GetAnonymousStatus(hazard.Status);
                        }

                        // Load location information for map display
                        await LoadLocationInformation(tracking.HazardCode);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Could not load hazard details for anonymous view, code: {HazardCode}", tracking.HazardCode);
                }
            }

            // Get validation information (anonymized)
            if (!string.IsNullOrEmpty(tracking.ReportCode))
            {
                try
                {
                    var validationQuery = new GetReportValidationByReportIdQuery(new ReportID(tracking.ReportCode));
                    var validationResult = await Mediator.SendAsync(validationQuery, CancellationToken.None);

                    if (validationResult.IsSuccess && validationResult.Value != null)
                    {
                        reportDetails.ValidationDecision = validationResult.Value.ValidationDecision ?? "";
                        reportDetails.ValidationDate = validationResult.Value.ValidatedDate;
                        reportDetails.ValidatedBy = "SMS Safety Team"; // Anonymized
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Could not load validation details for anonymous view, report: {ReportCode}", tracking.ReportCode);
                }
            }

            return reportDetails;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error building anonymous report details for tracking: {TrackingCode}", tracking.TrackingCode);
            return null;
        }
    }

    /// <summary>
    /// Load location information for map display
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
                Logger.LogInformation("Loaded location information for confidential hazard: {HazardCode}", hazardCode);
            }
            else
            {
                Logger.LogInformation("No location information found for confidential hazard: {HazardCode}", hazardCode);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error loading location information for confidential hazard: {HazardCode}", hazardCode);
        }
    }

    #endregion

    #region UI Helper Methods

    /// <summary>
    /// Get generic hazard type for anonymous display
    /// </summary>
    /// <param name="specificType">Specific hazard type</param>
    /// <returns>Generic type for anonymous display</returns>
    private string GetGenericHazardType(string specificType)
    {
        return specificType?.ToUpper() switch
        {
            "RWY_INCURSION" => "Runway Safety Report",
            "ACFT_DAMAGE" => "Aircraft Safety Report",
            "GROUND_VEHICLE" => "Ground Operations Report",
            "WILDLIFE_STRIKE" => "Wildlife Safety Report",
            "FOD" => "Foreign Object Report",
            "EQUIPMENT_FAIL" => "Equipment Safety Report",
            "PERSONNEL_INJURY" => "Personnel Safety Report",
            _ => "Safety Report"
        };
    }

    /// <summary>
    /// Get anonymized status for public display
    /// </summary>
    /// <param name="internalStatus">Internal status from system</param>
    /// <returns>Anonymized status for public display</returns>
    private string GetAnonymousStatus(string internalStatus)
    {
        return internalStatus?.ToLower() switch
        {
            "active" or "open" or "initial" => "Under Review",
            "processing" or "investigation" => "Being Evaluated",
            "completed" or "closed" => "Review Complete",
            "cancelled" or "invalid" => "Closed",
            _ => "Under Review"
        };
    }

    /// <summary>
    /// Get badge style for report status
    /// </summary>
    /// <param name="status">Report status</param>
    /// <returns>Badge style</returns>
    public BadgeStyle GetStatusBadgeStyle(string status)
    {
        return status?.ToLower() switch
        {
            "review complete" or "closed" => BadgeStyle.Success,
            "being evaluated" or "under review" => BadgeStyle.Info,
            "submitted" => BadgeStyle.Warning,
            _ => BadgeStyle.Secondary
        };
    }

    /// <summary>
    /// Get badge style for validation decision
    /// </summary>
    /// <param name="decision">Validation decision</param>
    /// <returns>Badge style</returns>
    public BadgeStyle GetValidationBadgeStyle(string decision)
    {
        return decision?.ToUpper() switch
        {
            "SMS_RISK" => BadgeStyle.Success,
            "NOT_SMS_RISK" => BadgeStyle.Info,
            "NEEDS_INVESTIGATION" => BadgeStyle.Warning,
            _ => BadgeStyle.Secondary
        };
    }

    /// <summary>
    /// Get display text for validation decision
    /// </summary>
    /// <param name="decision">Validation decision value</param>
    /// <returns>Display text</returns>
    public string GetValidationDecisionDisplay(string decision)
    {
        return decision?.ToUpper() switch
        {
            "SMS_RISK" => "SMS Risk Identified",
            "NOT_SMS_RISK" => "No SMS Risk",
            "NEEDS_INVESTIGATION" => "Under Investigation",
            _ => "Under Review"
        };
    }

    /// <summary>
    /// Check if a processing step is completed based on current status and validation
    /// </summary>
    /// <param name="step">Step to check (InitialReview, RiskAssessment, FinalProcessing)</param>
    /// <returns>True if step is completed</returns>
    public bool IsStepCompleted(string step)
    {
        if (ReportDetails == null) return false;

        return step switch
        {
            "InitialReview" => !string.IsNullOrEmpty(ReportDetails.CurrentStatus) &&
                              ReportDetails.CurrentStatus != "Under Review",
            "RiskAssessment" => !string.IsNullOrEmpty(ReportDetails.ValidationDecision),
            "FinalProcessing" => ReportDetails.CurrentStatus?.ToLower() == "review complete",
            _ => false
        };
    }

    #endregion

    #region Navigation Methods

    /// <summary>
    /// Navigate back to search page
    /// </summary>
    public void BackToSearch()
    {
        Navigation.NavigateTo("/ConfidentialReporting/TrackStatus");
    }

    /// <summary>
    /// Navigate to confidential reporting page
    /// </summary>
    public void NavigateToReporting()
    {
        Navigation.NavigateTo("/ConfidentialReporting");
    }

    #endregion

    #region Models

    /// <summary>
    /// Search result model for display (same as search page)
    /// </summary>
    public class HazardReportSearchResult
    {
        public string TrackingCode { get; set; } = string.Empty;
        public string HazardCode { get; set; } = string.Empty;
        public string ReportCode { get; set; } = string.Empty;
        public string HazardType { get; set; } = string.Empty;
        public string SubmittedBy { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public string ValidationDecision { get; set; } = string.Empty;
        public DateTime? ValidationDate { get; set; }
        public string ValidatedBy { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsAnonymous { get; set; }
        public DateTime CreatedDate { get; set; }
        
        // Location fallback properties for text-based location info
        public string LocationArea { get; set; } = string.Empty;
        public string LocationSubArea { get; set; } = string.Empty;
    }

    #endregion
}