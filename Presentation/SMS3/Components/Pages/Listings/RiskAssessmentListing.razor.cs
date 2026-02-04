namespace SMS3.Components.Pages.Listings;

public partial class RiskAssessmentListing : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<RiskAssessmentListing> Logger { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private RadzenDataGrid<RiskAssessment>? assessmentsGrid;
    private IEnumerable<RiskAssessment> assessments = new List<RiskAssessment>();
    private int totalCount;
    private bool isLoading = false;
    private bool ShowViewDialog = false;
    private RiskAssessment? SelectedAssessment = null;

    protected override async Task OnInitializedAsync()
    {
        await LoadInitialData();
    }

    private async Task LoadInitialData()
    {
        try
        {
            var query = new GetAllRiskAssessmentsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                assessments = result.Value;
                totalCount = assessments.Count();
                Logger.LogInformation("Loaded {Count} risk assessments", totalCount);
            }
            else
            {
                ShowErrorNotification("Failed to load risk assessments");
                Logger.LogError("Failed to load risk assessments: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading risk assessments");
            ShowErrorNotification("Error loading risk assessments");
        }
    }

    private async Task LoadData(LoadDataArgs args)
    {
        try
        {
            isLoading = true;
            StateHasChanged();

            await LoadInitialData();

            var query = assessments.AsQueryable();

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

            assessments = query.ToList();
            totalCount = assessments.Count();
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

    private static Expression<Func<RiskAssessment, object>> GetPropertyExpression(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(RiskAssessment), "x");
        var property = Expression.Property(parameter, propertyName);
        var conversion = Expression.Convert(property, typeof(object));
        return Expression.Lambda<Func<RiskAssessment, object>>(conversion, parameter);
    }

    private async Task ViewAssessment(RiskAssessment assessment)
    {
        try
        {
            Logger.LogInformation("Viewing risk assessment: {Code}", assessment.Code);
            SelectedAssessment = assessment;
            ShowViewDialog = true;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error viewing risk assessment {Code}", assessment.Code);
            ShowErrorNotification("Error viewing risk assessment");
        }
    }

    private async Task EditAssessment(RiskAssessment assessment)
    {
        try
        {
            Logger.LogInformation("Editing risk assessment: {Code}", assessment.Code);
            await NavigateToTechnicalAssessment(assessment);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error editing risk assessment {Code}", assessment.Code);
            ShowErrorNotification("Error opening risk assessment editor");
        }
    }

   

    
    private async Task NavigateToTechnicalAssessment(RiskAssessment assessment)
    {
        try
        {
            // ✅ PROPER WAY: Get ReportCode via HazardCode using CQRS
            var reportCode = await GetReportCodeFromAssessmentAsync(assessment);

            if (string.IsNullOrEmpty(reportCode))
            {
                Logger.LogWarning("Could not determine ReportCode for assessment {AssessmentCode}", assessment.Code);
                // You could either show an error or use a fallback
                return;
            }

            var navigationUrl = $"/SMSRiskManagement/TechnicalAssessment/{reportCode}/{assessment.HazardCode}/1";

            Logger.LogInformation("Navigating to Technical Assessment: {Url}", navigationUrl);
            Navigation.NavigateTo(navigationUrl);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error navigating to Technical Assessment for {AssessmentCode}", assessment.Code);
            ShowErrorNotification("Failed to navigate to Technical Assessment");
        }
    }
    private async Task<string?> GetReportCodeFromAssessmentAsync(RiskAssessment assessment)
    {
        try
        {
            // Use the HazardCode from the assessment to get the proper ReportCode
            if (string.IsNullOrEmpty(assessment.HazardCode))
            {
                Logger.LogWarning("Assessment {AssessmentCode} has no HazardCode", assessment.Code);
                return null;
            }

            Logger.LogInformation("Getting ReportCode via HazardCode {HazardCode} from assessment {AssessmentCode}",
                assessment.HazardCode, assessment.Code);

            var hazardQuery = new GetHazardByCodeQuery(new HazardID(assessment.HazardCode));
            var hazardResult = await Mediator.SendAsync(hazardQuery, CancellationToken.None);

            if (hazardResult.IsSuccess && hazardResult.Value != null)
            {
                var reportCode = hazardResult.Value.ReportCode;
                Logger.LogInformation("Found ReportCode {ReportCode} for HazardCode {HazardCode}",
                    reportCode, assessment.HazardCode);
                return reportCode;
            }
            else
            {
                Logger.LogWarning("Failed to load Hazard {HazardCode}: {Error}",
                    assessment.HazardCode, hazardResult.Error?.Message ?? "Unknown error");
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting ReportCode from assessment {AssessmentCode} via HazardCode {HazardCode}",
                assessment.Code, assessment.HazardCode);
            return null;
        }
    }



    private async Task DeleteAssessment(RiskAssessment assessment)
    {
        try
        {
            var confirmResult = await DialogService.Confirm(
                message: $"Are you sure you want to delete risk assessment '{assessment.Name}' ({assessment.Code})?\n\nThis action cannot be undone.",
                title: "Confirm Deletion",
                options: new ConfirmOptions
                {
                    OkButtonText = "Yes, Delete",
                    CancelButtonText = "Cancel",
                    Width = "400px"
                });

            if (confirmResult == true)
            {
                Logger.LogInformation("Deleting risk assessment: {Code}", assessment.Code);

                var deleteCommand = new DeleteRiskAssessmentCommand(new RiskAssessmentID(assessment.Id.Value));
                var result = await Mediator.SendAsync(deleteCommand, CancellationToken.None);

                if (result.IsSuccess)
                {
                    ShowSuccessNotification($"Risk assessment '{assessment.Name}' deleted successfully");
                    Logger.LogInformation("Successfully deleted risk assessment: {Code}", assessment.Code);

                    // Refresh the data grid
                    await LoadInitialData();
                    if (assessmentsGrid != null)
                    {
                        await assessmentsGrid.Reload();
                    }
                    StateHasChanged();
                }
                else
                {
                    ShowErrorNotification($"Failed to delete risk assessment: {result.Error?.Message}");
                    Logger.LogError("Failed to delete risk assessment {Code}: {Error}", assessment.Code, result.Error?.Message);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting risk assessment {Code}", assessment.Code);
            ShowErrorNotification("Error deleting risk assessment");
        }
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
            "Draft" => BadgeStyle.Secondary,
            _ => BadgeStyle.Secondary
        };
    }

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
}