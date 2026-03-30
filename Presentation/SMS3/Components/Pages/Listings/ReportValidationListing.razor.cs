using SMS_Domain.Entities;

using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.Listings;

public partial class ReportValidationListing : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<ReportValidationListing> Logger { get; set; } = default!;
    [Inject] private INotificationHelper NotificationHelper { get; set; } = default!;
    

    private RadzenDataGrid<ReportValidation>? validationsGrid;
    private IEnumerable<ReportValidation> validations = new List<ReportValidation>();
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
            var query = new GetAllReportValidationsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                validations = result.Value;
                totalCount = validations.Count();
                Logger.LogInformation("Loaded {Count} report validations", totalCount);
            }
            else
            {
                await NotificationHelper.ShowErrorAsync("Failed to load report validations");
                Logger.LogError("Failed to load report validations: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading report validations");
            await NotificationHelper.ShowErrorAsync("Error loading report validations");
        }
    }

    private async Task LoadData(LoadDataArgs args)
    {
        try
        {
            isLoading = true;
            StateHasChanged();

            await LoadInitialData();

            var query = validations.AsQueryable();

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

            validations = query.ToList();
            totalCount = validations.Count();
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

    private static Expression<Func<ReportValidation, object>> GetPropertyExpression(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(ReportValidation), "x");
        var property = Expression.Property(parameter, propertyName);
        var conversion = Expression.Convert(property, typeof(object));
        return Expression.Lambda<Func<ReportValidation, object>>(conversion, parameter);
    }

    private void ShowActions(ReportValidation validation)
    {
        Logger.LogInformation("Actions requested for report validation: {Code}", validation.Code);
    }
}