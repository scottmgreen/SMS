using SMS_Domain.Entities;
using SMS_Domain.Events;

using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.SMSListings;

public partial class RiskAnalysisListing : ComponentBase
{
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<RiskAnalysisListing> _logger { get; set; } = default!;
    [Inject] private IBaseEventBus _eventBus { get; set; } = default!;

    private RadzenDataGrid<RiskAnalysis>? _analysisGrid;
    private IEnumerable<RiskAnalysis> _analysisResults = new List<RiskAnalysis>();
    private int _totalCount;

    protected override async Task OnInitializedAsync()
    {
        await LoadInitialData();
    }

    private async Task LoadInitialData()
    {
        try
        {
            var query = new GetAllRiskAnalysisQuery();
            var result = await _mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value is not null)
            {
                _analysisResults = result.Value;
                _totalCount = _analysisResults.Count();
                _logger.LogInformation("Loaded {Count} risk analysis results", _totalCount);
            }
            else
            {
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Failed to load risk analysis results"));
                _logger.LogError("Failed to load risk analysis: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading risk analysis");
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Error loading risk analysis"));
        }
    }

    private async Task LoadData(LoadDataArgs args)
    {
        try
        {
            await LoadInitialData();

            var query = _analysisResults.AsQueryable();

            if (!string.IsNullOrEmpty(args.OrderBy))
            {
                var (propertyName, isDescending) = ParseOrderBy(args.OrderBy);
                if (!string.IsNullOrWhiteSpace(propertyName))
                {
                    query = isDescending
                        ? query.OrderByDescending(GetPropertyExpression(propertyName))
                        : query.OrderBy(GetPropertyExpression(propertyName));
                }
            }

            _totalCount = query.Count();

            if (args.Skip.HasValue)
            {
                query = query.Skip(args.Skip.Value);
            }

            if (args.Top.HasValue)
            {
                query = query.Take(args.Top.Value);
            }

            _analysisResults = query.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in LoadData");
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Error loading data"));
        }
    }

    private static Expression<Func<RiskAnalysis, object>> GetPropertyExpression(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(RiskAnalysis), "x");
        var property = Expression.Property(parameter, propertyName);
        var conversion = Expression.Convert(property, typeof(object));
        return Expression.Lambda<Func<RiskAnalysis, object>>(conversion, parameter);
    }

    private static (string propertyName, bool isDescending) ParseOrderBy(string orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
        {
            return (string.Empty, false);
        }

        var parts = orderBy.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var propertyName = parts[0];
        var isDescending = parts.Length > 1 && string.Equals(parts[1], "desc", StringComparison.OrdinalIgnoreCase);
        return (propertyName, isDescending);
    }

    private void ShowActions(RiskAnalysis analysis)
    {
        _logger.LogInformation("Actions requested for risk analysis: {Code}", analysis.Code);
    }
}
