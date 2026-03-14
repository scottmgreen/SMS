using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.Listings;

public partial class ScoringPanelListing : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<ScoringPanelListing> Logger { get; set; } = default!;
    [Inject] private INotificationHelper NotificationHelper { get; set; } = default!;
    

    private RadzenDataGrid<ScoringPanel>? panelsGrid;
    private IEnumerable<ScoringPanel> panels = new List<ScoringPanel>();
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
            var query = new GetAllScoringPanelsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                panels = result.Value;
                totalCount = panels.Count();
                Logger.LogInformation("Loaded {Count} scoring panels", totalCount);
            }
            else
            {
                await NotificationHelper.ShowErrorAsync("Failed to load scoring panels");
                Logger.LogError("Failed to load scoring panels: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading scoring panels");
            await NotificationHelper.ShowErrorAsync("Error loading scoring panels");
        }
    }

    private async Task LoadData(LoadDataArgs args)
    {
        try
        {
            isLoading = true;
            StateHasChanged();

            await LoadInitialData();

            var query = panels.AsQueryable();

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

            panels = query.ToList();
            totalCount = panels.Count();
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

    private static Expression<Func<ScoringPanel, object>> GetPropertyExpression(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(ScoringPanel), "x");
        var property = Expression.Property(parameter, propertyName);
        var conversion = Expression.Convert(property, typeof(object));
        return Expression.Lambda<Func<ScoringPanel, object>>(conversion, parameter);
    }

    private void ShowActions(ScoringPanel panel)
    {
        Logger.LogInformation("Actions requested for scoring panel: {Code}", panel.Code);
    }
}