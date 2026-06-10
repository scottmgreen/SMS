using SMS_Shared.Configuration;

using SMS_Application.Interfaces;
using SMS_Domain.Events;
using SMS3.Components.Pages.SMSAssurance.Components;
using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration.Extensions;

namespace SMS3.Components.Pages.SMSAssurance;

public partial class SPIDetail : ComponentBase
{
    #region Parameters
    [Parameter] public string SPICode { get; set; } = string.Empty;
    #endregion

    #region Injected Services
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<SPIDetail> _logger { get; set; } = default!;

    [Inject] private IBaseEventBus _eventBus { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    #endregion

    #region Component State
    private bool IsLoading { get; set; } = true;
    private bool IsAuthenticated { get; set; } = true; // TODO: Implement actual authentication check
    private string? ErrorMessage { get; set; }

    private SafetyPerformanceIndicator? SPI { get; set; }
    private List<SPIDataPoint>? DataPoints { get; set; }
    private SPIDataPoint? CurrentDataPoint { get; set; }
    private bool IsEditingDataPoint { get; set; }

    // Component References
    private RadzenDataGrid<SPIDataPoint>? _dataPointsGrid;
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        _logger.LogInformation("Initializing SPIDetail page for code: {SPICode}", SPICode);
        await LoadSPIDataAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!string.IsNullOrEmpty(SPICode))
        {
            await LoadSPIDataAsync();
        }
    }
    #endregion

    #region Data Loading
    private async Task LoadSPIDataAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            StateHasChanged();

            _logger.LogInformation("Loading SPI data for code: {SPICode}", SPICode);

            if (string.IsNullOrWhiteSpace(SPICode))
            {
                ErrorMessage = "SPI Code is required";
                return;
            }

            var query = new GetSafetyPerformanceIndicatorByCodeQuery(SPICode, includeDataPoints: true);
            var result = await _mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value is not null)
            {
                SPI = result.Value;
                DataPoints = SPI.DataPoints?.OrderByDescending(dp => dp.MeasurementDate).ToList() ?? new List<SPIDataPoint>();

                _logger.LogInformation("Successfully loaded SPI: {SPIName} with {DataPointCount} data points",
                    SPI.Name, DataPoints.Count);
            }
            else
            {
                ErrorMessage = result.Error?.Message ?? "Failed to load SPI data";
                _logger.LogError("Failed to load SPI data for code {SPICode}: {Error}", SPICode, ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "An error occurred while loading SPI data";
            _logger.LogError(ex, "Error loading SPI data for code: {SPICode}", SPICode);
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task RefreshDataAsync()
    {
        await LoadSPIDataAsync();
        await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", "SPI data refreshed successfully"));
    }
    #endregion

    #region _navigation Methods
    private void NavigateToConfiguration()
    {
        _navigation.NavigateToSecure("/SMSAssurance/SPIConfiguration");
    }

    private async Task EditSPI()
    {
        // Navigate to edit mode in configuration page
        _navigation.NavigateTo($"/SMSAssurance/SPIConfiguration?edit={SPICode}");
    }
    #endregion

    #region Data Point Management
    private async Task ShowAddDataPointDialog()
    {
        if (SPI is null) return;

        IsEditingDataPoint = false; // Ensure we're not in edit mode
        CurrentDataPoint = null; // Clear any previous data point

        var currentDataPoint = new SPIDataPoint(new("DP-0000"))
        {
            MeasurementDate = DateTime.Today,
            DataSource = SPI.DataSource,
            CreatedBy = "SYSTEM", // TODO: Get current user
            CreatedDate = DateTime.UtcNow,
            IsVerified = true,
            Period = string.Empty
        };

        var parameters = new Dictionary<string, object?>
        {
            { "SPI", SPI },
            { "DataPoint", currentDataPoint },
            { "IsEditMode", false },
            { "OnSave", EventCallback.Factory.Create<SPIDataPoint>(this, OnDataPointSaved) },
            { "OnCancel", EventCallback.Factory.Create(this, OnDataPointDialogCanceled) }
        };

        await _dialogService.OpenAsync<SPIDataPointDialog>(
            null, // No title needed as it's built into the component
            parameters,
            new DialogOptions
            {
                Width = "800px",
                Height = "1094px",
                Resizable = true,
                Draggable = true,
                CloseDialogOnOverlayClick = false,
                ShowClose = true,
                Style = "border-radius: 8px; box-shadow: 0 8px 32px rgba(0,0,0,0.15);"
            });
    }

    private async Task EditDataPoint(SPIDataPoint dataPoint)
    {
        if (dataPoint is null)
        {
            _logger?.LogWarning("EditDataPoint called with null dataPoint");
            return;
        }

        IsEditingDataPoint = true; // Set this flag so we know we're editing
        CurrentDataPoint = dataPoint; // Store the current data point being edited

        //var currentDataPoint = new SPIDataPoint(new SPIDataPointID(dataPoint.Code))
        //{
        //    Code = dataPoint.Code, // Include the ID for edit mode
        //    Value = dataPoint.Value,
        //    MeasurementDate = dataPoint.MeasurementDate,
        //    Period = dataPoint.Period,
        //    DataSource = dataPoint.DataSource,
        //    EnteredBy = dataPoint.EnteredBy,
        //    EnteredDate = dataPoint.EnteredDate,
        //    Notes = dataPoint.Notes,
        //    IsVerified = dataPoint.IsVerified,
        //    VerifiedBy = dataPoint.VerifiedBy,
        //    VerifiedDate = dataPoint.VerifiedDate
        //};

        var parameters = new Dictionary<string, object?>
        {
            { "SPI", SPI ?? throw new InvalidOperationException("SPI cannot be null") },
            { "DataPoint", dataPoint ?? new SPIDataPoint(new("DP-0000")) },
            { "IsEditMode", true },
            { "OnSave", EventCallback.Factory.Create<SPIDataPoint>(this, OnDataPointSaved) },
            { "OnCancel", EventCallback.Factory.Create(this, OnDataPointDialogCanceled) }
        };

        await _dialogService.OpenAsync<SPIDataPointDialog>(
            null, // No title needed as it's built into the component
            parameters,
            new DialogOptions
            {
                Width = "800px",
                Height = "auto",
                Resizable = true,
                Draggable = true,
                CloseDialogOnOverlayClick = false,
                ShowClose = true,
                Style = "border-radius: 8px; box-shadow: 0 8px 32px rgba(0,0,0,0.15);"
            });
    }

    private async Task DeleteDataPoint(SPIDataPoint dataPoint)
    {
        var confirmed = await _dialogService.Confirm(
            $"Are you sure you want to delete this data point? Value: {dataPoint.Value:F2} from {dataPoint.MeasurementDate:MMM dd, yyyy}",
            "Confirm Delete",
            new ConfirmOptions { OkButtonText = "Delete", CancelButtonText = "Cancel" });

        if (confirmed == true)
        {
            try
            {
                var command = new DeleteSPIDataPointCommand(dataPoint);
                //    dataPoint.Code,
                //    SPI!.Code,
                //    "SYSTEM" // TODO: Get current user
                //);

                var result = await _mediator.SendAsync(command, CancellationToken.None);

                if (result.IsSuccess)
                {
                    await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", "Data point deleted successfully"));
                    await RefreshDataAsync();
                }
                else
                {
                    await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Delete Failed", result.Error?.Message ?? "Failed to delete data point"));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting data point");
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Failed to delete data point"));
            }
        }
    }

    private async Task OnDataPointSaved(SPIDataPoint savedDataPoint)
    {
        try
        {
            if (SPI is null) return;

            _logger.LogInformation("Saving data point for SPI {SPICode}: Value={Value}, Date={Date}",
                SPI.Code, savedDataPoint.Value, savedDataPoint.MeasurementDate);

            if (IsEditingDataPoint && CurrentDataPoint is not null)
            {
                // Update existing data point
                var updateCommand = new UpdateSPIDataPointCommand(savedDataPoint);

                var result = await _mediator.SendAsync(updateCommand, CancellationToken.None);

                if (result.IsSuccess)
                {
                    await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", "Data point updated successfully"));
                    await RefreshDataAsync();
                }
                else
                {
                    await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Update Failed", result.Error?.Message ?? "Failed to update data point"));
                }
            }
            else
            {
                // Add new data point
                var addCommand = new AddSPIDataPointCommand(savedDataPoint);
                //    SPI.Code,
                //    savedDataPoint.Value,
                //    savedDataPoint.MeasurementDate,
                //    savedDataPoint.DataSource,
                //    savedDataPoint.EnteredBy,
                //    savedDataPoint.Notes
                //);

                var result = await _mediator.SendAsync(addCommand, CancellationToken.None);

                if (result.IsSuccess)
                {
                    await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", "Data point added successfully"));
                    await RefreshDataAsync();
                }
                else
                {
                    await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Save Failed", result.Error?.Message ?? "Failed to save data point"));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving data point");
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "An error occurred while saving the data point"));
        }
    }

    private void OnDataPointDialogCanceled()
    {
        CurrentDataPoint = null;
        IsEditingDataPoint = false;
        StateHasChanged();
    }
    #endregion

    #region UI Helper Methods
    private BadgeStyle GetStatusBadgeStyle(SPIStatus status)
    {
        return status.Value switch
        {
            "ACTIVE" => BadgeStyle.Success,
            "INACTIVE" => BadgeStyle.Secondary,
            "UNDER_REVIEW" => BadgeStyle.Warning,
            "DEPRECATED" => BadgeStyle.Danger,
            _ => BadgeStyle.Light
        };
    }

    private string GetCurrentValueStyle()
    {
        if (SPI?.GetCurrentValue() is null) return "color: #6c757d;";

        var currentValue = SPI.GetCurrentValue();

        if (SPI.CriticalThreshold.HasValue && currentValue >= SPI.CriticalThreshold.Value)
            return "color: #dc3545; font-weight: bold;";

        if (SPI.WarningThreshold.HasValue && currentValue >= SPI.WarningThreshold.Value)
            return "color: #fd7e14; font-weight: bold;";

        if (SPI.TargetValue.HasValue && currentValue >= SPI.TargetValue.Value)
            return "color: #198754; font-weight: bold;";

        return "color: #0d6efd;";
    }

    private string GetProgressBarStyle(double achievement)
    {
        if (achievement >= 95) return "background: linear-gradient(90deg, #198754 0%, #28a745 100%);";
        if (achievement >= 80) return "background: linear-gradient(90deg, #0dcaf0 0%, #17a2b8 100%);";
        if (achievement >= 60) return "background: linear-gradient(90deg, #ffc107 0%, #fd7e14 100%);";
        return "background: linear-gradient(90deg, #dc3545 0%, #c82333 100%);";
    }

    private string GetTrendIcon(string direction)
    {
        return direction switch
        {
            "IMPROVING" or "Improving" => "trending_up",
            "DECLINING" or "Declining" => "trending_down",
            _ => "trending_flat"
        };
    }

    private string GetTrendIconStyle(string direction)
    {
        return direction switch
        {
            "IMPROVING" or "Improving" => "color: #198754;",
            "DECLINING" or "Declining" => "color: #dc3545;",
            _ => "color: #6c757d;"
        };
    }

    private string GetDataPointValueStyle(SPIDataPoint dataPoint)
    {
        if (SPI is null) return "";

        if (SPI.CriticalThreshold.HasValue && dataPoint.Value >= SPI.CriticalThreshold.Value)
            return "color: #dc3545; font-weight: bold;";

        if (SPI.WarningThreshold.HasValue && dataPoint.Value >= SPI.WarningThreshold.Value)
            return "color: #fd7e14; font-weight: bold;";

        if (SPI.TargetValue.HasValue && dataPoint.Value >= SPI.TargetValue.Value)
            return "color: #198754; font-weight: bold;";

        return "";
    }

    private string GetTargetComparisonStyle(decimal difference)
    {
        if (difference >= 0) return "color: #198754;";
        return "color: #dc3545;";
    }
    #endregion
}