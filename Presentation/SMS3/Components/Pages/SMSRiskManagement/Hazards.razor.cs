using SMS_Shared.Configuration;

using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration.Extensions;

namespace SMS3.Components.Pages.SMSRiskManagement;

public partial class Hazards : ComponentBase
{
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<Hazards> _logger { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;
    
    [Inject] private INotificationHelper _notificationHelper { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;

    [Inject] private ICurrentUserService CurrentUserService { get; set; } = default!;

    // **CASCADING PARAMETER**: Get authentication from MainLayout (same pattern as Reports.razor)
    
    // Radzen DataList Reference
    private RadzenDataList<Hazard>? hazardsDataList;

    // State Properties
    private List<Hazard> AllHazards { get; set; } = new();
    private bool IsLoading { get; set; } = true;
    private string ErrorMessage { get; set; } = string.Empty;

    // Authentication Properties (same pattern as Reports.razor)
    private bool IsAuthenticated => CurrentUserService?.IsAuthenticated == true;
    private string? CurrentUserName => CurrentUserService?.UserDisplayName;

    // Pagination Properties
    private int PageSize { get; set; } = 10;
    private readonly int[] PageSizeOptions = { 5, 10, 20, 50 };

    // Row Expansion State
    private readonly HashSet<string> _expandedRows = new();

    protected override async Task OnInitializedAsync()
    {
        await CheckAuthenticationAsync();
        await LoadHazardsAsync();
    }

    /// <summary>
    /// Check user authentication status
    /// </summary>
    private async Task CheckAuthenticationAsync()
    {
        try
        {
            if (!IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized access attempt to Hazards page");
                ErrorMessage = "You must be logged in to view hazards.";
            }
            else
            {
                _logger.LogInformation("Authenticated user {UserName} accessing Hazards page", CurrentUserName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking authentication status");
            ErrorMessage = "Authentication check failed.";
        }
    }

    private async Task LoadHazardsAsync()
    {
        if (!IsAuthenticated)
        {
            IsLoading = false;
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            _logger.LogInformation("Loading all hazards for user: {UserName}", CurrentUserName);

            // Execute GetAllHazardsQuery via Mediator
            var query = new GetAllHazardsQuery();
            var result = await _mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value is not null)
            {
                AllHazards = result.Value.ToList();
                await _notificationHelper.ShowSuccessAsync($"Successfully loaded {AllHazards.Count} hazards");
                _logger.LogInformation("Successfully loaded {Count} hazards", AllHazards.Count);
            }
            else
            {
                ErrorMessage = result.Error?.Message ?? "Failed to load hazards";
                await _notificationHelper.ShowErrorAsync(ErrorMessage);
                _logger.LogError("Failed to load hazards: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "An unexpected error occurred while loading hazards.";
            _logger.LogError(ex, "Exception loading hazards for user: {UserName}", CurrentUserName);
            await _notificationHelper.ShowErrorAsync("An unexpected error occurred while loading hazards.");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task RefreshAsync()
    {
        _logger.LogInformation("Refreshing hazards list");
        _expandedRows.Clear(); // Clear expanded rows when refreshing
        await LoadHazardsAsync();
    }

    private void ShowCreateHazardDialog()
    {
        _logger.LogInformation("Navigate to create new hazard");
        _navigation.NavigateToSecure("/SMSRiskManagement/HazardCreate");
    }

    private async Task OnViewHazardAsync(Hazard hazard)
    {
        _logger.LogInformation("View hazard details: {HazardCode}", hazard.Code);

        try
        {
            // Show detailed hazard information dialog
            var hazardDetails = $@"Hazard Details:

                Code: {hazard.Code}
                Name: {hazard.Name ?? "Not specified"}
                Type: {hazard.HazardType ?? "Not specified"}
                Category: {hazard.HazardCategory ?? "Not specified"}
                Description: {hazard.Description ?? "Not specified"}
                Status: {hazard.Status?.Name ?? "Not specified"}

                Risk Level: {hazard.HazardRiskLevel ?? "Not assessed"}

                Created: {hazard.CreatedDate?.ToString("MM/dd/yyyy") ?? "N/A"}";

            await _dialogService.Alert(hazardDetails, $"Hazard Information - {hazard.Code}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening hazard details for: {HazardCode}", hazard.Code);
            await _notificationHelper.ShowErrorAsync("Failed to display hazard details.");
        }
    }

    private async Task OnEditHazardAsync(Hazard hazard)
    {
        _logger.LogInformation("Edit hazard: {HazardCode}", hazard.Code);
        // ?? SECURE NAVIGATION - Navigate to Hazard Edit with encrypted URL
        _navigation.NavigateToSecure($"/SMSRiskManagement/HazardEdit/{hazard.Code}");
    }

    private async Task OnDeleteHazardAsync(Hazard hazard)
    {
        _logger.LogInformation("Delete hazard request: {HazardCode}", hazard.Code);

        try
        {
            // Show confirmation dialog
            var confirmed = await _dialogService.Confirm(
                $"Are you sure you want to delete hazard {hazard.Code}?\n\nThis action cannot be undone.",
                "Confirm Delete",
                new ConfirmOptions()
                {
                    OkButtonText = "Yes, Delete",
                    CancelButtonText = "Cancel"
                });

            if (confirmed == true)
            {
                // TODO: Implement delete via CQRS command when available
                _logger.LogInformation("Delete confirmed for hazard: {HazardCode}", hazard.Code);

                await _notificationHelper.ShowInfoAsync($"Hazard deletion for {hazard.Code} will be implemented in a future update.");

                // await RefreshAsync(); // Uncomment when delete is implemented
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting hazard: {HazardCode}", hazard.Code);
            await _notificationHelper.ShowErrorAsync("Failed to delete the hazard. Please try again.");
        }
    }

    // Page Size Change Handler
    private async Task OnPageSizeChanged(object value)
    {
        if (int.TryParse(value?.ToString(), out var pageSize))
        {
            _logger.LogInformation("Page size changed from {OldSize} to {NewSize}", PageSize, pageSize);
            PageSize = pageSize;

            // Clear expanded rows when changing page size
            _expandedRows.Clear();

            // Refresh the data list to apply new page size
            if (hazardsDataList is not null)
            {
                await hazardsDataList.Reload();
            }

            StateHasChanged();

            // Show notification
            await _notificationHelper.ShowInfoAsync($"Now showing {PageSize} hazards per page");
        }
    }

    // Row Expansion Methods (same pattern as Reports)
    private void ToggleRowExpansion(Hazard hazard)
    {
        if (_expandedRows.Contains(hazard.Code))
        {
            _expandedRows.Remove(hazard.Code);
            _logger.LogDebug("Collapsed hazard details: {HazardCode}", hazard.Code);
        }
        else
        {
            _expandedRows.Add(hazard.Code);
            _logger.LogDebug("Expanded hazard details: {HazardCode}", hazard.Code);
        }
        StateHasChanged();
    }

    private bool IsRowExpanded(Hazard hazard)
    {
        return _expandedRows.Contains(hazard.Code);
    }

    // Badge Style Helpers
    private BadgeStyle GetStatusBadgeStyle(string? status)
    {
        return status?.ToUpper() switch
        {
            //HazardStatus.InitialRiskAssessment.Name.ToString() => BadgeStyle.Success,
            //"CLOSED" => BadgeStyle.Secondary,
            //"INVESTIGATION" => BadgeStyle.Warning,
            //"PENDING" => BadgeStyle.Info,
            //"UNDER_REVIEW" => BadgeStyle.Info,
            //"ESCALATED" => BadgeStyle.Warning,
            _ => BadgeStyle.Success
        };
    }

    private BadgeStyle GetPriorityBadgeStyle(string? priority)
    {
        return priority?.ToUpper() switch
        {
            "HIGH" => BadgeStyle.Danger,
            "MEDIUM" => BadgeStyle.Warning,
            "LOW" => BadgeStyle.Info,
            "VERYHIGH" => BadgeStyle.Danger,
            "VERYLOW" => BadgeStyle.Light,
            _ => BadgeStyle.Secondary
        };
    }
}