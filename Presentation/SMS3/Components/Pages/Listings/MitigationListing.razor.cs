namespace SMS3.Components.Pages.Listings;

public partial class MitigationListing : ComponentBase
{
    [Parameter] public string? ReportId { get; set; }
    [Parameter] public string? HazardCode { get; set; }
    [Parameter] public bool ShowBulkApprove { get; set; } = true;
    [Parameter] public string Title { get; set; } = "Mitigations";

    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<MitigationListing> Logger { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private RadzenDataGrid<Mitigation>? mitigationsGrid;
    private IEnumerable<Mitigation> mitigations = new List<Mitigation>();
    private IEnumerable<Mitigation> selectedMitigations = new List<Mitigation>();
    private int totalCount;
    private bool isLoading = false;
    private bool ShowViewDialog = false;
    private bool ShowBulkApprovalDialog = false;
    private bool IsProcessingBulkApproval = false;
    private Mitigation? SelectedMitigation = null;

    // For context display
    private Hazard? ContextHazard = null;
    private Report? ContextReport = null;

    protected override async Task OnInitializedAsync()
    {
        await LoadContextData();
        await LoadInitialData();
    }

    protected override async Task OnParametersSetAsync()
    {
        // Reload data when parameters change
        await LoadContextData();
        await LoadInitialData();
    }

    private async Task LoadContextData()
    {
        try
        {
            // Load hazard context if provided
            if (!string.IsNullOrEmpty(HazardCode))
            {
                var hazardQuery = new GetHazardByCodeQuery(new HazardID(HazardCode));
                var hazardResult = await Mediator.SendAsync(hazardQuery, CancellationToken.None);

                if (hazardResult.IsSuccess && hazardResult.Value != null)
                {
                    ContextHazard = hazardResult.Value;
                    Logger.LogInformation("Loaded context hazard: {HazardCode}", HazardCode);
                }
            }

            // Load report context if provided
            if (!string.IsNullOrEmpty(ReportId))
            {
                var reportQuery = new GetReportByCodeQuery(new ReportID(ReportId));
                var reportResult = await Mediator.SendAsync(reportQuery, CancellationToken.None);

                if (reportResult.IsSuccess && reportResult.Value != null)
                {
                    ContextReport = reportResult.Value;
                    Logger.LogInformation("Loaded context report: {ReportId}", ReportId);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading context data for Report: {ReportId}, Hazard: {HazardCode}", ReportId, HazardCode);
        }
    }

    private async Task LoadInitialData()
    {
        try
        {
            isLoading = true;
            StateHasChanged();

            if (!string.IsNullOrEmpty(HazardCode))
            {
                // Load mitigations for specific hazard
                var query = new GetMitigationsByHazardCodeQuery(HazardCode);
                var result = await Mediator.SendAsync(query, CancellationToken.None);

                if (result.IsSuccess && result.Value != null)
                {
                    var hazardMitigations = result.Value.ToList();

                    // Further filter by report if provided
                    if (!string.IsNullOrEmpty(ReportId) && ContextHazard?.ReportCode != null)
                    {
                        hazardMitigations = hazardMitigations
                            .Where(m => ContextHazard.ReportCode.Equals(ReportId, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                    }

                    mitigations = hazardMitigations;
                    totalCount = mitigations.Count();
                    Logger.LogInformation("Loaded {Count} mitigations for hazard {HazardCode}", totalCount, HazardCode);
                }
                else
                {
                    mitigations = new List<Mitigation>();
                    totalCount = 0;
                    Logger.LogInformation("No mitigations found for hazard {HazardCode}", HazardCode);
                }
            }
            else
            {
                // Load all mitigations (original behavior)
                var query = new GetAllMitigationsQuery();
                var result = await Mediator.SendAsync(query, CancellationToken.None);

                if (result.IsSuccess && result.Value != null)
                {
                    mitigations = result.Value;
                    totalCount = mitigations.Count();
                    Logger.LogInformation("Loaded {Count} total mitigations", totalCount);
                }
                else
                {
                    ShowErrorNotification("Failed to load mitigations");
                    Logger.LogError("Failed to load mitigations: {Error}", result.Error?.Message);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading mitigations");
            ShowErrorNotification("Error loading mitigations");
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
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

    // ✅ NEW: Bulk Approve functionality
    private async Task OpenBulkApprovalDialog()
    {
        try
        {
            var approvableMitigations = mitigations.Where(m => m.Status != "Approved").ToList();

            if (!approvableMitigations.Any())
            {
                ShowInfoNotification("All mitigations are already approved");
                return;
            }

            selectedMitigations = approvableMitigations;
            ShowBulkApprovalDialog = true;
            StateHasChanged();

            Logger.LogInformation("Opening bulk approval dialog for {Count} mitigations", approvableMitigations.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening bulk approval dialog");
            ShowErrorNotification("Error opening bulk approval dialog");
        }
    }

    private async Task CloseBulkApprovalDialog()
    {
        ShowBulkApprovalDialog = false;
        selectedMitigations = new List<Mitigation>();
        StateHasChanged();
    }

    private async Task ProcessBulkApproval()
    {
        try
        {
            IsProcessingBulkApproval = true;
            StateHasChanged();

            var mitigationsToApprove = selectedMitigations.ToList();
            var successCount = 0;
            var errorCount = 0;

            Logger.LogInformation("Starting bulk approval for {Count} mitigations", mitigationsToApprove.Count);

            foreach (var mitigation in mitigationsToApprove)
            {
                try
                {
                    // Update mitigation status to Approved
                    mitigation.Status = MitigationStatus.Approved;
                    mitigation.UpdatedDate = DateTime.UtcNow;
                    mitigation.UpdatedBy = AuthService.CurrentUser.Code; // You might want to get the current user

                    var updateCommand = new UpdateMitigationCommand(mitigation);
                    var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

                    if (result.IsSuccess)
                    {
                        successCount++;
                        Logger.LogInformation("Approved mitigation: {Code}", mitigation.Code);
                    }
                    else
                    {
                        errorCount++;
                        Logger.LogError("Failed to approve mitigation {Code}: {Error}", mitigation.Code, result.Error?.Message);
                    }
                }
                catch (Exception ex)
                {
                    errorCount++;
                    Logger.LogError(ex, "Error approving mitigation {Code}", mitigation.Code);
                }
            }

            if (successCount > 0)
            {
                ShowSuccessNotification($"Successfully approved {successCount} mitigation(s)");
            }

            if (errorCount > 0)
            {
                ShowErrorNotification($"Failed to approve {errorCount} mitigation(s)");
            }

            // Refresh the data
            await LoadInitialData();

            // Close the dialog
            await CloseBulkApprovalDialog();

            Logger.LogInformation("Bulk approval completed: {SuccessCount} approved, {ErrorCount} failed",
                successCount, errorCount);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing bulk approval");
            ShowErrorNotification("Error processing bulk approval");
        }
        finally
        {
            IsProcessingBulkApproval = false;
            StateHasChanged();
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

    // Helper methods for context display
    private string GetContextTitle()
    {
        if (ContextHazard != null && ContextReport != null)
        {
            return $"Mitigations for Report {ReportId} - Hazard {HazardCode}";
        }
        else if (ContextHazard != null)
        {
            return $"Mitigations for Hazard {HazardCode}";
        }
        else if (ContextReport != null)
        {
            return $"Mitigations for Report {ReportId}";
        }

        return Title;
    }

    private int GetApprovableMitigationCount()
    {
        return mitigations.Count(m => m.Status != "Approved");
    }

    // Notification methods (unchanged)
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