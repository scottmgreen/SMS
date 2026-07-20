using System.Linq.Expressions;

using SMS_Domain.Entities;
using SMS_Domain.Events;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Radzen;

using SMS_Application.Interfaces;
using SMS_Application.Commands;
using SMS_Application.Queries;

using SMS_Domain.ValueObjects;

using SMS_Shared.Configuration;

using SMS3.Components.Shared;
using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration.Extensions;

namespace SMS3.Components.Pages.SMSListings;

/// <summary>
/// Airport Shared Dataset Listing page component
/// Displays ADAM datasets with full CRUD operations and export capabilities
/// </summary>
public partial class AirportSharedDatasetListing : ComponentBase
{
    #region Injected Services
    [Inject] private IBaseMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<AirportSharedDatasetListing> Logger { get; set; } = default!;
    [Inject] private IBaseEventBus EventBus { get; set; } = default!;

    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    #endregion

    #region State Properties
    private RadzenDataGrid<AirportSharedDataset>? _datasetsGrid;
    private IEnumerable<AirportSharedDataset> _datasets = new List<AirportSharedDataset>();
    private int _totalCount;
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

            if (result.IsSuccess && result.Value is not null)
            {
                _datasets = result.Value;
                _totalCount = _datasets.Count();
                Logger.LogInformation("Loaded {Count} airport shared datasets", _totalCount);
            }
            else
            {
                await ShowErrorAsyncNotification("Failed to load airport shared datasets");
                Logger.LogError("Failed to load datasets: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading airport shared datasets");
            await ShowErrorAsyncNotification("Error loading datasets");
        }
    }

    private async Task LoadData(LoadDataArgs args)
    {
        try
        {
            StateHasChanged();

            await LoadInitialData();

            var query = _datasets.AsQueryable();

            if (!string.IsNullOrEmpty(args.OrderBy))
            {
                var (propertyName, isDescending) = ParseOrderBy(args.OrderBy);
                if (!string.IsNullOrWhiteSpace(propertyName))
                {
                    query = isDescending
                        ? query.OrderByDescending(GetPropertyExpression(propertyName))
                        : query.OrderBy(GetPropertyExpression(propertyName));
                }
            }

            _totalCount = query.Count();

            if (args.Skip.HasValue)
            {
                query = query.Skip(args.Skip.Value);
            }

            const int pageSize = 15;
            query = query.Take(pageSize);

            _datasets = query.ToList();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in LoadData");
            await ShowErrorAsyncNotification("Error loading data");
        }
        finally
        {
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

    private static (string propertyName, bool isDescending) ParseOrderBy(string orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
        {
            return (string.Empty, false);
        }

        var parts = orderBy.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var propertyName = parts[0];
        var isDescending = parts.Length > 1 && string.Equals(parts[1], "desc", StringComparison.OrdinalIgnoreCase);
        return (propertyName, isDescending);
    }
    #endregion

    #region Action Methods
    private async Task ViewDataset(AirportSharedDataset dataset)
    {
        try
        {
            Logger.LogInformation("Viewing dataset: {Code}", dataset.Code);

            // Navigate to dataset details page
            Navigation.NavigateToSecure($"/DatasetDetails/{dataset.Code}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error viewing dataset {Code}", dataset.Code);
            await ShowErrorAsyncNotification("Error opening dataset details");
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
                await ShowErrorAsyncNotification("Dataset does not have an associated hazard code for editing");
                return;
            }

            // Get the hazard to find the report code
            var hazardQuery = new GetHazardByCodeQuery(new HazardID(dataset.HazardCode));
            var hazardResult = await Mediator.SendAsync(hazardQuery, CancellationToken.None);

            if (hazardResult.IsSuccess && hazardResult.Value is not null)
            {
                var hazard = hazardResult.Value;
                var reportCode = hazard.ReportCode;

                if (string.IsNullOrEmpty(reportCode))
                {
                    await ShowErrorAsyncNotification("Associated hazard does not have a report code");
                    return;
                }

                // Navigate to the dataset edit page using the original routing pattern
                var editUrl = $"/SMSRiskManagement/AirportSharedDataset/{reportCode}/{dataset.HazardCode}";
                Logger.LogInformation("Navigating to edit dataset: {Url}", editUrl);

                Navigation.NavigateToSecure(editUrl);
            }
            else
            {
                await ShowErrorAsyncNotification($"Could not find hazard {dataset.HazardCode} associated with this dataset");
                Logger.LogError("Failed to find hazard {HazardCode} for dataset {DatasetCode}: {Error}",
                    dataset.HazardCode, dataset.Code, hazardResult.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error editing dataset {Code}", dataset.Code);
            await ShowErrorAsyncNotification("Error opening dataset editor");
        }
    }

    private async Task ExportDataset(AirportSharedDataset dataset)
    {
        try
        {
            Logger.LogInformation("Exporting dataset: {Code}", dataset.Code);

            // TODO: Implement export functionality
            await ShowInfoAsyncNotification("Export functionality will be available in a future update");

            // Future implementation could include:
            // - Export to Excel/CSV
            // - Export to PDF report
            // - Export to ADAM standard format
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error exporting dataset {Code}", dataset.Code);
            await ShowErrorAsyncNotification("Error exporting dataset");
        }
    }

    private async Task DuplicateDataset(AirportSharedDataset dataset)
    {
        try
        {
            Logger.LogInformation("Duplicating dataset: {Code}", dataset.Code);

            // TODO: Implement duplication functionality
            await ShowInfoAsyncNotification("Duplicate functionality will be available in a future update");

            // Future implementation:
            // - Create new dataset with same data but new ID
            // - Navigate to edit page for the duplicated dataset
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error duplicating dataset {Code}", dataset.Code);
            await ShowErrorAsyncNotification("Error duplicating dataset");
        }
    }

    private async Task ViewHistory(AirportSharedDataset dataset)
    {
        try
        {
            Logger.LogInformation("Viewing history for dataset: {Code}", dataset.Code);

            // TODO: Implement history viewing functionality
            await ShowInfoAsyncNotification("History functionality will be available in a future update");

            // Future implementation:
            // - Show audit trail of changes
            // - Show version history
            // - Show access logs
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error viewing dataset history {Code}", dataset.Code);
            await ShowErrorAsyncNotification("Error viewing dataset history");
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
            await ShowErrorAsyncNotification("Error deleting dataset");
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
                await ShowSuccessAsyncNotification($"Dataset '{dataset.Code}' deleted successfully");
                await LoadInitialData(); // Refresh the grid
            if (_datasetsGrid != null)
                await _datasetsGrid.Reload(); // Refresh the grid display
            }
            else
            {
                await ShowErrorAsyncNotification($"Failed to delete dataset: {result.Error?.Message}");
                Logger.LogError("Failed to delete dataset {Code}: {Error}", dataset.Code, result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error performing delete for dataset {Code}", dataset.Code);
            await ShowErrorAsyncNotification("Error deleting dataset");
        }
    }
    #endregion

    #region Notification Methods (EventBus-Driven)
    private async Task ShowSuccessAsyncNotification(string message)
    {
        await EventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", message));
    }

    private async Task ShowErrorAsyncNotification(string message)
    {
        await EventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", message));
    }

    private async Task ShowInfoAsyncNotification(string message)
    {
        await EventBus.PublishUIEventAsync(UINotificationEvent.Info("Information", message));
    }
    #endregion
}
