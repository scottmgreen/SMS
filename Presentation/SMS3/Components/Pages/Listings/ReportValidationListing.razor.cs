using SMS_Domain.Entities;

using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.Listings;

public partial class ReportValidationListing : ComponentBase
{
    [Inject] private IMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<ReportValidationListing> _logger { get; set; } = default!;
    [Inject] private INotificationHelper _notificationHelper { get; set; } = default!;
    

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
            var result = await _mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                validations = result.Value;
                totalCount = validations.Count();
                _logger.LogInformation("Loaded {Count} report validations", totalCount);
            }
            else
            {
                await _notificationHelper.ShowErrorAsync("Failed to load report validations");
                _logger.LogError("Failed to load report validations: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading report validations");
            await _notificationHelper.ShowErrorAsync("Error loading report validations");
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
            _logger.LogError(ex, "Error in LoadData");
            await _notificationHelper.ShowErrorAsync("Error loading data");
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
        _logger.LogInformation("Actions requested for report validation: {Code}", validation.Code);
    }
}