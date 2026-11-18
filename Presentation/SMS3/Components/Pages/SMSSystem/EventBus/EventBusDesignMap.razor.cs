using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Radzen;
using SMS_Domain.Enums;
using SMS3.Components.Pages.SMSSystem.Services;

namespace SMS3.Components.Pages.SMSSystem.EventBus;

public partial class EventBusDesignMap
{
    [Inject] private ILogger<EventBusDesignMap> _logger { get; set; } = default!;
    [Inject] private IEventBusDesignMapService _eventBusDesignMapService { get; set; } = default!;

    private List<EventBusMapEventItem> _eventItems = [];
    private List<EventBusDesignTreeNode> _treeNodes = [];
    private bool _isLoading = true;
    private string _errorMessage = string.Empty;

    private int _domainEventCount => _eventItems.Count(x => string.Equals(x.Category, "DomainEvent", StringComparison.OrdinalIgnoreCase));
    private int _integrationEventCount => _eventItems.Count(x => string.Equals(x.Category, "IntegrationEvent", StringComparison.OrdinalIgnoreCase));
    private int _uiEventCount => _eventItems.Count(x => string.Equals(x.Category, "UIEvent", StringComparison.OrdinalIgnoreCase));
    private int _unusedEventCount => _eventItems.Count(x => x.IsUnused);

    protected override async Task OnInitializedAsync()
    {
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        try
        {
            _isLoading = true;
            _errorMessage = string.Empty;

            _eventItems = (await _eventBusDesignMapService.BuildMapAsync(CancellationToken.None))
                .ToList();

            BuildTreeNodes();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load EventBus design map dialog.");
            _errorMessage = "Failed to load EventBus map data.";
            _eventItems = [];
            _treeNodes = [];
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void BuildTreeNodes()
    {
        _treeNodes =
        [
            BuildCategoryNode("DomainEvent", _eventItems.Where(IsDomainEvent)),
            BuildCategoryNode("IntegrationEvent", _eventItems.Where(IsIntegrationEvent)),
            BuildCategoryNode("UIEvent", _eventItems.Where(IsUiEvent))
        ];
    }

    private static EventBusDesignTreeNode BuildCategoryNode(string categoryName, IEnumerable<EventBusMapEventItem> items)
    {
        var categoryItems = items
            .OrderBy(x => x.EventClassName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var children = categoryItems
            .Select(BuildEventNode)
            .ToList();

        return new EventBusDesignTreeNode
        {
            Title = $"{categoryName} ({categoryItems.Count})",
            NodeKind = EventBusTreeNodeKind.Category,
            IsUnused = false,
            Children = children
        };
    }

    private static EventBusDesignTreeNode BuildEventNode(EventBusMapEventItem item)
    {
        return new EventBusDesignTreeNode
        {
            Title = item.EventClassName,
            NodeKind = EventBusTreeNodeKind.Event,
            IsUnused = item.IsUnused,
            LifecycleState = item.LifecycleState,
            LifecycleNotes = item.LifecycleNotes,
            Children =
            [
                BuildSectionNode("Lifecycle Notes", [item.LifecycleNotes]),
                BuildSectionNode("Triggered At", item.TriggeredAt),
                BuildSectionNode("Handled By", item.HandledBy),
                BuildSectionNode("Chained To", item.ChainedTo),
                BuildSectionNode("Chained From", item.ChainedFrom),
                BuildSectionNode("Chain Flow", item.ChainFlow)
            ]
        };
    }

    private static EventBusDesignTreeNode BuildSectionNode(string sectionName, IReadOnlyList<string> values)
    {
        IReadOnlyList<EventBusDesignTreeNode> details;

        if (values.Count == 0)
        {
            details =
            [
                new EventBusDesignTreeNode
                {
                    Title = "None",
                    NodeKind = EventBusTreeNodeKind.Detail,
                    IsUnused = false,
                    Children = []
                }
            ];
        }
        else
        {
            details = values
                .Select(value => new EventBusDesignTreeNode
                {
                    Title = value,
                    NodeKind = EventBusTreeNodeKind.Detail,
                    IsUnused = false,
                    Children = []
                })
                .ToList();
        }

        return new EventBusDesignTreeNode
        {
            Title = $"{sectionName} ({values.Count})",
            NodeKind = EventBusTreeNodeKind.Section,
            IsUnused = false,
            Children = details
        };
    }

    private static bool IsDomainEvent(EventBusMapEventItem item)
    {
        return string.Equals(item.Category, "DomainEvent", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsIntegrationEvent(EventBusMapEventItem item)
    {
        return string.Equals(item.Category, "IntegrationEvent", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsUiEvent(EventBusMapEventItem item)
    {
        return string.Equals(item.Category, "UIEvent", StringComparison.OrdinalIgnoreCase);
    }

    protected static BadgeStyle GetLifecycleBadgeStyle(EventLifecycleState lifecycleState)
    {
        return lifecycleState.Value switch
        {
            "ACTIVE" => BadgeStyle.Success,
            "PLANNED_STUB" => BadgeStyle.Warning,
            "DEPRECATED" => BadgeStyle.Danger,
            _ => BadgeStyle.Secondary
        };
    }

    protected sealed class EventBusDesignTreeNode
    {
        public string Title { get; set; } = string.Empty;
        public EventBusTreeNodeKind NodeKind { get; set; }
        public bool IsUnused { get; set; }
        public EventLifecycleState? LifecycleState { get; set; }
        public string LifecycleNotes { get; set; } = string.Empty;
        public IReadOnlyList<EventBusDesignTreeNode> Children { get; set; } = [];
    }

    protected enum EventBusTreeNodeKind
    {
        Category,
        Event,
        Section,
        Detail
    }
}
