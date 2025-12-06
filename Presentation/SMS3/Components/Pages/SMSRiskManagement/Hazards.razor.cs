using Microsoft.AspNetCore.Components;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;
using SMS_Shared.Common;
using Radzen;
using Radzen.Blazor;

namespace SMS3.Components.Pages.SMSRiskManagement;

public partial class Hazards : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ISMSSessionService SessionService { get; set; } = default!;
    [Inject] private ILogger<Hazards> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    // Radzen DataList Reference
    private RadzenDataList<Hazard>? hazardsDataList;

    // State Properties
    private List<Hazard> AllHazards { get; set; } = new();
    private bool IsLoading { get; set; } = true;
    private string ErrorMessage { get; set; } = string.Empty;
    private bool IsAuthenticated { get; set; } = false;
    private string CurrentUserName { get; set; } = string.Empty;

    // Pagination Properties
    private int PageSize { get; set; } = 10;
    private readonly int[] PageSizeOptions = { 5, 10, 20, 50 };

    // Row Expansion State
    private readonly HashSet<string> _expandedRows = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            // Check authentication
            IsAuthenticated = SessionService.IsAuthenticated();
            if (!IsAuthenticated)
            {
                Logger.LogWarning("Unauthenticated access attempt to Hazards page");
                ErrorMessage = "Authentication required to access SMS hazards";
                return;
            }

            CurrentUserName = SessionService.GetCurrentUserDisplayName() ?? "User";
            Logger.LogInformation("Hazards page accessed by user: {UserName}", CurrentUserName);

            // Load hazards
            await LoadHazardsAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error initializing Hazards page");
            ErrorMessage = "Failed to initialize hazards page. Please try refreshing.";
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task LoadHazardsAsync()
    {
        try
        {
            ErrorMessage = string.Empty;
            IsLoading = true;
            StateHasChanged();

            Logger.LogInformation("Loading all hazards via CQRS");

            var query = new GetAllHazardsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess)
            {
                AllHazards = result.Value?.ToList() ?? new List<Hazard>();
                Logger.LogInformation("Successfully loaded {Count} hazards", AllHazards.Count);
            }
            else
            {
                ErrorMessage = result.Error?.Message ?? "Failed to load hazards";
                Logger.LogError("Failed to load hazards: {Error}", ErrorMessage);
                AllHazards = new List<Hazard>();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading hazards");
            ErrorMessage = "An unexpected error occurred while loading hazards.";
            AllHazards = new List<Hazard>();
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
        Navigation.NavigateTo($"/SMSRiskManagement/HazardDetails/{hazard.Code}");
    }

    private async Task OnEditHazardAsync(Hazard hazard)
    {
        Logger.LogInformation("Edit hazard: {HazardCode}", hazard.Code);
        Navigation.NavigateTo($"/SMSRiskManagement/HazardEdit/{hazard.Code}");
    }

    private async Task OnDeleteHazardAsync(Hazard hazard)
    {
        Logger.LogInformation("Delete hazard request: {HazardCode}", hazard.Code);
        
        // TODO: Add proper confirmation dialog
        var confirmed = await Task.FromResult(true);
        
        if (confirmed)
        {
            try
            {
                // TODO: Implement delete via CQRS command
                Logger.LogInformation("Deleting hazard: {HazardCode}", hazard.Code);
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error deleting hazard: {HazardCode}", hazard.Code);
                ErrorMessage = $"Failed to delete hazard {hazard.Code}";
            }
        }
    }

    // Page Size Change Handler
    private async Task OnPageSizeChanged(object value)
    {
        PageSize = (int)value;
        Logger.LogInformation("Page size changed to: {PageSize}", PageSize);
        
        // Refresh the data list to apply new page size
        if (hazardsDataList != null)
        {
            await hazardsDataList.Reload();
        }
        
        StateHasChanged();
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
            "ACTIVE" => BadgeStyle.Success,
            "CLOSED" => BadgeStyle.Secondary,
            "INVESTIGATION" => BadgeStyle.Warning,
            "PENDING" => BadgeStyle.Info,
            _ => BadgeStyle.Secondary
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