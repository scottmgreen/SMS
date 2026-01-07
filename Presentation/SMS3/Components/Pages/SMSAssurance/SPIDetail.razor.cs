using Microsoft.AspNetCore.Components;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Application.Messaging.Commands;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Shared.Common;
using Radzen;
using Radzen.Blazor;
using SMS3.Components.Pages.SMSAssurance.Components;

namespace SMS3.Components.Pages.SMSAssurance;

public partial class SPIDetail : ComponentBase
{
    #region Parameters
    [Parameter] public string SPICode { get; set; } = string.Empty;
    #endregion

    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<SPIDetail> Logger { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
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
    private RadzenDataGrid<SPIDataPoint>? dataPointsGrid;
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        Logger.LogInformation("Initializing SPIDetail page for code: {SPICode}", SPICode);
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

            Logger.LogInformation("Loading SPI data for code: {SPICode}", SPICode);

            if (string.IsNullOrWhiteSpace(SPICode))
            {
                ErrorMessage = "SPI Code is required";
                return;
            }

            var query = new GetSafetyPerformanceIndicatorByCodeQuery(SPICode, includeDataPoints: true);
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                SPI = result.Value;
                DataPoints = SPI.DataPoints?.OrderByDescending(dp => dp.MeasurementDate).ToList() ?? new List<SPIDataPoint>();
                
                Logger.LogInformation("Successfully loaded SPI: {SPIName} with {DataPointCount} data points", 
                    SPI.Name, DataPoints.Count);
            }
            else
            {
                ErrorMessage = result.Error?.Message ?? "Failed to load SPI data";
                Logger.LogError("Failed to load SPI data for code {SPICode}: {Error}", SPICode, ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "An error occurred while loading SPI data";
            Logger.LogError(ex, "Error loading SPI data for code: {SPICode}", SPICode);
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
        ShowSuccessNotification("SPI data refreshed successfully");
    }
    #endregion

    #region Navigation Methods
    private void NavigateToConfiguration()
    {
        Navigation.NavigateTo("/SMSAssurance/SPIConfiguration");
    }

    private async Task EditSPI()
    {
        // Navigate to edit mode in configuration page
        Navigation.NavigateTo($"/SMSAssurance/SPIConfiguration?edit={SPICode}");
    }
    #endregion

    #region Data Point Management
    private async Task ShowAddDataPointDialog()
    {
        if (SPI == null) return;

        IsEditingDataPoint = false; // Ensure we're not in edit mode
        CurrentDataPoint = null; // Clear any previous data point

        var currentDataPoint = new SPIDataPoint
        {
            MeasurementDate = DateTime.Today,
            DataSource = SPI.DataSource,
            EnteredBy = "SYSTEM", // TODO: Get current user
            EnteredDate = DateTime.UtcNow,
            Period = string.Empty
        };
        
        var parameters = new Dictionary<string, object>
        {
            { "SPI", SPI },
            { "DataPoint", currentDataPoint },
            { "IsEditMode", false },
            { "OnSave", EventCallback.Factory.Create<SPIDataPoint>(this, OnDataPointSaved) },
            { "OnCancel", EventCallback.Factory.Create(this, OnDataPointDialogCanceled) }
        };

        await DialogService.OpenAsync<SPIDataPointDialog>(
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

    private async Task EditDataPoint(SPIDataPoint dataPoint)
    {
        IsEditingDataPoint = true; // Set this flag so we know we're editing
        CurrentDataPoint = dataPoint; // Store the current data point being edited
        
        var currentDataPoint = new SPIDataPoint
        {
            Id = dataPoint.Id, // Include the ID for edit mode
            Value = dataPoint.Value,
            MeasurementDate = dataPoint.MeasurementDate,
            Period = dataPoint.Period,
            DataSource = dataPoint.DataSource,
            EnteredBy = dataPoint.EnteredBy,
            EnteredDate = dataPoint.EnteredDate,
            Notes = dataPoint.Notes,
            IsVerified = dataPoint.IsVerified,
            VerifiedBy = dataPoint.VerifiedBy,
            VerifiedDate = dataPoint.VerifiedDate
        };
        
        var parameters = new Dictionary<string, object>
        {
            { "SPI", SPI },
            { "DataPoint", currentDataPoint },
            { "IsEditMode", true },
            { "OnSave", EventCallback.Factory.Create<SPIDataPoint>(this, OnDataPointSaved) },
            { "OnCancel", EventCallback.Factory.Create(this, OnDataPointDialogCanceled) }
        };

        await DialogService.OpenAsync<SPIDataPointDialog>(
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
        var confirmed = await DialogService.Confirm(
            $"Are you sure you want to delete this data point? Value: {dataPoint.Value:F2} from {dataPoint.MeasurementDate:MMM dd, yyyy}",
            "Confirm Delete",
            new ConfirmOptions { OkButtonText = "Delete", CancelButtonText = "Cancel" });

        if (confirmed == true)
        {
            try
            {
                var command = new DeleteSPIDataPointCommand(
                    dataPoint.Id,
                    SPI!.Code,
                    "SYSTEM" // TODO: Get current user
                );

                var result = await Mediator.SendAsync(command, CancellationToken.None);

                if (result.IsSuccess)
                {
                    ShowSuccessNotification("Data point deleted successfully");
                    await RefreshDataAsync();
                }
                else
                {
                    ShowErrorNotification(result.Error?.Message ?? "Failed to delete data point");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error deleting data point");
                ShowErrorNotification("Failed to delete data point");
            }
        }
    }

    private async Task OnDataPointSaved(SPIDataPoint savedDataPoint)
    {
        try
        {
            if (SPI == null) return;

            Logger.LogInformation("Saving data point for SPI {SPICode}: Value={Value}, Date={Date}", 
                SPI.Code, savedDataPoint.Value, savedDataPoint.MeasurementDate);

            if (IsEditingDataPoint && CurrentDataPoint != null)
            {
                // Update existing data point
                var updateCommand = new UpdateSPIDataPointCommand(
                    savedDataPoint.Id,
                    SPI.Code,
                    savedDataPoint.Value,
                    savedDataPoint.MeasurementDate,
                    savedDataPoint.DataSource,
                    "SYSTEM", // TODO: Get current user
                    savedDataPoint.Notes,
                    savedDataPoint.IsVerified,
                    savedDataPoint.VerifiedBy
                );

                var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

                if (result.IsSuccess)
                {
                    ShowSuccessNotification("Data point updated successfully");
                    await RefreshDataAsync();
                }
                else
                {
                    ShowErrorNotification(result.Error?.Message ?? "Failed to update data point");
                }
            }
            else
            {
                // Add new data point
                var addCommand = new AddSPIDataPointCommand(
                    SPI.Code,
                    savedDataPoint.Value,
                    savedDataPoint.MeasurementDate,
                    savedDataPoint.DataSource,
                    savedDataPoint.EnteredBy,
                    savedDataPoint.Notes
                );

                var result = await Mediator.SendAsync(addCommand, CancellationToken.None);

                if (result.IsSuccess)
                {
                    ShowSuccessNotification("Data point added successfully");
                    await RefreshDataAsync();
                }
                else
                {
                    ShowErrorNotification(result.Error?.Message ?? "Failed to save data point");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving data point");
            ShowErrorNotification("An error occurred while saving the data point");
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
        if (SPI?.GetCurrentValue() == null) return "color: #6c757d;";

        var currentValue = SPI.GetCurrentValue().Value;
        
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
        if (SPI == null) return "";

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

    private void ShowWarningNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Warning,
            Summary = "Warning",
            Detail = message,
            Duration = 5000
        });
    }

    private void ShowInfoNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Info,
            Summary = "Info",
            Detail = message,
            Duration = 4000
        });
    }
    #endregion
}