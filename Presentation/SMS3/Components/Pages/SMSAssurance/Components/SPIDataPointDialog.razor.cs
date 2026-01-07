using Microsoft.AspNetCore.Components;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Application.Messaging.Commands;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Shared.Common;
using Radzen;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

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
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    #endregion

    #region Component State
    private EditDataPointModel editModel = new();
    private bool IsSaving { get; set; } = false;

    public class EditDataPointModel
    {
        [Required(ErrorMessage = "Value is required")]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Value must be greater than or equal to 0")]
        public decimal Value { get; set; }

        [Required(ErrorMessage = "Measurement date is required")]
        public DateTime MeasurementDate { get; set; } = DateTime.Today;

        public string Period { get; set; } = string.Empty;

        [Required(ErrorMessage = "Data source is required")]
        public string DataSource { get; set; } = string.Empty;

        public string? Notes { get; set; }
        public bool IsVerified { get; set; }
        public string? VerifiedBy { get; set; }
    }
    #endregion

    #region Lifecycle Methods
    protected override void OnParametersSet()
    {
        InitializeEditModel();
        UpdatePeriod();
    }
    #endregion

    #region Initialization
    private void InitializeEditModel()
    {
        if (DataPoint != null)
        {
            editModel = new EditDataPointModel
            {
                Value = DataPoint.Value,
                MeasurementDate = DataPoint.MeasurementDate,
                Period = DataPoint.Period,
                DataSource = DataPoint.DataSource,
                Notes = DataPoint.Notes,
                IsVerified = DataPoint.IsVerified,
                VerifiedBy = DataPoint.VerifiedBy
            };
        }
        else
        {
            editModel = new EditDataPointModel
            {
                MeasurementDate = DateTime.Today,
                DataSource = SPI?.DataSource ?? "Manual Entry"
            };
        }

        UpdatePeriod();
    }

    private void UpdatePeriod()
    {
        if (SPI != null)
        {
            editModel.Period = GetPeriodFromDate(editModel.MeasurementDate);
        }
    }

    private string GetPeriodFromDate(DateTime date)
    {
        if (SPI == null) return string.Empty;

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
        if (IsSaving) return;
        
        IsSaving = true;
        try
        {
            await HandleSave(editModel);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task HandleSave(EditDataPointModel model)
    {
        try
        {
            IsSaving = true;

            var dataPoint = new SPIDataPoint
            {
                Value = model.Value,
                MeasurementDate = model.MeasurementDate,
                Period = model.Period,
                DataSource = model.DataSource,
                Notes = model.Notes,
                IsVerified = model.IsVerified,
                VerifiedBy = model.VerifiedBy,
                EnteredBy = "SYSTEM", // TODO: Get current user
                EnteredDate = DateTime.UtcNow
            };

            if (model.IsVerified && string.IsNullOrEmpty(model.VerifiedBy))
            {
                dataPoint.VerifiedBy = "SYSTEM"; // TODO: Get current user
                dataPoint.VerifiedDate = DateTime.UtcNow;
            }
            else if (model.IsVerified)
            {
                dataPoint.VerifiedDate = DateTime.UtcNow;
            }

            await OnSave.InvokeAsync(dataPoint);
            DialogService.Close();
        }
        catch (Exception ex)
        {
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Error",
                Detail = $"Failed to save data point: {ex.Message}",
                Duration = 5000
            });
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task HandleCancel()
    {
        try
        {
            await OnCancel.InvokeAsync();
            DialogService.Close();
        }
        catch
        {
            // Silently handle cancel errors and still close dialog
            DialogService.Close();
        }
    }

    private void OnDateChanged()
    {
        UpdatePeriod();
        StateHasChanged();
    }
    #endregion

    #region Data Source Options
    private List<string> GetDataSourceOptions()
    {
        return new List<string>
        {
            "Manual Entry",
            "System Generated",
            "External Import", 
            "Database Query",
            "Excel Import",
            "API Integration",
            "Automated Collection",
            "Survey Data",
            "Third Party System",
            "Legacy System",
            "Mobile App",
            "Web Portal",
            "Sensor Data",
            "Calculated Value",
            "Quality Assurance",
            "Safety Reports",
            "Audit Results",
            "Inspection Data"
        };
    }
    #endregion
}