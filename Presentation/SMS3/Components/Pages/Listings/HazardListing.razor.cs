namespace SMS3.Components.Pages.Listings;

public partial class HazardListing : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<HazardListing> Logger { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;


    private RadzenDataGrid<Hazard>? hazardsGrid;
    private IEnumerable<Hazard> hazards = new List<Hazard>();
    private int totalCount;
    private bool isLoading = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadInitialData();
    }

    private async Task LoadInitialData()
    {
        try
        {
            var query = new GetAllHazardsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                hazards = result.Value;
                totalCount = hazards.Count();
                Logger.LogInformation("Loaded {Count} hazards", totalCount);
            }
            else
            {
                ShowErrorNotification("Failed to load hazards");
                Logger.LogError("Failed to load hazards: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading hazards");
            ShowErrorNotification("Error loading hazards");
        }
    }

    private async Task LoadData(LoadDataArgs args)
    {
        try
        {
            isLoading = true;
            StateHasChanged();

            await LoadInitialData();

            var query = hazards.AsQueryable();

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

            hazards = query.ToList();
            totalCount = hazards.Count();
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

    private static Expression<Func<Hazard, object>> GetPropertyExpression(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(Hazard), "x");
        var property = Expression.Property(parameter, propertyName);
        var conversion = Expression.Convert(property, typeof(object));
        return Expression.Lambda<Func<Hazard, object>>(conversion, parameter);
    }

    private void ShowActions(Hazard hazard)
    {
        Logger.LogInformation("Actions requested for hazard: {Code}", hazard.Code);
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

    public BadgeStyle GetStatusBadgeStyle(string? status)
    {
        return status?.ToLower() switch
        {
            "active" => BadgeStyle.Success,
            "pending" => BadgeStyle.Warning,
            "closed" => BadgeStyle.Secondary,
            "cancelled" => BadgeStyle.Danger,
            _ => BadgeStyle.Info
        };
    }
    

    #region CRUD Action Methods

    /// <summary>
    /// Handle view report details - Show comprehensive read-only modal
    /// </summary>
    /// <param name="report">Report to view</param>
    public async Task OnViewHazardAsync(Hazard hazard)
    {
        Logger.LogInformation("View hazard details requested: {HazardCode}", hazard.Code);

        try
        {
            isLoading = true;
            StateHasChanged();

            // Get detailed report information
            //var reportQuery = new GetReportByCodeQuery(new ReportID(report.Code));
            //var reportResult = await Mediator.SendAsync(reportQuery, CancellationToken.None);

            //if (reportResult.IsSuccess && reportResult.Value != null)
            //{
            //    SelectedReport = reportResult.Value;
            //}
            //else
            //{
            //    SelectedReport = report; // Fallback to grid data
            //}

            //// Load associated hazards for this report
            //await LoadAssociatedHazardsAsync(report.Code);

            //// Show the details modal
            //ShowDetailsModal = true;

            //Logger.LogInformation("Displaying details for report: {ReportCode} with {HazardCount} hazards",
            //    report.Code, AssociatedHazards.Count);

            //NotificationService.Notify(new NotificationMessage
            //{
            //    Severity = NotificationSeverity.Info,
            //    Summary = "Report Details Loaded",
            //    Detail = $"Displaying comprehensive details for {report.Code}",
            //    Duration = 2000
            //});
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading report details for {ReportCode}", hazard.Code);

            ShowErrorNotification("Failed to load report details");
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Handle edit hazard request - Navigate to Hazard page in edit mode
    /// </summary>
    /// <param name="hazard">Hazard to edit</param>
    public async Task OnEditHazardAsync(Hazard hazard)
    {
        Logger.LogInformation("Edit report requested: {ReportCode}", hazard.Code);

        try
        {
            var confirmed = await DialogService.Confirm($"Edit hazard '{hazard.Code} - {hazard.Name}'?\n\nThis will navigate to the hazard  form in edit mode.",
                "Edit hazard",
                new ConfirmOptions()
                {
                    OkButtonText = "Yes, Edit Hazard",
                    CancelButtonText = "Cancel"
                });

            if (confirmed == true)
            {
                //Navigation.NavigateTo($"/SMSRiskManagement/HazardReporting?mode=edit&reportCode={report.Code}");

                //Logger.LogInformation("Navigating to edit report: {ReportCode}", report.Code);

                //NotificationService.Notify(new NotificationMessage
                //{
                //    Severity = NotificationSeverity.Info,
                //    Summary = "Navigating to Edit",
                //    Detail = $"Opening {report.Code} for editing...",
                //    Duration = 3000
                //});
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error navigating to edit report {ReportCode}", hazard.Code);
            ShowErrorNotification("Failed to navigate to edit form");
        }
    }

    /// <summary>
    /// Handle delete hazard request - Show confirmation and delete via CQRS
    /// </summary>
    /// <param name="hazard">Hazard to delete</param>
    public async Task OnDeleteHazardAsync(Hazard hazard)
    {
        Logger.LogInformation("Delete report requested: {ReportCode}", hazard.Code);

        try
        {
            // Load associated hazards to show in confirmation
            //await LoadAssociatedHazardsAsync(hazard.Code);

            //var hazardCount = AssociatedHazards.Count;

            var confirmationMessage = $"Are you sure you want to delete hazard '{hazard.Code}'?\n\n" +
                                    $"Hazard Details:\n" +
                                    $"• Name: {hazard.Name ?? "Unnamed Hazard"}\n" +
                                    $"• Status: {hazard.Status ?? "Unknown"}\n" +
                                    $"• Associated Details: \n\n" +
                                    (1 > 0 ? "??  WARNING: This hazard has associated details that may also be affected.\n\n" : "") +
                                    "?? This action cannot be undone!";

            var confirmed = await DialogService.Confirm(confirmationMessage, "Confirm Delete Hazard",
                new ConfirmOptions()
                {
                    OkButtonText = "Yes, Delete Hazard",
                    CancelButtonText = "Cancel",
                    AutoFocusFirstElement = false
                });

            if (confirmed == true)
            {
                //var deleteCommand = new DeleteReportCommand(new ReportID(report.Code));
                //var result = await Mediator.SendAsync(deleteCommand, CancellationToken.None);

                //if (result.IsSuccess && result.Value)
                //{
                //    Logger.LogInformation("Successfully deleted report: {ReportCode}", report.Code);

                //    // Reload the grid data
                //    await LoadInitialData();

                //    NotificationService.Notify(new NotificationMessage
                //    {
                //        Severity = NotificationSeverity.Success,
                //        Summary = "Report Deleted",
                //        Detail = $"Report {report.Code} has been successfully deleted.",
                //        Duration = 4000
                //    });
                //}
                //else
                //{
                //    throw new InvalidOperationException(result.Error?.Message ?? "Failed to delete report");
                //}
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting report: {ReportCode}", hazard.Code);
            ShowErrorNotification("Failed to delete the report");
        }
    }
    #endregion
}