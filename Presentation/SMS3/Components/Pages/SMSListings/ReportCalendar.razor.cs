using SMS_Domain.Entities;
using SMS_Domain.Events;

using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration.Extensions;

namespace SMS3.Components.Pages.SMSListings;

public partial class ReportCalendar : ComponentBase
{
    #region Injected Services
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<ReportCalendar> _logger { get; set; } = default!;
    [Inject] private IBaseEventBus _eventBus { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    #endregion

    #region Component State
    private bool IsLoading { get; set; } = true;
    private RadzenScheduler<ReportSchedulerItem> scheduler = default!;
    private List<Report> Reports { get; set; } = new();
    private List<ReportSchedulerItem> SchedulerData { get; set; } = new();
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        await LoadReportsAsync();
    }
    #endregion

    #region Data Loading
    private async Task LoadReportsAsync()
    {
        try
        {
            IsLoading = true;
            StateHasChanged();

            _logger.LogInformation("Loading reports for calendar display");

            var query = new GetAllReportsQuery();
            var result = await _mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value is not null)
            {
                Reports = result.Value.ToList();
                _logger.LogInformation("Loaded {Count} reports for calendar", Reports.Count);

                // Convert reports to scheduler items
                SchedulerData = Reports.Select(MapReportToSchedulerItem).ToList();
            }
            else
            {
                _logger.LogError("Failed to load reports: {Error}", result.Error?.Message);
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Failed to load reports for calendar"));
                Reports = new List<Report>();
                SchedulerData = new List<ReportSchedulerItem>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading reports for calendar");
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Error loading reports"));
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task RefreshData()
    {
        await LoadReportsAsync();
        await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", "Calendar data refreshed"));
    }
    #endregion

    #region Data Mapping
    private ReportSchedulerItem MapReportToSchedulerItem(Report report)
    {
        var reportDate = report.SubmittedDate ;
        //var hazardsQuery = new GetHazardsByReportCodeQuery(new ReportID(report.Code));
        //var hazardsResult = _mediator.SendAsync(hazardsQuery, CancellationToken.None);

        //var initialHazard = hazardsResult.Result.Value.FirstOrDefault(h => h.IsInitialHazard)
        //                       ?? hazardsResult.Result.Value.FirstOrDefault();

        return new ReportSchedulerItem
        {
            ReportCode = report.Code ?? "Unknown",
            Text = $"{report.Code}", // - {GetShortDescription(report)}",
            Start = reportDate,
            End = reportDate.AddHours(1), // Default 1 hour duration for display
            HazardCategory = "", //initialHazard?.HazardCategory ?? "Unknown",
            HazardType = "", //initialHazard?.HazardType ?? "Unknown",
            Status = report.Status ?? "Unknown",
            Reporter = report.SubmittedBy ?? "Unknown",
            Description = report.Description ?? "No description available"
        };
    }

    private string GetShortDescription(Report report)
    {
        var description = report.Description ?? "No description";
        return description.Length > 50 ? $"{description[..50]}..." : description;
    }

        
    #endregion

    #region Scheduler Event Handlers
    private async Task OnSlotSelect(SchedulerSlotSelectEventArgs args)
    {
        try
        {
            _logger.LogInformation("Slot selected: {Start} to {End}", args.Start, args.End);

            // Optional: Show dialog to create new report for selected date
            // This can be implemented later if needed
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling slot selection");
        }
    }

    private async Task OnAppointmentSelect(SchedulerAppointmentSelectEventArgs<ReportSchedulerItem> args)
    {
        try
        {
            var reportItem = args.Data;
            _logger.LogInformation("Report appointment selected: {ReportCode}", reportItem?.ReportCode);

            // Navigate to the initial hazard report for the selected report
            if (reportItem is not null)
            {
                await NavigateToInitialHazardReportAsync(reportItem.ReportCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling appointment selection");
        }
    }

    private async Task NavigateToInitialHazardReportAsync(string reportCode)
    {
        try
        {
            var hazardsQuery = new GetHazardsByReportCodeQuery(new ReportID(reportCode));
            var hazardsResult = await _mediator.SendAsync(hazardsQuery, CancellationToken.None);

            if (!hazardsResult.IsSuccess || hazardsResult.Value is null || !hazardsResult.Value.Any())
            {
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", $"No hazards found for report {reportCode}"));
                return;
            }

            var initialHazard = hazardsResult.Value.FirstOrDefault(h => h.IsInitialHazard)
                               ?? hazardsResult.Value.FirstOrDefault();

            if (initialHazard is null)
            {
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", $"Unable to determine initial hazard for report {reportCode}"));
                return;
            }

            _logger.LogInformation("Navigating from report {ReportCode} to initial hazard {HazardCode}", reportCode, initialHazard.Code);
            _navigation.NavigateToSecure($"/SMSRiskManagement/HazardReporting/{initialHazard.Code}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to initial hazard for report {ReportCode}", reportCode);
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Failed to open hazard report"));
        }
    }

    private void OnAppointmentRender(SchedulerAppointmentRenderEventArgs<ReportSchedulerItem> args)
    {
        try
        {
            // Customize appointment appearance based on report type
            var reportItem = args.Data;

            if (reportItem is null)
            {
                return;
            }

            
            args.Attributes["class"] = "report-hazard";
                  
            
            //Add tooltip with additional information
            args.Attributes["title"] = $"Report: {reportItem.ReportCode}\nCategory: {reportItem.HazardCategory}\nType: {reportItem.HazardType}\nReporter: {reportItem.Reporter}\nStatus: {reportItem.Status}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering appointment");
        }
    }

    private void OnSlotRender(SchedulerSlotRenderEventArgs args)
    {
        try
        {
            // Optional: Customize slot rendering if needed
            // This can be used to highlight specific dates or add visual indicators
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering slot");
        }
    }
    #endregion

    #region Navigation and Actions
    private async Task GoToToday()
    {
        try
        {
            if (scheduler is not null)
            {
                // Navigate the scheduler to today's date
                scheduler.CurrentDate = DateTime.Today;
                await scheduler.Reload();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to today");
        }
    }
    #endregion

    #region Statistics Methods
    private int GetReportsThisMonth()
    {
        var now = DateTime.Now;
        return Reports.Count(r => r.CreatedDate?.Year == now.Year && r.CreatedDate?.Month == now.Month);
    }

    private int GetReportsThisWeek()
    {
        var now = DateTime.Now;
        var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(7);

        return Reports.Count(r => r.CreatedDate >= startOfWeek && r.CreatedDate < endOfWeek);
    }

    private int GetReportsToday()
    {
        var today = DateTime.Today;
        return Reports.Count(r => r.CreatedDate?.Date == today);
    }
    #endregion

}

#region Supporting Classes and Enums
/// <summary>
/// Scheduler item representation of a Report for calendar display
/// </summary>
public class ReportSchedulerItem
{
    public string ReportCode { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string HazardCategory { get; set; }
    public string HazardType { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Reporter { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}



#endregion
