using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.SMSRiskManagement;

public partial class Hazards : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<Hazards> Logger { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

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
                Logger.LogWarning("Unauthorized access attempt to Hazards page");
                ErrorMessage = "You must be logged in to view hazards.";
            }
            else
            {
                Logger.LogInformation("Authenticated user {UserName} accessing Hazards page", CurrentUserName);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error checking authentication status");
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

            Logger.LogInformation("Loading all hazards for user: {UserName}", CurrentUserName);

            // Execute GetAllHazardsQuery via Mediator
            var query = new GetAllHazardsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                AllHazards = result.Value.ToList();
                Logger.LogInformation("Successfully loaded {Count} hazards", AllHazards.Count);

                // Show success notification
                ShowSuccessNotification($"Successfully loaded {AllHazards.Count} hazards");
            }
            else
            {
                AllHazards = new List<Hazard>();
                ErrorMessage = result.Error?.Message ?? "Failed to load hazards";
                Logger.LogWarning("Failed to load hazards: {Error}", ErrorMessage);

                // Show error notification
                ShowErrorNotification(ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception occurred while loading hazards");
            AllHazards = new List<Hazard>();
            ErrorMessage = "An unexpected error occurred while loading hazards.";

            // Show error notification
            ShowErrorNotification("An unexpected error occurred while loading hazards.");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task RefreshAsync()
    {
        Logger.LogInformation("Refreshing hazards list");
        _expandedRows.Clear(); // Clear expanded rows when refreshing
        await LoadHazardsAsync();
    }

    private void ShowCreateHazardDialog()
    {
        Logger.LogInformation("Navigate to create new hazard");
        Navigation.NavigateTo("/SMSRiskManagement/HazardCreate");
    }

    private async Task OnViewHazardAsync(Hazard hazard)
    {
        Logger.LogInformation("View hazard details: {HazardCode}", hazard.Code);

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

            await DialogService.Alert(hazardDetails, $"Hazard Information - {hazard.Code}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error showing hazard details for {HazardCode}", hazard.Code);
            ShowErrorNotification("Failed to display hazard details.");
        }
    }

    private async Task OnEditHazardAsync(Hazard hazard)
    {
        Logger.LogInformation("Edit hazard: {HazardCode}", hazard.Code);
        Navigation.NavigateTo($"/SMSRiskManagement/HazardEdit/{hazard.Code}");
    }

    private async Task OnDeleteHazardAsync(Hazard hazard)
    {
        Logger.LogInformation("Delete hazard request: {HazardCode}", hazard.Code);

        try
        {
            // Show confirmation dialog
            var confirmed = await DialogService.Confirm(
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
                Logger.LogInformation("Delete confirmed for hazard: {HazardCode}", hazard.Code);

                ShowInfoNotification($"Hazard deletion for {hazard.Code} will be implemented in a future update.");

                // await RefreshAsync(); // Uncomment when delete is implemented
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting hazard: {HazardCode}", hazard.Code);
            ShowErrorNotification("Failed to delete the hazard. Please try again.");
        }
    }

    // Page Size Change Handler
    private async Task OnPageSizeChanged(object value)
    {
        if (int.TryParse(value?.ToString(), out var pageSize))
        {
            Logger.LogInformation("Page size changed from {OldSize} to {NewSize}", PageSize, pageSize);
            PageSize = pageSize;

            // Clear expanded rows when changing page size
            _expandedRows.Clear();

            // Refresh the data list to apply new page size
            if (hazardsDataList != null)
            {
                await hazardsDataList.Reload();
            }

            StateHasChanged();

            // Show notification
            ShowInfoNotification($"Now showing {PageSize} hazards per page");
        }
    }

    // Row Expansion Methods (same pattern as Reports)
    private void ToggleRowExpansion(Hazard hazard)
    {
        if (_expandedRows.Contains(hazard.Code))
        {
            _expandedRows.Remove(hazard.Code);
            Logger.LogDebug("Collapsed hazard details: {HazardCode}", hazard.Code);
        }
        else
        {
            _expandedRows.Add(hazard.Code);
            Logger.LogDebug("Expanded hazard details: {HazardCode}", hazard.Code);
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

    // Notification helper methods
    private void ShowSuccessNotification(string message)
    {
        NotificationHelper.ShowSuccess(NotificationService, message, 3000);
    }

    private void ShowErrorNotification(string message)
    {
        NotificationHelper.ShowError(NotificationService, message, 5000);
    }

    private void ShowInfoNotification(string message)
    {
        NotificationHelper.ShowInfo(NotificationService, message, 5000);
    }
}