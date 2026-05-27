using SMS_Domain.Entities;
using SMS_Domain.Events;

using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.SMSListings;

public partial class ScoringPanelListing : ComponentBase
{
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<ScoringPanelListing> _logger { get; set; } = default!;
    [Inject] private IBaseEventBus _eventBus { get; set; } = default!;
    

    private RadzenDataGrid<ScoringPanel>? panelsGrid;
    private IEnumerable<ScoringPanel> panels = new List<ScoringPanel>();
    private int totalCount;

    protected override async Task OnInitializedAsync()
    {
        await LoadInitialData();
    }

    private async Task LoadInitialData()
    {
        try
        {
            var query = new GetAllScoringPanelsQuery();
            var result = await _mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value is not null)
            {
                panels = result.Value;
                totalCount = panels.Count();
                _logger.LogInformation("Loaded {Count} scoring panels", totalCount);
            }
            else
            {
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Failed to load scoring panels"));
                _logger.LogError("Failed to load scoring panels: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading scoring panels");
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Error loading scoring panels"));
        }
    }

    private async Task LoadData(LoadDataArgs args)
    {
        try
        {
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
            _logger.LogError(ex, "Error in LoadData");
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Error loading data"));
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
        _logger.LogInformation("Actions requested for scoring panel: {Code}", panel.Code);
    }
}
