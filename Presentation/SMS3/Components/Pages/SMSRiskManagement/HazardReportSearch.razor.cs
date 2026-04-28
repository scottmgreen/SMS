using Microsoft.AspNetCore.Components.Web;

using SMS_Shared.Configuration;

using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration.Extensions;

namespace SMS3.Components.Pages.SMSRiskManagement;

/// <summary>
/// Hazard Report Search page - allows users to search for reports using tracking ID or advanced criteria
/// </summary>
public partial class HazardReportSearch : ComponentBase
{
    #region Dependencies
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<HazardReportSearch> _logger { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    
    [Inject] private INotificationHelper  _notificationHelper { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;

    [Inject] private ICurrentUserService CurrentUserService { get; set; } = default!;
    
    /// <summary>
    /// Optional tracking ID parameter from URL for direct search
    /// </summary>
    [Parameter] public string? trackingId { get; set; }
    #endregion

    #region Search Properties
    /// <summary>
    /// Primary tracking ID search field
    /// </summary>
    public string TrackingIdSearch { get; set; } = string.Empty;

    /// <summary>
    /// Advanced search: Report Code
    /// </summary>
    public string ReportCodeSearch { get; set; } = string.Empty;

    /// <summary>
    /// Advanced search: Hazard Code
    /// </summary>
    public string HazardCodeSearch { get; set; } = string.Empty;

    /// <summary>
    /// Advanced search: Reported By
    /// </summary>
    public string SubmittedBySearch { get; set; } = string.Empty;

    /// <summary>
    /// Advanced search: Date from
    /// </summary>
    public DateTime? DateFromSearch { get; set; }

    /// <summary>
    /// Advanced search: Date to
    /// </summary>
    public DateTime? DateToSearch { get; set; }

    /// <summary>
    /// Show/hide advanced search section
    /// </summary>
    public bool ShowAdvancedSearch { get; set; }

    /// <summary>
    /// Check if advanced search has any criteria
    /// </summary>
    public bool HasAdvancedSearchCriteria =>
        !string.IsNullOrWhiteSpace(ReportCodeSearch) ||
        !string.IsNullOrWhiteSpace(HazardCodeSearch) ||
        !string.IsNullOrWhiteSpace(SubmittedBySearch) ||
        DateFromSearch.HasValue ||
        DateToSearch.HasValue;
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

    #region UI Properties
    /// <summary>
    /// Page title for header component
    /// </summary>
    public string PageTitle => "Hazard Report Search";

    /// <summary>
    /// Page subtitle for header component
    /// </summary>
    public string PageSubtitle => "Search for hazard reports using tracking ID or report details";
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
    /// Toggle advanced search visibility
    /// </summary>
    public void ToggleAdvancedSearch()
    {
        ShowAdvancedSearch = !ShowAdvancedSearch;
        StateHasChanged();
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
            TrackingIdFormatMessage = "? Valid tracking ID format";
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

            TrackingIdFormatMessage = $"? Will search for format: {suggestion}";
            TrackingIdFormatColor = "#ffc107";
            TrackingIdFormatIcon = "fa-exclamation-triangle";
        }
        // Check for partial HT- format
        else if (cleaned.StartsWith("HT-"))
        {
            TrackingIdFormatMessage = "? Incomplete format - continue typing or search anyway";
            TrackingIdFormatColor = "#17a2b8";
            TrackingIdFormatIcon = "fa-info-circle";
        }
        else
        {
            TrackingIdFormatMessage = "? Expected format: HT-YYYY-NNNN (e.g., HT-2026-0006)";
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
            await _notificationHelper.ShowWarningAsync("Please enter a tracking ID to search");
            return;
        }

        try
        {
            IsSearching = true;
            HasSearched = false;
            SearchResults.Clear();
            LastSearchQuery = TrackingIdSearch.Trim();
            StateHasChanged();

            _logger.LogInformation("Searching by tracking ID: {TrackingId}", TrackingIdSearch);

            // Clean and validate the tracking ID format
            var cleanedTrackingId = CleanTrackingId(TrackingIdSearch.Trim());

            // First try exact match
            var query = new GetHazardReportTrackingByTrackingCodeQuery(cleanedTrackingId);
            var result = await _mediator.SendAsync(query, CancellationToken.None);

            HasSimilarResults = false;
            SimilarityScores.Clear();

            if (result.IsSuccess && result.Value is not null)
            {
                // Get detailed information for the found tracking record
                var searchResult = await BuildSearchResultFromTracking(result.Value);
                if (searchResult is not null)
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
                _logger.LogInformation("No results found for tracking ID: {TrackingId}", TrackingIdSearch);
            }
            else
            {
                var message = SearchResults.Count == 1
                    ? $"Found hazard report for tracking ID: {TrackingIdSearch}"
                    : $"Found {SearchResults.Count} similar tracking IDs for: {TrackingIdSearch}";
                await _notificationHelper.ShowSuccessAsync(message);
                _logger.LogInformation("Found {Count} result(s) for tracking ID: {TrackingId}", SearchResults.Count, TrackingIdSearch);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching by tracking ID: {TrackingId}", TrackingIdSearch);
            await _notificationHelper.ShowErrorAsync("Error occurred while searching. Please try again.");
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
    /// Search for tracking IDs that are similar to the input
    /// </summary>
    /// <param name="trackingId">Input tracking ID</param>
    private async Task SearchForSimilarTrackingIds(string trackingId)
    {
        try
        {
            // Get all tracking records and find similar ones
            var allTrackingQuery = new GetAllHazardReportTrackingQuery();
            var allResult = await _mediator.SendAsync(allTrackingQuery, CancellationToken.None);

            if (allResult.IsSuccess && allResult.Value?.Any() == true)
            {
                var similarTrackingIds = FindSimilarTrackingIds(trackingId, allResult.Value);

                foreach (var tracking in similarTrackingIds.Take(5)) // Limit to top 5 matches
                {
                    var trackingDetails = CreateTrackingDetails(tracking);
                    var searchResult = await BuildSearchResultFromTracking(trackingDetails);
                    if (searchResult is not null)
                    {
                        SearchResults.Add(searchResult);
                    }
                }

                // Indicate that similar results were found
                HasSimilarResults = true;

                // Highlight best matches
                HighlightBestMatches(similarTrackingIds);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error searching for similar tracking IDs for: {TrackingId}", trackingId);
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
    /// Show detailed no results found message with suggestions
    /// </summary>
    private async Task ShowNoResultsFoundMessage(string trackingId)
    {
        var message = $"No hazard report found for tracking ID: {trackingId}";
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
                      "• Contact support if you need assistance";
        }

        await _notificationHelper.ShowInfoAsync(message);
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
    /// Advanced search using multiple criteria
    /// </summary>
    public async Task SearchAdvanced()
    {
        if (!HasAdvancedSearchCriteria)
        {
            await _notificationHelper.ShowWarningAsync("Please enter at least one search criteria");
            return;
        }

        try
        {
            IsSearching = true;
            HasSearched = false;
            SearchResults.Clear();
            LastSearchQuery = "Advanced search criteria";
            StateHasChanged();

            _logger.LogInformation("Performing advanced search with criteria: ReportCode={ReportCode}, HazardCode={HazardCode}, SubmittedBy={SubmittedBy}",
                ReportCodeSearch, HazardCodeSearch, SubmittedBySearch);

            // Step 1: Search by report code if provided
            if (!string.IsNullOrWhiteSpace(ReportCodeSearch))
            {
                await SearchByReportCode(ReportCodeSearch.Trim());
            }

            // Step 2: Search by hazard code if provided
            if (!string.IsNullOrWhiteSpace(HazardCodeSearch))
            {
                await SearchByHazardCode(HazardCodeSearch.Trim());
            }

            // Step 3: If no specific codes provided, search all tracking records and filter
            if (string.IsNullOrWhiteSpace(ReportCodeSearch) && string.IsNullOrWhiteSpace(HazardCodeSearch))
            {
                await SearchAllWithFilters();
            }

            HasSearched = true;

            if (SearchResults?.Any() == true)
            {
                await _notificationHelper.ShowInfoAsync("No hazard reports found matching your search criteria");
            }
            else
            {
                await _notificationHelper.ShowSuccessAsync($"Found {SearchResults.Count} hazard report(s) matching your criteria");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in advanced search");
            await _notificationHelper.ShowErrorAsync("Error occurred while searching. Please try again.");
        }
        finally
        {
            IsSearching = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Search by report code
    /// </summary>
    private async Task SearchByReportCode(string reportCode)
    {
        var query = new GetHazardReportTrackingByReportCodeQuery(reportCode);
        var result = await _mediator.SendAsync(query, CancellationToken.None);

        if (result.IsSuccess && result.Value?.Any() == true)
        {
            foreach (var tracking in result.Value)
            {
                var trackingDetails = CreateTrackingDetails(tracking);
                var searchResult = await BuildSearchResultFromTracking(trackingDetails);
                if (searchResult is not null && MatchesAdvancedFilters(searchResult))
                {
                    // Avoid duplicates
                    if (!SearchResults.Any(sr => sr.TrackingCode == searchResult.TrackingCode))
                    {
                        SearchResults.Add(searchResult);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Search by hazard code
    /// </summary>
    private async Task SearchByHazardCode(string hazardCode)
    {
        var query = new GetHazardReportTrackingByHazardCodeQuery(hazardCode);
        var result = await _mediator.SendAsync(query, CancellationToken.None);

        if (result.IsSuccess && result.Value?.Any() == true)
        {
            foreach (var tracking in result.Value)
            {
                var trackingDetails = CreateTrackingDetails(tracking);
                var searchResult = await BuildSearchResultFromTracking(trackingDetails);
                if (searchResult is not null && MatchesAdvancedFilters(searchResult))
                {
                    // Avoid duplicates
                    if (!SearchResults.Any(sr => sr.TrackingCode == searchResult.TrackingCode))
                    {
                        SearchResults.Add(searchResult);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Search all tracking records and apply filters
    /// </summary>
    private async Task SearchAllWithFilters()
    {
        var query = new GetAllHazardReportTrackingQuery();
        var result = await _mediator.SendAsync(query, CancellationToken.None);

        if (result.IsSuccess && result.Value?.Any() == true)
        {
            foreach (var tracking in result.Value)
            {
                var trackingDetails = CreateTrackingDetails(tracking);
                var searchResult = await BuildSearchResultFromTracking(trackingDetails);
                if (searchResult is not null && MatchesAdvancedFilters(searchResult))
                {
                    SearchResults.Add(searchResult);
                }
            }
        }
    }

    /// <summary>
    /// Helper method to create HazardReportTrackingDetails from HazardReportTracking
    /// </summary>
    private HazardReportTrackingDetails CreateTrackingDetails(HazardReportTracking tracking)
    {
        return new HazardReportTrackingDetails
        {
            HazardReportTracking = tracking,
            CurrentStatus = "Processing",
            ProcessingStage = "Analysis",
            LastUpdated = tracking.UpdatedDate ?? tracking.CreatedDate,
            ProcessingNotes = new List<string>()
        };
    }

    #endregion

    #region Navigation Methods

    /// <summary>
    /// View detailed results for a tracking ID
    /// </summary>
    /// <param name="trackingCode">Tracking code to view details for</param>
    public void ViewDetails(string trackingCode)
    {
        _logger.LogInformation("Navigating to details view for tracking code: {TrackingCode}", trackingCode);
        _navigation.NavigateToSecure($"/SMSRiskManagement/HazardReportSearchResult/{trackingCode}");
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
            "in_progress" or "processing" => BadgeStyle.Info,
            "initial" or "draft" => BadgeStyle.Warning,
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
            "NEEDS_INVESTIGATION" => "Needs Investigation",
            _ => decision ?? "Unknown"
        };
    }

    /// <summary>
    /// Clear advanced search criteria
    /// </summary>
    public void ClearAdvancedSearch()
    {
        ReportCodeSearch = string.Empty;
        HazardCodeSearch = string.Empty;
        SubmittedBySearch = string.Empty;
        DateFromSearch = null;
        DateToSearch = null;
        StateHasChanged();
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

    #region Helper Methods

    /// <summary>
    /// Build a search result object from the tracking query result
    /// </summary>
    /// <param name="trackingResult">Tracking query result</param>
    /// <returns>Search result object</returns>
    private async Task<HazardReportSearchResult?> BuildSearchResultFromTracking(HazardReportTrackingDetails tracking)
    {
        try
        {
            var searchResult = new HazardReportSearchResult
            {
                TrackingCode = tracking.TrackingCode,
                HazardCode = tracking.HazardCode,
                ReportCode = tracking.ReportCode,
                CreatedDate = tracking.CreatedDate
            };

            // Get hazard details if available
            if (!string.IsNullOrEmpty(tracking.HazardCode))
            {
                try
                {
                    var hazardQuery = new GetHazardByCodeQuery(new HazardID(tracking.HazardCode));
                    var hazardResult = await _mediator.SendAsync(hazardQuery, CancellationToken.None);

                    if (hazardResult.IsSuccess && hazardResult.Value is not null)
                    {
                        var hazard = hazardResult.Value;
                        searchResult.HazardType = hazard.HazardType ?? "Unknown";
                        searchResult.HazardCategory = hazard.HazardCategory ?? "Unknown";
                        //searchResult.SubmittedBy = hazard.SubmittedBy ?? "Unknown";
                        //searchResult.SubmittedDate = hazard.SubmittedDate != DateTime.MinValue ? hazard.SubmittedDate : DateTime.MinValue;
                        searchResult.Description = hazard.Description;
                        searchResult.CurrentStatus = hazard.Status ?? "Unknown";
                        //searchResult.IsConfidential = hazard.IsAnonymous;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not load hazard details for code: {HazardCode}", tracking.HazardCode);
                }
            }

            // Get report details if available
            if (!string.IsNullOrEmpty(tracking.ReportCode))
            {
                try
                {
                    var reportQuery = new GetReportByCodeQuery(new ReportID(tracking.ReportCode));
                    var reportResult = await _mediator.SendAsync(reportQuery, CancellationToken.None);

                    if (reportResult.IsSuccess && reportResult.Value is not null)
                    {
                        var report = reportResult.Value;

                        // Use report details if hazard details not available
                        if (string.IsNullOrEmpty(searchResult.SubmittedBy))
                        {
                            searchResult.SubmittedBy = report.SubmittedBy ?? "Unknown";
                        }
                        if (searchResult.SubmittedDate == DateTime.MinValue)
                        {
                            searchResult.SubmittedDate = report.SubmittedDate != DateTime.MinValue ? report.SubmittedDate : DateTime.MinValue;
                        }
                        if (string.IsNullOrEmpty(searchResult.CurrentStatus))
                        {
                            searchResult.CurrentStatus = report.Status ?? "Unknown";
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not load report details for code: {ReportCode}", tracking.ReportCode);
                }
            }

            // Get validation details if available
            if (!string.IsNullOrEmpty(tracking.ReportCode))
            {
                try
                {
                    var validationQuery = new GetReportValidationByReportIdQuery(new ReportID(tracking.ReportCode));
                    var validationResult = await _mediator.SendAsync(validationQuery, CancellationToken.None);

                    if (validationResult.IsSuccess && validationResult.Value is not null)
                    {
                        searchResult.ValidationDecision = validationResult.Value.ValidationDecision ?? "";
                        searchResult.ValidationDate = validationResult.Value.ValidatedDate;
                        searchResult.ValidatedBy = validationResult.Value.ValidatedBy;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not load validation details for report: {ReportCode}", tracking.ReportCode);
                }
            }

            return searchResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building search result for tracking: {TrackingCode}", tracking.TrackingCode);
            return null;
        }
    }

    /// <summary>
    /// Check if search result matches advanced filter criteria
    /// </summary>
    private bool MatchesAdvancedFilters(HazardReportSearchResult result)
    {
        // Filter by reported by
        if (!string.IsNullOrWhiteSpace(SubmittedBySearch) &&
            !result.SubmittedBy.Contains(SubmittedBySearch, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Filter by date range
        if (DateFromSearch.HasValue && result.SubmittedDate < DateFromSearch.Value.Date)
        {
            return false;
        }

        if (DateToSearch.HasValue && result.SubmittedDate > DateToSearch.Value.Date.AddDays(1))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Highlight the best matches in the search results
    /// </summary>
    /// <param name="similarTrackingIds">List of similar tracking IDs</param>
    private void HighlightBestMatches(List<HazardReportTracking> similarTrackingIds)
    {
        // Clear previous similarity scores
        SimilarityScores.Clear();

        foreach (var tracking in similarTrackingIds)
        {
            // Calculate the similarity score for each result
            var score = CalculateSimilarityScore(TrackingIdSearch.Trim(), tracking.TrackingCode);

            // Add to the similarity scores dictionary
            SimilarityScores[tracking.TrackingCode] = score;
        }
    }

    #endregion

    #region Models

    /// <summary>
    /// Search result model for display in grid
    /// </summary>
    public class HazardReportSearchResult
    {
        public string TrackingCode { get; set; } = string.Empty;
        public string HazardCode { get; set; } = string.Empty;
        public string ReportCode { get; set; } = string.Empty;
        public string HazardType { get; set; } = string.Empty;
        public string HazardCategory { get; set; } = string.Empty;
        public string SubmittedBy { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public string ValidationDecision { get; set; } = string.Empty;
        public DateTime? ValidationDate { get; set; }
        public string ValidatedBy { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsConfidential { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    #endregion

    #region Lifecycle Methods

    /// <summary>
    /// Initialize component and handle trackingId parameter if provided
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        // If trackingId parameter is provided in URL, automatically search for it
        if (!string.IsNullOrEmpty(trackingId))
        {
            TrackingIdSearch = trackingId;
            _logger.LogInformation("Auto-searching for tracking ID from URL parameter: {TrackingId}", trackingId);
            
            // Automatically trigger search for the provided tracking ID
            await SearchByTrackingId();
        }
    }

    #endregion
}