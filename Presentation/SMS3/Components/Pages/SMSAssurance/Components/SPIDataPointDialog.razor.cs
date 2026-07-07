using System.Globalization;



using SMS_Application.Common;
using SMS_Application.Interfaces;
using SMS_Domain.Events;
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
    private SPIDataPoint _currentDataPoint = default!;
    private bool IsSaving { get; set; } = false;

    private bool IsValid =>
        _currentDataPoint.Value >= 0 &&
        _currentDataPoint.MeasurementDate != default &&
        !string.IsNullOrWhiteSpace(_currentDataPoint.DataSource) &&
        (!_currentDataPoint.IsVerified || !string.IsNullOrWhiteSpace(_currentDataPoint.VerifiedBy));
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
            _currentDataPoint = new SPIDataPoint(new SPIDataPointID(DataPoint.Code))
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
            _currentDataPoint.CreatedDate = DataPoint.CreatedDate;
            _currentDataPoint.UpdatedBy = DataPoint.UpdatedBy;
            _currentDataPoint.UpdatedDate = DataPoint.UpdatedDate;
        }
        else
        {
            // Add mode - create new data point
            _currentDataPoint = new SPIDataPoint(
                new SPIDataPointID("DP-0000")) // TODO: Get current user
            {
                SPIId = SPI?.Code ?? string.Empty,
                MeasurementDate = DateTime.Today,
                DataSource = SPI?.DataSource ?? string.Empty,
                Period = string.Empty
            };
        }

        UpdatePeriod();
    }

    private void UpdatePeriod()
    {
        if (SPI is not null)
        {
            _currentDataPoint.Period = GetPeriodFromDate(_currentDataPoint.MeasurementDate);
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
            if (_currentDataPoint.IsVerified)
            {
                _currentDataPoint.VerifiedDate = DateTime.UtcNow;
                if (string.IsNullOrWhiteSpace(_currentDataPoint.VerifiedBy))
                {
                    _currentDataPoint.VerifiedBy = "SYSTEM"; // Fallback
                }
            }
            else
            {
                _currentDataPoint.VerifiedBy = null;
                _currentDataPoint.VerifiedDate = null;
            }

            await OnSave.InvokeAsync(_currentDataPoint);
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
    /// UPDATED: Now uses reflection-based discovery of IEventSource implementations
    /// </summary>
    private List<string> GetDataSourceOptions()
    {
        var dataSources = new List<string>();

        try
        {
            // Add event-driven data sources (discovered via reflection)
            var eventDrivenSources = SPIConstants.SPIDataSources.GetEventDrivenSourceNames();
            dataSources.AddRange(eventDrivenSources);

            return dataSources.OrderBy(ds => ds).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get dynamic data sources");
            return new List<string>();
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
            var eventSources = SPIConstants.SPIDataSources.GetEventDrivenSourcesByCategory();
            foreach (var category in eventSources.Keys)
            {
                var sourceNames = eventSources[category].Select(eds => eds.DisplayName).ToList();
                grouped[$"{category}"] = sourceNames;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get grouped data sources");
            grouped["Data Sources"] = new List<string>();
        }

        return grouped;
    }

    #endregion
}
