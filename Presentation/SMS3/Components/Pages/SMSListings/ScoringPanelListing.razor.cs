using SMS_Domain.Entities;
using SMS_Domain.Events;

using SMS3.Components.Shared.UIHelpers;
using SMS3.Components.Shared;
using SMS3.Hubs.Contracts;

namespace SMS3.Components.Pages.SMSListings;

public partial class ScoringPanelListing : ComponentBase, IEventReceiver, IDisposable
{
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<ScoringPanelListing> _logger { get; set; } = default!;
    [Inject] private IBaseEventBus _eventBus { get; set; } = default!;
    [Inject] private INotificationHelper _notificationHelper { get; set; } = default!;
    [Inject] private ICurrentUserService _currentUserService { get; set; } = default!;
    [Inject] private ILiveUpdatesPublisher _liveUpdatesPublisher { get; set; } = default!;
    

    private RadzenDataGrid<ScoringPanel>? _panelsGrid;
    private IEnumerable<ScoringPanel> _panels = new List<ScoringPanel>();
    private readonly Dictionary<string, string> _hazardToReportCode = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _lockedRecordKeys = new(StringComparer.OrdinalIgnoreCase);
    private string _activeRecordLockKey = string.Empty;
    private string _activeReportCode = string.Empty;
    private int _totalCount;

    protected override async Task OnInitializedAsync()
    {
        EventBusDispatcher.Register(this, _currentUserService?.UserCode ?? string.Empty, BuildCurrentSessionKey());
        HydrateLockedRecordKeys();
        await LoadInitialData();
    }

    private void HydrateLockedRecordKeys()
    {
        _lockedRecordKeys.Clear();
        foreach (var lockKey in RecordLockStateStore.GetLockedKeys())
        {
            _lockedRecordKeys.Add(lockKey);
        }
    }

    public void Dispose()
    {
        EventBusDispatcher.Unregister(this);
        _ = InvokeAsync(() => ReleaseActiveRecordLockAsync("NavigationAway"));
    }

    public async Task HandleEventAsync(NotificationSeverity severity, string title, string message, int duration)
    {
        if (!title.Contains(LiveUpdateEventNames.RecordEditLocked, StringComparison.OrdinalIgnoreCase)
            && !title.Contains(LiveUpdateEventNames.RecordEditReleased, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var lockKey = LiveUpdateRecordLockMessage.TryExtractLockKey(message);
        if (!LiveUpdateRecordLock.TryGetReportCode(lockKey, out _))
        {
            return;
        }

        await InvokeAsync(async () =>
        {
            if (title.Contains(LiveUpdateEventNames.RecordEditLocked, StringComparison.OrdinalIgnoreCase))
            {
                _lockedRecordKeys.Add(lockKey!);
            }
            else
            {
                _lockedRecordKeys.Remove(lockKey!);
                await LoadInitialData();
                if (_panelsGrid is not null)
                {
                    await _panelsGrid.Reload();
                }
            }

            StateHasChanged();
        });
    }

    private string BuildCurrentSessionKey()
    {
        return $"scoring-panel-listing:{_currentUserService?.UserCode}:{GetHashCode()}";
    }

    private bool IsRecordLocked(string? reportCode)
    {
        if (string.IsNullOrWhiteSpace(reportCode))
        {
            return false;
        }

        var lockKey = LiveUpdateRecordLock.BuildKey(reportCode);
        var isStoreLocked = RecordLockStateStore.IsLockKeyLocked(lockKey);
        if (!isStoreLocked)
        {
            _lockedRecordKeys.Remove(lockKey);
        }

        return _lockedRecordKeys.Contains(lockKey) || isStoreLocked;
    }

    private string? GetReportCodeForPanel(ScoringPanel? panel)
    {
        if (panel is null || string.IsNullOrWhiteSpace(panel.HazardCode))
        {
            return null;
        }

        return _hazardToReportCode.TryGetValue(panel.HazardCode, out var reportCode)
            ? reportCode
            : null;
    }

    private async Task<bool> TryAcquireRecordLockAsync(string? reportCode, string action)
    {
        if (string.IsNullOrWhiteSpace(reportCode))
        {
            await _notificationHelper.ShowWarningAsync("Unable to lock record graph because report code is missing.");
            return false;
        }

        var lockKey = LiveUpdateRecordLock.BuildKey(reportCode);
        if (_lockedRecordKeys.Contains(lockKey))
        {
            await _notificationHelper.ShowWarningAsync($"Report {reportCode} and related records are locked by another session.");
            return false;
        }

        _activeRecordLockKey = lockKey;
        _activeReportCode = reportCode;
        _lockedRecordKeys.Add(lockKey);
        await PublishRecordLockAsync(lockKey, true, action);
        return true;
    }

    private async Task ReleaseActiveRecordLockAsync(string action)
    {
        var lockKey = _activeRecordLockKey;
        if (string.IsNullOrWhiteSpace(lockKey) && !string.IsNullOrWhiteSpace(_activeReportCode))
        {
            lockKey = LiveUpdateRecordLock.BuildKey(_activeReportCode);
        }

        if (string.IsNullOrWhiteSpace(lockKey))
        {
            return;
        }

        _activeRecordLockKey = string.Empty;
        _activeReportCode = string.Empty;
        _lockedRecordKeys.Remove(lockKey);
        await PublishRecordLockAsync(lockKey, false, action);
    }

    private async Task PublishRecordLockAsync(string lockKey, bool isLocked, string action)
    {
        await _liveUpdatesPublisher.PublishToApplicationUsersAsync(new LiveUpdateEnvelope
        {
            EventName = isLocked ? LiveUpdateEventNames.RecordEditLocked : LiveUpdateEventNames.RecordEditReleased,
            SourceModule = nameof(ScoringPanelListing),
            OccurredOn = DateTime.Now,
            ActorUserCode = _currentUserService?.UserCode,
            Message = LiveUpdateRecordLockMessage.Build(lockKey, action),
            EntityType = nameof(ScoringPanel),
            EntityCode = lockKey
        });
    }

    private async Task LoadHazardToReportMapAsync()
    {
        _hazardToReportCode.Clear();
        var hazardsResult = await _mediator.SendAsync(new GetAllHazardsQuery(), CancellationToken.None);
        if (!hazardsResult.IsSuccess || hazardsResult.Value is null)
        {
            return;
        }

        foreach (var hazard in hazardsResult.Value)
        {
            if (!string.IsNullOrWhiteSpace(hazard.Code) && !string.IsNullOrWhiteSpace(hazard.ReportCode))
            {
                _hazardToReportCode[hazard.Code] = hazard.ReportCode;
            }
        }
    }

    private async Task LoadInitialData()
    {
        try
        {
            await LoadHazardToReportMapAsync();
            var query = new GetAllScoringPanelsQuery();
            var result = await _mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value is not null)
            {
                _panels = result.Value;
                _totalCount = _panels.Count();
                _logger.LogInformation("Loaded {Count} scoring panels", _totalCount);
            }
            else
            {
                await _notificationHelper.ShowErrorAsync("Failed to load scoring panels");
                _logger.LogError("Failed to load scoring panels: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading scoring panels");
            await _notificationHelper.ShowErrorAsync("Error loading scoring panels");
        }
    }

    private async Task LoadData(LoadDataArgs args)
    {
        try
        {
            await LoadInitialData();

            var query = _panels.AsQueryable();

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

            const int pageSize = 15;
            query = query.Take(pageSize);

            _panels = query.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in LoadData");
            await _notificationHelper.ShowErrorAsync("Error loading data");
        }
    }

    private static Expression<Func<ScoringPanel, object>> GetPropertyExpression(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(ScoringPanel), "x");
        var property = Expression.Property(parameter, propertyName);
        var conversion = Expression.Convert(property, typeof(object));
        return Expression.Lambda<Func<ScoringPanel, object>>(conversion, parameter);
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

    private async Task ShowActions(ScoringPanel panel)
    {
        var reportCode = GetReportCodeForPanel(panel);
        if (!await TryAcquireRecordLockAsync(reportCode, "ActionsOpen").ConfigureAwait(false))
        {
            return;
        }

        _logger.LogInformation("Actions requested for scoring panel: {Code}", panel.Code);
        await ReleaseActiveRecordLockAsync("ActionsClosed").ConfigureAwait(false);
    }
}
