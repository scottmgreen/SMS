namespace SMS3.Components.Pages.Listings;

public partial class MitigationListing : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<MitigationListing> Logger { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private RadzenDataGrid<Mitigation>? mitigationsGrid;
    private IEnumerable<Mitigation> mitigations = new List<Mitigation>();
    private int totalCount;
    private bool isLoading = false;
    private bool ShowViewDialog = false;
    private Mitigation? SelectedMitigation = null;

    protected override async Task OnInitializedAsync()
    {
        await LoadInitialData();
    }

    private async Task LoadInitialData()
    {
        try
        {
            var query = new GetAllMitigationsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                mitigations = result.Value;
                totalCount = mitigations.Count();
                Logger.LogInformation("Loaded {Count} mitigations", totalCount);
            }
            else
            {
                ShowErrorNotification("Failed to load mitigations");
                Logger.LogError("Failed to load mitigations: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading mitigations");
            ShowErrorNotification("Error loading mitigations");
        }
    }

    private async Task LoadData(LoadDataArgs args)
    {
        try
        {
            isLoading = true;
            StateHasChanged();

            await LoadInitialData();

            var query = mitigations.AsQueryable();

            if (!string.IsNullOrEmpty(args.OrderBy))
            {
                query = args.OrderBy.Contains("desc")
                    ? query.OrderByDescending(GetPropertyExpression(args.OrderBy.Replace(" desc", "")))
                    : query.OrderBy(GetPropertyExpression(args.OrderBy));
            }

            if (args.Skip.HasValue)
            {
                query = query.Skip(args.Skip.Value);
            }

            if (args.Top.HasValue)
            {
                query = query.Take(args.Top.Value);
            }

            mitigations = query.ToList();
            totalCount = mitigations.Count();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in LoadData");
            ShowErrorNotification("Error loading data");
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private static Expression<Func<Mitigation, object>> GetPropertyExpression(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(Mitigation), "x");
        var property = Expression.Property(parameter, propertyName);
        var conversion = Expression.Convert(property, typeof(object));
        return Expression.Lambda<Func<Mitigation, object>>(conversion, parameter);
    }

    private async Task ViewMitigation(Mitigation mitigation)
    {
        try
        {
            Logger.LogInformation("Viewing mitigation: {Code}", mitigation.Code);
            SelectedMitigation = mitigation;
            ShowViewDialog = true;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error viewing mitigation {Code}", mitigation.Code);
            ShowErrorNotification("Error viewing mitigation");
        }
    }

    private async Task EditMitigation(Mitigation mitigation)
    {
        try
        {
            Logger.LogInformation("Editing mitigation: {Code}", mitigation.Code);

            // Navigate to HazardMitigation edit page
            Navigation.NavigateTo($"/SMSRiskManagement/HazardMitigation/Edit/{mitigation.Code}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error editing mitigation {Code}", mitigation.Code);
            ShowErrorNotification("Error opening mitigation editor");
        }
    }

    private async Task ViewHistory(Mitigation mitigation)
    {
        try
        {
            Logger.LogInformation("Viewing history for mitigation: {Code}", mitigation.Code);
            ShowInfoNotification($"History functionality for mitigation {mitigation.Code} needs to be implemented");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error viewing history for mitigation {Code}", mitigation.Code);
            ShowErrorNotification("Error viewing mitigation history");
        }
    }

    private async Task DeleteMitigation(Mitigation mitigation)
    {
        try
        {
            var confirmResult = await DialogService.Confirm(
                message: $"Are you sure you want to delete mitigation '{mitigation.Name}' ({mitigation.Code})?\n\nThis action cannot be undone.",
                title: "Confirm Deletion",
                options: new ConfirmOptions
                {
                    OkButtonText = "Yes, Delete",
                    CancelButtonText = "Cancel",
                    Width = "400px"
                });

            if (confirmResult == true)
            {
                Logger.LogInformation("Deleting mitigation: {Code}", mitigation.Code);

                var deleteCommand = new DeleteMitigationCommand(new MitigationID(mitigation.Id.Value));
                var result = await Mediator.SendAsync(deleteCommand, CancellationToken.None);

                if (result.IsSuccess)
                {
                    ShowSuccessNotification($"Mitigation '{mitigation.Name}' deleted successfully");
                    Logger.LogInformation("Successfully deleted mitigation: {Code}", mitigation.Code);

                    // Refresh the data grid
                    await LoadInitialData();
                    if (mitigationsGrid != null)
                    {
                        await mitigationsGrid.Reload();
                    }
                    StateHasChanged();
                }
                else
                {
                    ShowErrorNotification($"Failed to delete mitigation: {result.Error?.Message}");
                    Logger.LogError("Failed to delete mitigation {Code}: {Error}", mitigation.Code, result.Error?.Message);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting mitigation {Code}", mitigation.Code);
            ShowErrorNotification("Error deleting mitigation");
        }
    }

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

    private void ShowInfoNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Info,
            Summary = "Information",
            Detail = message,
            Duration = 5000
        });
    }

    private BadgeStyle GetStatusBadgeStyle(string status)
    {
        return status switch
        {
            "Completed" => BadgeStyle.Success,
            "InProgress" => BadgeStyle.Info,
            "Approved" => BadgeStyle.Primary,
            "OnHold" => BadgeStyle.Warning,
            "Cancelled" => BadgeStyle.Danger,
            _ => BadgeStyle.Secondary
        };
    }

    private BadgeStyle GetPriorityBadgeStyle(string priority)
    {
        return priority switch
        {
            "Critical" => BadgeStyle.Danger,
            "High" => BadgeStyle.Warning,
            "Medium" => BadgeStyle.Info,
            "Low" => BadgeStyle.Success,
            _ => BadgeStyle.Secondary
        };
    }
}