namespace SMS3.Components.Pages.Listings;

public partial class AirportSharedDatasetListing : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<AirportSharedDatasetListing> Logger { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;

    private RadzenDataGrid<AirportSharedDataset>? datasetsGrid;
    private IEnumerable<AirportSharedDataset> datasets = new List<AirportSharedDataset>();
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
            var query = new GetAllAirportSharedDatasetsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                datasets = result.Value;
                totalCount = datasets.Count();
                Logger.LogInformation("Loaded {Count} airport shared datasets", totalCount);
            }
            else
            {
                ShowErrorNotification("Failed to load airport shared datasets");
                Logger.LogError("Failed to load datasets: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading airport shared datasets");
            ShowErrorNotification("Error loading datasets");
        }
    }

    private async Task LoadData(LoadDataArgs args)
    {
        try
        {
            isLoading = true;
            StateHasChanged();

            await LoadInitialData();

            var query = datasets.AsQueryable();
            
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

            datasets = query.ToList();
            totalCount = datasets.Count();
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

    private static Expression<Func<AirportSharedDataset, object>> GetPropertyExpression(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(AirportSharedDataset), "x");
        var property = Expression.Property(parameter, propertyName);
        var conversion = Expression.Convert(property, typeof(object));
        return Expression.Lambda<Func<AirportSharedDataset, object>>(conversion, parameter);
    }

    private void ShowActions(AirportSharedDataset dataset)
    {
        Logger.LogInformation("Actions requested for dataset: {Code}", dataset.Code);
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