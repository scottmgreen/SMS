using Microsoft.AspNetCore.Components.Web;

namespace SMS3.Components.Pages.SMSRiskManagement;

/// <summary>
/// Confidential/Anonymous Hazard Report Search page - allows users to search for reports using tracking ID without logging in
/// This is specifically designed for users who submitted confidential reports and want to check status anonymously
/// </summary>
public partial class ConfidentialHazardReportSearch : ComponentBase
{
    #region Dependencies
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<ConfidentialHazardReportSearch> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    #endregion

    #region Search Properties
    /// <summary>
    /// Primary tracking ID search field
    /// </summary>
    public string TrackingIdSearch { get; set; } = string.Empty;

    /// <summary>
    /// Format validation message for tracking ID
    /// </summary>
    public string TrackingIdFormatMessage { get; set; } = string.Empty;

    /// <summary>
    /// Color for tracking ID format message
    /// </summary>
    public string TrackingIdFormatColor { get; set; } = "#6c757d";

    /// <summary>
    /// Icon for tracking ID format message
    /// </summary>
    public string TrackingIdFormatIcon { get; set; } = "fa-info-circle";

    /// <summary>
    /// Indicates if we found similar results instead of exact match
    /// </summary>
    public bool HasSimilarResults { get; set; }

    /// <summary>
    /// List of tracking IDs with their similarity scores for highlighting best matches
    /// </summary>
    public Dictionary<string, int> SimilarityScores { get; set; } = new();
    #endregion

    #region State Properties
    /// <summary>
    /// Loading state indicator
    /// </summary>
    public bool IsSearching { get; set; }

    /// <summary>
    /// Indicates if a search has been performed
    /// </summary>
    public bool HasSearched { get; set; }

    /// <summary>
    /// Last search query performed (for display)
    /// </summary>
    public string LastSearchQuery { get; set; } = string.Empty;

    /// <summary>
    /// Search results collection
    /// </summary>
    public List<HazardReportSearchResult> SearchResults { get; set; } = new();

    /// <summary>
    /// Reference to the search results grid
    /// </summary>
    public RadzenDataGrid<HazardReportSearchResult>? SearchResultsGrid { get; set; }
    #endregion

    #region UI Properties
    /// <summary>
    /// Page title for header component
    /// </summary>
    public string PageTitle => "Track Your Confidential Report";

    /// <summary>
    /// Page subtitle for header component
    /// </summary>
    public string PageSubtitle => "Check the status of your anonymously submitted hazard report using your tracking ID";

    /// <summary>
    /// Additional header content with anonymous badge
    /// </summary>
    public RenderFragment AdditionalHeaderContent => builder =>
    {
        builder.OpenElement(0, "small");
        builder.AddAttribute(1, "class", "text-white-50");
        
        builder.OpenElement(2, "i");
        builder.AddAttribute(3, "class", "fas fa-shield-alt me-1");
        builder.CloseElement();
        
        builder.AddContent(4, "Anonymous & Secure");
        builder.CloseElement();
    };
    #endregion

    #region Event Handlers

    /// <summary>
    /// Handle Enter key press in tracking ID search
    /// </summary>
    /// <param name="args">Keyboard event args</param>
    public async Task OnTrackingIdKeyPress(KeyboardEventArgs args)
    {
        if (args.Key == "Enter" && !string.IsNullOrWhiteSpace(TrackingIdSearch))
        {
            await SearchByTrackingId();
        }
    }

    /// <summary>
    /// Handle input changes for tracking ID with real-time validation
    /// </summary>
    /// <param name="args">Change event args</param>
    public void OnTrackingIdInput(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;
        ValidateTrackingIdFormat(value);
        StateHasChanged();
    }

    /// <summary>
    /// Validate tracking ID format and provide feedback
    /// </summary>
    /// <param name="trackingId">Input tracking ID</param>
    private void ValidateTrackingIdFormat(string trackingId)
    {
        if (string.IsNullOrWhiteSpace(trackingId))
        {
            TrackingIdFormatMessage = string.Empty;
            return;
        }

        var cleaned = trackingId.Trim().ToUpper();

        // Check for exact HT-YYYY-NNNN format
        if (global::System.Text.RegularExpressions.Regex.IsMatch(cleaned, @"^HT-\d{4}-\d{4}$"))
        {
            TrackingIdFormatMessage = "Valid tracking ID format";
            TrackingIdFormatColor = "#28a745";
            TrackingIdFormatIcon = "fa-check-circle";
        }
        // Check for partial format (just numbers)
        else if (cleaned.All(char.IsDigit) && cleaned.Length >= 2 && cleaned.Length <= 8)
        {
            var currentYear = DateTime.Now.Year;
            var suggestion = cleaned.Length <= 4
                ? $"HT-{currentYear}-{cleaned.PadLeft(4, '0')}"
                : $"HT-{currentYear}-{cleaned}";

            TrackingIdFormatMessage = $"Will search for format: {suggestion}";
            TrackingIdFormatColor = "#ffc107";
            TrackingIdFormatIcon = "fa-exclamation-triangle";
        }
        // Check for partial HT- format
        else if (cleaned.StartsWith("HT-"))
        {
            TrackingIdFormatMessage = "Continue typing or search anyway";
            TrackingIdFormatColor = "#17a2b8";
            TrackingIdFormatIcon = "fa-info-circle";
        }
        else
        {
            TrackingIdFormatMessage = "Expected format: HT-YYYY-NNNN (e.g., HT-2026-0006)";
            TrackingIdFormatColor = "#dc3545";
            TrackingIdFormatIcon = "fa-times-circle";
        }
    }

    #endregion

    #region Search Methods

    /// <summary>
    /// Search by tracking ID (primary search method) with enhanced error handling and suggestions
    /// </summary>
    public async Task SearchByTrackingId()
    {
        if (string.IsNullOrWhiteSpace(TrackingIdSearch))
        {
            ShowWarningNotification("Please enter a tracking ID to search");
            return;
        }

        try
        {
            IsSearching = true;
            HasSearched = false;
            SearchResults.Clear();
            LastSearchQuery = TrackingIdSearch.Trim();
            StateHasChanged();

            Logger.LogInformation("Anonymous search by tracking ID: {TrackingId}", TrackingIdSearch);

            // Clean and validate the tracking ID format
            var cleanedTrackingId = CleanTrackingId(TrackingIdSearch.Trim());

            // First try exact match
            var query = new GetHazardReportTrackingByTrackingCodeQuery(cleanedTrackingId);
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            HasSimilarResults = false;
            SimilarityScores.Clear();

            if (result.IsSuccess && result.Value != null)
            {
                // Get detailed information for the found tracking record - but filter for confidential only
                var searchResult = await BuildSearchResultFromTracking(result.Value, isAnonymousSearch: true);
                if (searchResult != null)
                {
                    SearchResults.Add(searchResult);
                    SimilarityScores[searchResult.TrackingCode] = 100; // Exact match
                }
            }
            else
            {
                // If no exact match found, try to find similar tracking IDs
                HasSimilarResults = true;
                await SearchForSimilarTrackingIds(cleanedTrackingId);
            }

            HasSearched = true;

            if (!SearchResults.Any())
            {
                await ShowNoResultsFoundMessage(cleanedTrackingId);
                Logger.LogInformation("No anonymous results found for tracking ID: {TrackingId}", TrackingIdSearch);
            }
            else
            {
                var message = SearchResults.Count == 1
                    ? $"Found your confidential report for tracking ID: {TrackingIdSearch}"
                    : $"Found {SearchResults.Count} similar tracking IDs for: {TrackingIdSearch}";
                ShowSuccessNotification(message);
                Logger.LogInformation("Found {Count} anonymous result(s) for tracking ID: {TrackingId}", SearchResults.Count, TrackingIdSearch);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in anonymous search by tracking ID: {TrackingId}", TrackingIdSearch);
            ShowErrorNotification("Error occurred while searching. Please try again.");
        }
        finally
        {
            IsSearching = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Clean and standardize tracking ID format
    /// </summary>
    /// <param name="trackingId">Input tracking ID</param>
    /// <returns>Cleaned tracking ID</returns>
    private string CleanTrackingId(string trackingId)
    {
        if (string.IsNullOrWhiteSpace(trackingId)) return string.Empty;

        // Remove extra spaces and convert to uppercase
        trackingId = trackingId.Trim().ToUpper();

        // Ensure it starts with HT- if it's just numbers
        if (trackingId.All(char.IsDigit) && trackingId.Length >= 4)
        {
            // If it's just numbers, try to format it as a tracking ID
            // Assume current year if year not provided
            var currentYear = DateTime.Now.Year;
            trackingId = $"HT-{currentYear}-{trackingId.PadLeft(4, '0')}";
        }

        return trackingId;
    }

    /// <summary>
    /// Search for tracking IDs that are similar to the input (anonymous version)
    /// </summary>
    /// <param name="trackingId">Input tracking ID</param>
    private async Task SearchForSimilarTrackingIds(string trackingId)
    {
        try
        {
            // Get all tracking records and find similar ones
            var allTrackingQuery = new GetAllHazardReportTrackingQuery();
            var allResult = await Mediator.SendAsync(allTrackingQuery, CancellationToken.None);

            if (allResult.IsSuccess && allResult.Value?.Any() == true)
            {
                var similarTrackingIds = FindSimilarTrackingIds(trackingId, allResult.Value);

                foreach (var tracking in similarTrackingIds.Take(5)) // Limit to top 5 matches
                {
                    var trackingDetails = CreateTrackingDetails(tracking);
                    var searchResult = await BuildSearchResultFromTracking(trackingDetails, isAnonymousSearch: true);
                    if (searchResult != null)
                    {
                        SearchResults.Add(searchResult);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error searching for similar tracking IDs in anonymous search for: {TrackingId}", trackingId);
        }
    }

    /// <summary>
    /// Find tracking IDs similar to the search term
    /// </summary>
    /// <param name="searchId">Search tracking ID</param>
    /// <param name="allTracking">All tracking records</param>
    /// <returns>List of similar tracking records</returns>
    private List<HazardReportTracking> FindSimilarTrackingIds(string searchId, List<HazardReportTracking> allTracking)
    {
        var similarIds = new List<(HazardReportTracking tracking, int score)>();

        foreach (var tracking in allTracking)
        {
            var score = CalculateSimilarityScore(searchId, tracking.TrackingCode);
            if (score > 0)
            {
                similarIds.Add((tracking, score));
                // Store the score for UI highlighting
                SimilarityScores[tracking.TrackingCode] = score;
            }
        }

        // Return sorted by similarity score (highest first)
        return similarIds
            .OrderByDescending(x => x.score)
            .Select(x => x.tracking)
            .ToList();
    }

    /// <summary>
    /// Calculate similarity score between two tracking IDs
    /// </summary>
    /// <param name="searchId">Search tracking ID</param>
    /// <param name="trackingId">Database tracking ID</param>
    /// <returns>Similarity score (higher is more similar)</returns>
    private int CalculateSimilarityScore(string searchId, string trackingId)
    {
        if (string.IsNullOrEmpty(searchId) || string.IsNullOrEmpty(trackingId))
            return 0;

        var search = searchId.ToUpper();
        var target = trackingId.ToUpper();

        var score = 0;

        // Exact match
        if (search == target) return 100;

        // Partial match
        if (target.Contains(search) || search.Contains(target)) score += 50;

        // Extract numeric parts and compare
        var searchNumbers = ExtractNumbers(search);
        var targetNumbers = ExtractNumbers(target);

        foreach (var num in searchNumbers)
        {
            if (targetNumbers.Contains(num)) score += 10;
        }

        // Year pattern matching (common typo: 2006 vs 2026)
        if (search.Contains("2006") && target.Contains("2026") ||
            search.Contains("2026") && target.Contains("2006"))
        {
            score += 30; // High score for year typos
        }

        // Sequential number matching (e.g., 0066 vs 0006)
        var searchSeq = ExtractSequentialNumbers(search);
        var targetSeq = ExtractSequentialNumbers(target);

        if (searchSeq.Any() && targetSeq.Any())
        {
            var seqMatch = searchSeq.Intersect(targetSeq).Count();
            score += seqMatch * 5;
        }

        return score;
    }

    /// <summary>
    /// Extract numbers from a string
    /// </summary>
    private List<string> ExtractNumbers(string input)
    {
        var numbers = new List<string>();
        var current = "";

        foreach (char c in input)
        {
            if (char.IsDigit(c))
            {
                current += c;
            }
            else
            {
                if (!string.IsNullOrEmpty(current))
                {
                    numbers.Add(current);
                    current = "";
                }
            }
        }

        if (!string.IsNullOrEmpty(current))
        {
            numbers.Add(current);
        }

        return numbers;
    }

    /// <summary>
    /// Extract sequential numbers (4+ digits) from a string
    /// </summary>
    private List<string> ExtractSequentialNumbers(string input)
    {
        return ExtractNumbers(input).Where(n => n.Length >= 4).ToList();
    }

    /// <summary>
    /// Show detailed no results found message with suggestions (anonymous version)
    /// </summary>
    private async Task ShowNoResultsFoundMessage(string trackingId)
    {
        var message = $"No confidential report found for tracking ID: {trackingId}";
        var suggestions = GenerateTrackingIdSuggestions(trackingId);

        if (suggestions.Any())
        {
            message += "\n\nDid you mean:\n" + string.Join("\n", suggestions.Take(3));
        }
        else
        {
            message += "\n\nPlease check:\n" +
                      "• Tracking ID format (HT-YYYY-NNNN)\n" +
                      "• Correct year (current year: " + DateTime.Now.Year + ")\n" +
                      "• Call our confidential hotline: (503) 555-0199";
        }

        ShowInfoNotification(message);
    }

    /// <summary>
    /// Generate tracking ID format suggestions
    /// </summary>
    private List<string> GenerateTrackingIdSuggestions(string trackingId)
    {
        var suggestions = new List<string>();

        // Extract numbers from the input
        var numbers = ExtractNumbers(trackingId);

        if (numbers.Any())
        {
            var currentYear = DateTime.Now.Year;
            var lastYear = currentYear - 1;

            // Generate suggestions with current and previous year
            foreach (var num in numbers.Take(2))
            {
                if (num.Length >= 4)
                {
                    suggestions.Add($"HT-{currentYear}-{num}");
                    suggestions.Add($"HT-{lastYear}-{num}");
                }
                else if (num.Length >= 2)
                {
                    var paddedNum = num.PadLeft(4, '0');
                    suggestions.Add($"HT-{currentYear}-{paddedNum}");
                    suggestions.Add($"HT-{lastYear}-{paddedNum}");
                }
            }
        }

        return suggestions.Distinct().ToList();
    }

    /// <summary>
    /// Helper method to create HazardReportTrackingDetails from HazardReportTracking
    /// </summary>
    private HazardReportTrackingDetails CreateTrackingDetails(HazardReportTracking tracking)
    {
        return new HazardReportTrackingDetails
        {
            HazardReportTracking = tracking,
            CurrentStatus = "Under Review",
            ProcessingStage = "SMS Evaluation",
            LastUpdated = tracking.UpdatedDate ?? tracking.CreatedDate,
            ProcessingNotes = new List<string>()
        };
    }

    #endregion

    #region Navigation Methods

    /// <summary>
    /// View detailed results for a tracking ID (anonymous version)
    /// </summary>
    /// <param name="trackingCode">Tracking code to view details for</param>
    public void ViewDetails(string trackingCode)
    {
        Logger.LogInformation("Navigating to anonymous details view for tracking code: {TrackingCode}", trackingCode);
        Navigation.NavigateTo($"/ConfidentialReporting/TrackStatus/{trackingCode}");
    }

    /// <summary>
    /// Navigate to confidential reporting page
    /// </summary>
    public void NavigateToReporting()
    {
        Navigation.NavigateTo("/ConfidentialReporting");
    }

    #endregion

    #region UI Helper Methods

    /// <summary>
    /// Get badge style for report status
    /// </summary>
    /// <param name="status">Report status</param>
    /// <returns>Badge style</returns>
    public BadgeStyle GetStatusBadgeStyle(string status)
    {
        return status?.ToLower() switch
        {
            "completed" or "closed" => BadgeStyle.Success,
            "under review" or "processing" => BadgeStyle.Info,
            "initial" or "submitted" => BadgeStyle.Warning,
            "cancelled" => BadgeStyle.Danger,
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
            "NOT_SMS_RISK" => BadgeStyle.Danger,
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
            "SMS_RISK" => "SMS Risk",
            "NOT_SMS_RISK" => "Not SMS Risk",
            "NEEDS_INVESTIGATION" => "Under Investigation",
            _ => decision ?? "Under Review"
        };
    }

    /// <summary>
    /// Check if a tracking code is a high similarity match
    /// </summary>
    /// <param name="trackingCode">Tracking code to check</param>
    /// <returns>True if it's a high match (similarity score > 50)</returns>
    public bool IsHighMatch(string trackingCode)
    {
        return SimilarityScores.TryGetValue(trackingCode, out var score) && score > 50;
    }

    #endregion

    #region Notification Methods

    /// <summary>
    /// Show a success notification
    /// </summary>
    /// <param name="message">Message to display</param>
    private void ShowSuccessNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Success",
            Detail = message,
            Duration = 4000
        });
    }

    /// <summary>
    /// Show an error notification
    /// </summary>
    /// <param name="message">Message to display</param>
    private void ShowErrorNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Error",
            Detail = message,
            Duration = 5000
        });
    }

    /// <summary>
    /// Show a warning notification
    /// </summary>
    /// <param name="message">Message to display</param>
    private void ShowWarningNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Warning,
            Summary = "Warning",
            Detail = message,
            Duration = 5000
        });
    }

    /// <summary>
    /// Show an info notification
    /// </summary>
    /// <param name="message">Message to display</param>
    private void ShowInfoNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Info,
            Summary = "Information",
            Detail = message,
            Duration = 4000
        });
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Build a search result object from the tracking query result (anonymous version)
    /// Filters out sensitive information for anonymous access
    /// </summary>
    /// <param name="tracking">Tracking query result</param>
    /// <param name="isAnonymousSearch">Flag indicating this is an anonymous search</param>
    /// <returns>Search result object</returns>
    private async Task<HazardReportSearchResult?> BuildSearchResultFromTracking(HazardReportTrackingDetails tracking, bool isAnonymousSearch = false)
    {
        try
        {
            var searchResult = new HazardReportSearchResult
            {
                TrackingCode = tracking.TrackingCode,
                HazardCode = tracking.HazardCode,
                ReportCode = tracking.ReportCode,
                CreatedDate = tracking.CreatedDate,
                // Anonymous-specific defaults
                ReportedBy = "Anonymous", // Never show actual reporter name
                CurrentStatus = "Under Review", // Generic status for anonymous
                HazardType = "Confidential Report" // Generic type
            };

            // Get basic hazard details if available (but filter sensitive info for anonymous)
            if (!string.IsNullOrEmpty(tracking.HazardCode))
            {
                try
                {
                    var hazardQuery = new GetHazardByCodeQuery(tracking.HazardCode);
                    var hazardResult = await Mediator.SendAsync(hazardQuery, CancellationToken.None);

                    if (hazardResult.IsSuccess && hazardResult.Value != null)
                    {
                        var hazard = hazardResult.Value;

                        // Only show non-sensitive information for anonymous users
                        searchResult.ReportedOn = hazard.ReportedOn != DateTime.MinValue ? hazard.ReportedOn : DateTime.MinValue;
                        searchResult.IsConfidential = hazard.IsConfidential;

                        // Show generic hazard type rather than specific details
                        if (!string.IsNullOrEmpty(hazard.HazardType))
                        {
                            searchResult.HazardType = GetGenericHazardType(hazard.HazardType);
                        }

                        // Don't show actual status, description, or reporter info for anonymous access
                        searchResult.Description = "Details available to authorized personnel only";
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Could not load hazard details for anonymous search, code: {HazardCode}", tracking.HazardCode);
                }
            }

            // Get basic validation info if available (but anonymized)
            if (!string.IsNullOrEmpty(tracking.ReportCode))
            {
                try
                {
                    var validationQuery = new GetReportValidationByReportIdQuery(new ReportID(tracking.ReportCode));
                    var validationResult = await Mediator.SendAsync(validationQuery, CancellationToken.None);

                    if (validationResult.IsSuccess && validationResult.Value != null)
                    {
                        searchResult.ValidationDecision = validationResult.Value.ValidationDecision ?? "";
                        searchResult.ValidationDate = validationResult.Value.ValidatedDate;
                        searchResult.ValidatedBy = "SMS Team"; // Anonymous - don't show actual validator
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Could not load validation details for anonymous search, report: {ReportCode}", tracking.ReportCode);
                }
            }

            return searchResult;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error building anonymous search result for tracking: {TrackingCode}", tracking.TrackingCode);
            return null;
        }
    }

    /// <summary>
    /// Get generic hazard type for anonymous display
    /// </summary>
    /// <param name="specificType">Specific hazard type</param>
    /// <returns>Generic type for anonymous display</returns>
    private string GetGenericHazardType(string specificType)
    {
        return specificType?.ToUpper() switch
        {
            "RWY_INCURSION" => "Runway Safety",
            "ACFT_DAMAGE" => "Aircraft Incident",
            "GROUND_VEHICLE" => "Ground Operations",
            "WILDLIFE_STRIKE" => "Wildlife Hazard",
            "FOD" => "Foreign Object",
            "EQUIPMENT_FAIL" => "Equipment Issue",
            "PERSONNEL_INJURY" => "Personnel Safety",
            _ => "Safety Report"
        };
    }

    #endregion

    #region Models

    /// <summary>
    /// Search result model for display in grid (same as authenticated version)
    /// </summary>
    public class HazardReportSearchResult
    {
        public string TrackingCode { get; set; } = string.Empty;
        public string HazardCode { get; set; } = string.Empty;
        public string ReportCode { get; set; } = string.Empty;
        public string HazardType { get; set; } = string.Empty;
        public string ReportedBy { get; set; } = string.Empty;
        public DateTime ReportedOn { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public string ValidationDecision { get; set; } = string.Empty;
        public DateTime? ValidationDate { get; set; }
        public string ValidatedBy { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsConfidential { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    #endregion
}