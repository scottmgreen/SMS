using SMS_Domain.Entities;
using SMS_Domain.Events;

using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.SMSListings;

public partial class ReportValidationListing : ComponentBase
{
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<ReportValidationListing> _logger { get; set; } = default!;
    [Inject] private IBaseEventBus _eventBus { get; set; } = default!;
    

    private RadzenDataGrid<ReportValidation>? validationsGrid;
    private IEnumerable<ReportValidation> validations = new List<ReportValidation>();
    private int totalCount;

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

            if (result.IsSuccess && result.Value is not null)
            {
                validations = result.Value;
                totalCount = validations.Count();
                _logger.LogInformation("Loaded {Count} report validations", totalCount);
            }
            else
            {
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Failed to load report validations"));
                _logger.LogError("Failed to load report validations: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading report validations");
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Error loading report validations"));
        }
    }

    private async Task LoadData(LoadDataArgs args)
    {
        try
        {
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
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Error loading data"));
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
