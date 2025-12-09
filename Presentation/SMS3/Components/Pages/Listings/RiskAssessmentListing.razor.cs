namespace SMS3.Components.Pages.Listings;

public partial class RiskAssessmentListing : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<RiskAssessmentListing> Logger { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;

    private RadzenDataGrid<RiskAssessment>? assessmentsGrid;
    private IEnumerable<RiskAssessment> assessments = new List<RiskAssessment>();
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

    private void ShowActions(RiskAssessment assessment)
    {
        Logger.LogInformation("Actions requested for risk assessment: {Code}", assessment.Code);
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
}