using System.Globalization;



using SMS_Application.Common;
using SMS_Application.Interfaces;
using SMS_Domain.Events.UIEvents;
using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.SMSAssurance.Components;

public partial class SPIDataPointDialog : ComponentBase
{
    #region Parameters
    [Parameter] public SafetyPerformanceIndicator? SPI { get; set; }
    [Parameter] public SPIDataPoint? DataPoint { get; set; }
    [Parameter] public bool IsEditMode { get; set; }
    [Parameter] public EventCallback<SPIDataPoint> OnSave { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    #endregion

    #region Injected Services
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<SPIDataPointDialog> _logger { get; set; } = default!;
    [Inject] private IBaseEventBus _eventBus { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;
    #endregion

    #region Component State
    private SPIDataPoint currentDataPoint = default!;
    private bool IsSaving { get; set; } = false;

    private bool IsValid =>
        currentDataPoint.Value >= 0 &&
        currentDataPoint.MeasurementDate != default &&
        !string.IsNullOrWhiteSpace(currentDataPoint.DataSource) &&
        (!currentDataPoint.IsVerified || !string.IsNullOrWhiteSpace(currentDataPoint.VerifiedBy));
    #endregion

    #region Lifecycle Methods
    protected override void OnParametersSet()
    {
        InitializeDataPoint();
        UpdatePeriod();
    }
    #endregion

    #region Initialization
    private void InitializeDataPoint()
    {
        if (DataPoint is not null)
        {
            // Edit mode - clone existing data point
            currentDataPoint = new SPIDataPoint(new SPIDataPointID(DataPoint.Code))
            {
                SPIId = DataPoint.SPIId,
                Value = DataPoint.Value,
                MeasurementDate = DataPoint.MeasurementDate,
                Period = DataPoint.Period,
                DataSource = DataPoint.DataSource,
                Notes = DataPoint.Notes,
                IsVerified = DataPoint.IsVerified,
                VerifiedBy = DataPoint.VerifiedBy,
                VerifiedDate = DataPoint.VerifiedDate
            };

            // Preserve audit fields from original
            currentDataPoint.CreatedDate = DataPoint.CreatedDate;
            currentDataPoint.UpdatedBy = DataPoint.UpdatedBy;
            currentDataPoint.UpdatedDate = DataPoint.UpdatedDate;
        }
        else
        {
            // Add mode - create new data point
            currentDataPoint = new SPIDataPoint(
                new SPIDataPointID("DP-0000")) // TODO: Get current user
            {
                SPIId = SPI?.Code ?? string.Empty,
                MeasurementDate = DateTime.Today,
                DataSource = SPI?.DataSource ?? SPIConstants.DataSources.ManualEntry,
                Period = string.Empty
            };
        }

        UpdatePeriod();
    }

    private void UpdatePeriod()
    {
        if (SPI is not null)
        {
            currentDataPoint.Period = GetPeriodFromDate(currentDataPoint.MeasurementDate);
        }
    }

    private string GetPeriodFromDate(DateTime date)
    {
        if (SPI is null) return string.Empty;

        return SPI.MeasurementFrequency.Value switch
        {
            "DAILY" => date.ToString("yyyy-MM-dd"),
            "WEEKLY" => $"{date.Year}-W{GetWeekNumber(date):D2}",
            "MONTHLY" => date.ToString("yyyy-MM"),
            "QUARTERLY" => $"{date.Year}-Q{GetQuarter(date)}",
            "ANNUALLY" => date.ToString("yyyy"),
            _ => date.ToString("yyyy-MM")
        };
    }

    private int GetWeekNumber(DateTime date)
    {
        var culture = CultureInfo.CurrentCulture;
        return culture.Calendar.GetWeekOfYear(date,
            CalendarWeekRule.FirstDay, DayOfWeek.Monday);
    }

    private int GetQuarter(DateTime date)
    {
        return (date.Month - 1) / 3 + 1;
    }
    #endregion

    #region Event Handlers
    private async Task SubmitForm()
    {
        if (!IsValid || IsSaving) return;

        IsSaving = true;
        try
        {
            await HandleSave();
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task HandleSave()
    {
        try
        {
            // Set verification details if verified
            if (currentDataPoint.IsVerified)
            {
                currentDataPoint.VerifiedDate = DateTime.UtcNow;
                if (string.IsNullOrWhiteSpace(currentDataPoint.VerifiedBy))
                {
                    currentDataPoint.VerifiedBy = "SYSTEM"; // Fallback
                }
            }
            else
            {
                currentDataPoint.VerifiedBy = null;
                currentDataPoint.VerifiedDate = null;
            }

            await OnSave.InvokeAsync(currentDataPoint);
            _dialogService.Close();
        }
        catch (Exception ex)
        {
            // EventBus-driven error notification
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error(
                "Save Failed", 
                "Failed to save data point. Please try again."
            ));

            _logger.LogError(ex, "Failed to save SPI data point in dialog");
        }
    }

    private async Task HandleCancel()
    {
        try
        {
            await OnCancel.InvokeAsync();
            _dialogService.Close();
        }
        catch
        {
            _dialogService.Close();
        }
    }

    private void OnDateChanged()
    {
        UpdatePeriod();
        StateHasChanged();
    }
    #endregion

    #region Data Source Options

    /// <summary>
    /// Gets dynamic data sources from domain events + manual sources
    /// UPDATED: Now uses reflection-based discovery of IEventDataSource implementations
    /// </summary>
    private List<string> GetDataSourceOptions()
    {
        var dataSources = new List<string>();

        try
        {
            // Add event-driven data sources (discovered via reflection)
            var eventDrivenSources = SPIConstants.DataSources.GetEventDrivenSourceNames();
            dataSources.AddRange(eventDrivenSources);

            // Add manual data sources
            var manualSources = SPIConstants.DataSources.GetManualSources();
            dataSources.AddRange(manualSources);

            return dataSources.OrderBy(ds => ds).ToList();
        }
        catch (Exception ex)
        {
            // Fallback to manual sources only if reflection fails
            _logger.LogWarning(ex, "Failed to get dynamic data sources, falling back to manual sources");
            return SPIConstants.DataSources.GetManualSources();
        }
    }

    /// <summary>
    /// Gets grouped data sources for better UI organization
    /// </summary>
    private Dictionary<string, List<string>> GetGroupedDataSourceOptions()
    {
        var grouped = new Dictionary<string, List<string>>();

        try
        {
            // Event-driven sources grouped by category
            var eventSources = SPIConstants.DataSources.GetEventDrivenSourcesByCategory();
            foreach (var category in eventSources.Keys)
            {
                var sourceNames = eventSources[category].Select(eds => eds.DisplayName).ToList();
                grouped[$"?? {category}"] = sourceNames;
            }

            // Manual sources as a separate group
            grouped["?? Manual Sources"] = SPIConstants.DataSources.GetManualSources();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get grouped data sources");
            grouped["Data Sources"] = SPIConstants.DataSources.GetManualSources();
        }

        return grouped;
    }

    #endregion
}