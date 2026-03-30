using SMS_Domain.Entities;

using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.Listings;

public partial class RiskAnalysisListing : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<RiskAnalysisListing> Logger { get; set; } = default!;
    [Inject] private INotificationHelper NotificationHelper { get; set; } = default!;

    private RadzenDataGrid<RiskAnalysis>? analysisGrid;
    private IEnumerable<RiskAnalysis> analysisResults = new List<RiskAnalysis>();
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
            var query = new GetAllRiskAnalysisQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                analysisResults = result.Value;
                totalCount = analysisResults.Count();
                Logger.LogInformation("Loaded {Count} risk analysis results", totalCount);
            }
            else
            {
                await NotificationHelper.ShowErrorAsync("Failed to load risk analysis results");
                Logger.LogError("Failed to load risk analysis: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading risk analysis");
            await NotificationHelper.ShowErrorAsync("Error loading risk analysis");
        }
    }

    private async Task LoadData(LoadDataArgs args)
    {
        try
        {
            isLoading = true;
            StateHasChanged();

            await LoadInitialData();

            var query = analysisResults.AsQueryable();

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

            analysisResults = query.ToList();
            totalCount = analysisResults.Count();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in LoadData");
            await NotificationHelper.ShowErrorAsync("Error loading data");
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private static Expression<Func<RiskAnalysis, object>> GetPropertyExpression(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(RiskAnalysis), "x");
        var property = Expression.Property(parameter, propertyName);
        var conversion = Expression.Convert(property, typeof(object));
        return Expression.Lambda<Func<RiskAnalysis, object>>(conversion, parameter);
    }

    private void ShowActions(RiskAnalysis analysis)
    {
        Logger.LogInformation("Actions requested for risk analysis: {Code}", analysis.Code);
    }
}