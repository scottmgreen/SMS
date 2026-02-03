namespace SMS3.Components.Pages.Listings;

/// <summary>
/// Airport Shared Dataset Listing page component
/// Displays ADAM datasets with full CRUD operations and export capabilities
/// </summary>
public partial class AirportSharedDatasetListing : ComponentBase
{
    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<AirportSharedDatasetListing> Logger { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    #endregion

    #region State Properties
    private RadzenDataGrid<AirportSharedDataset>? datasetsGrid;
    private IEnumerable<AirportSharedDataset> datasets = new List<AirportSharedDataset>();
    private int totalCount;
    private bool isLoading = false;
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        await LoadInitialData();
    }
    #endregion

    #region Data Loading
    private async Task LoadInitialData()
    {
        try
        {
            var query = new GetAllAirportSharedDatasetsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                datasets = result.Value;
                totalCount = datasets.Count();
                Logger.LogInformation("Loaded {Count} airport shared datasets", totalCount);
            }
            else
            {
                ShowErrorNotification("Failed to load airport shared datasets");
                Logger.LogError("Failed to load datasets: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading airport shared datasets");
            ShowErrorNotification("Error loading datasets");
        }
    }

    private async Task LoadData(LoadDataArgs args)
    {
        try
        {
            isLoading = true;
            StateHasChanged();

            await LoadInitialData();

            var query = datasets.AsQueryable();

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

            datasets = query.ToList();
            totalCount = datasets.Count();
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

    private static Expression<Func<AirportSharedDataset, object>> GetPropertyExpression(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(AirportSharedDataset), "x");
        var property = Expression.Property(parameter, propertyName);
        var conversion = Expression.Convert(property, typeof(object));
        return Expression.Lambda<Func<AirportSharedDataset, object>>(conversion, parameter);
    }
    #endregion

    #region Action Methods
    private async Task ViewDataset(AirportSharedDataset dataset)
    {
        try
        {
            Logger.LogInformation("Viewing dataset: {Code}", dataset.Code);

            // Navigate to dataset details page
            Navigation.NavigateTo($"/DatasetDetails/{dataset.Code}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error viewing dataset {Code}", dataset.Code);
            ShowErrorNotification("Error opening dataset details");
        }
    }

    private async Task EditDataset(AirportSharedDataset dataset)
    {
        try
        {
            Logger.LogInformation("Editing dataset: {Code} with HazardCode: {HazardCode}", dataset.Code, dataset.HazardCode);

            // We need to look up the ReportCode from the Hazard since the dataset only has HazardCode
            if (string.IsNullOrEmpty(dataset.HazardCode))
            {
                ShowErrorNotification("Dataset does not have an associated hazard code for editing");
                return;
            }

            // Get the hazard to find the report code
            var hazardQuery = new GetHazardByCodeQuery(new HazardID(dataset.HazardCode));
            var hazardResult = await Mediator.SendAsync(hazardQuery, CancellationToken.None);

            if (hazardResult.IsSuccess && hazardResult.Value != null)
            {
                var hazard = hazardResult.Value;
                var reportCode = hazard.ReportCode;

                if (string.IsNullOrEmpty(reportCode))
                {
                    ShowErrorNotification("Associated hazard does not have a report code");
                    return;
                }

                // Navigate to the dataset edit page using the original routing pattern
                var editUrl = $"/SMSRiskManagement/AirportSharedDataset/{reportCode}/{dataset.HazardCode}";
                Logger.LogInformation("Navigating to edit dataset: {Url}", editUrl);

                Navigation.NavigateTo(editUrl);
            }
            else
            {
                ShowErrorNotification($"Could not find hazard {dataset.HazardCode} associated with this dataset");
                Logger.LogError("Failed to find hazard {HazardCode} for dataset {DatasetCode}: {Error}",
                    dataset.HazardCode, dataset.Code, hazardResult.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error editing dataset {Code}", dataset.Code);
            ShowErrorNotification("Error opening dataset editor");
        }
    }

    private async Task ExportDataset(AirportSharedDataset dataset)
    {
        try
        {
            Logger.LogInformation("Exporting dataset: {Code}", dataset.Code);

            // TODO: Implement export functionality
            ShowInfoNotification("Export functionality will be available in a future update");

            // Future implementation could include:
            // - Export to Excel/CSV
            // - Export to PDF report
            // - Export to ADAM standard format
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error exporting dataset {Code}", dataset.Code);
            ShowErrorNotification("Error exporting dataset");
        }
    }

    private async Task DuplicateDataset(AirportSharedDataset dataset)
    {
        try
        {
            Logger.LogInformation("Duplicating dataset: {Code}", dataset.Code);

            // TODO: Implement duplication functionality
            ShowInfoNotification("Duplicate functionality will be available in a future update");

            // Future implementation:
            // - Create new dataset with same data but new ID
            // - Navigate to edit page for the duplicated dataset
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error duplicating dataset {Code}", dataset.Code);
            ShowErrorNotification("Error duplicating dataset");
        }
    }

    private async Task ViewHistory(AirportSharedDataset dataset)
    {
        try
        {
            Logger.LogInformation("Viewing history for dataset: {Code}", dataset.Code);

            // TODO: Implement history viewing functionality
            ShowInfoNotification("History functionality will be available in a future update");

            // Future implementation:
            // - Show audit trail of changes
            // - Show version history
            // - Show access logs
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error viewing dataset history {Code}", dataset.Code);
            ShowErrorNotification("Error viewing dataset history");
        }
    }

    private async Task DeleteDataset(AirportSharedDataset dataset)
    {
        try
        {
            Logger.LogInformation("Delete requested for dataset: {Code}", dataset.Code);

            // Show confirmation dialog
            var confirmed = await DialogService.Confirm(
                message: $"Are you sure you want to delete dataset '{dataset.Code}'? This action cannot be undone.",
                title: "Confirm Delete",
                options: new ConfirmOptions
                {
                    OkButtonText = "Delete",
                    CancelButtonText = "Cancel",
                    AutoFocusFirstElement = false
                });

            if (confirmed == true)
            {
                await PerformDelete(dataset);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error initiating delete for dataset {Code}", dataset.Code);
            ShowErrorNotification("Error deleting dataset");
        }
    }

    private async Task PerformDelete(AirportSharedDataset dataset)
    {
        try
        {
            Logger.LogInformation("Performing delete for dataset: {Code}", dataset.Code);

            var datasetId = new AirportSharedDatasetID(dataset.Code);
            var deleteCommand = new DeleteAirportSharedDatasetCommand(datasetId);
            var result = await Mediator.SendAsync(deleteCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification($"Dataset '{dataset.Code}' deleted successfully");
                await LoadInitialData(); // Refresh the grid
                await datasetsGrid?.Reload(); // Refresh the grid display
            }
            else
            {
                ShowErrorNotification($"Failed to delete dataset: {result.Error?.Message}");
                Logger.LogError("Failed to delete dataset {Code}: {Error}", dataset.Code, result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error performing delete for dataset {Code}", dataset.Code);
            ShowErrorNotification("Error deleting dataset");
        }
    }
    #endregion

    #region Notification Methods
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
            Duration = 4000
        });
    }
    #endregion
}